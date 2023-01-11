using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RolandK.InProcessMessaging.Checking;
using RolandK.InProcessMessaging.Exceptions;
using RolandK.InProcessMessaging.Extensions;

namespace RolandK.InProcessMessaging;

/// <summary>
/// Main class for sending/receiving messages.
/// This class follows the Messenger pattern but modifies it on some parts like thread synchronization and routing between more messengers.
/// What 'messenger' actually is, see here a short explanation:
/// http://stackoverflow.com/questions/22747954/mvvm-light-toolkit-messenger-uses-event-aggregator-or-mediator-pattern
/// </summary>
public class InProcessMessenger : IInProcessMessagePublisher, IInProcessMessageSubscriber
{
    public const string METHOD_NAME_MESSAGE_RECEIVED = "OnMessageReceived";

    /// <summary>
    /// Gets or sets a custom exception handler which is used globally.
    /// Return true to skip default exception behavior (exception is thrown to publisher by default).
    /// </summary>
    public static Func<InProcessMessenger, Exception, bool>? CustomPublishExceptionHandler;

    // Global synchronization (enables the possibility to publish a message over more messengers / areas of the application)
    private static ConcurrentDictionary<string, InProcessMessenger> s_messengersByName;

    // Global information about message routing
    private static ConcurrentDictionary<Type, string[]> s_messagesAsyncTargets;
    private static ConcurrentDictionary<Type, string[]> s_messageSources;

    // Checking and global synchronization
    private string _globalMessengerName;
    private SynchronizationContext? _hostSyncContext;
    private InProcessMessengerThreadingBehavior _publishCheckBehavior;

    // Message subscriptions
    private Dictionary<Type, List<MessageSubscription>> _messageSubscriptions;
    private object _messageSubscriptionsLock;

    /// <summary>
    /// Gets or sets the synchronization context on which to publish all messages.
    /// </summary>
    public SynchronizationContext? HostSyncContext
    {
        get { return _hostSyncContext; }
    }

    /// <summary>
    /// Gets the current threading behavior of this Messenger.
    /// </summary>
    public InProcessMessengerThreadingBehavior ThreadingBehavior
    {
        get { return _publishCheckBehavior; }
    }

    /// <summary>
    /// Gets the name of this messenger.
    /// </summary>
    public string MessengerName
    {
        get { return _globalMessengerName; }
    }

    public bool IsConnectedToGlobalMessaging
    {
        get => !string.IsNullOrEmpty(_globalMessengerName);
    }

    /// <summary>
    /// Counts all message subscriptions.
    /// </summary>
    public int CountSubscriptions
    {
        get
        {
            lock (_messageSubscriptionsLock)
            {
                var totalCount = 0;
                foreach (var actKeyValuePair in _messageSubscriptions)
                {
                    totalCount += actKeyValuePair.Value.Count;
                }
                return totalCount;
            }
        }
    }

    /// <summary>
    /// Custom compare function for <see cref="SynchronizationContext"/>
    /// This was implemented for WPF, because there the SynchronizationContext object differs
    /// </summary>
    public Func<SynchronizationContext, SynchronizationContext, bool>? CustomSynchronizationContextEqualityChecker { get; set; }

    /// <summary>
    /// Gets the total count of globally registered messengers.
    /// </summary>
    public static int CountGlobalMessengers
    {
        get { return s_messengersByName.Count; }
    }

    /// <summary>
    /// Initializes the <see cref="InProcessMessenger" /> class.
    /// </summary>
    static InProcessMessenger()
    {
        s_messengersByName = new ConcurrentDictionary<string, InProcessMessenger>();
        s_messagesAsyncTargets = new ConcurrentDictionary<Type, string[]>();
        s_messageSources = new ConcurrentDictionary<Type, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InProcessMessenger"/> class.
    /// </summary>
    public InProcessMessenger()
    {
        _globalMessengerName = string.Empty;
        _hostSyncContext = null;
        _publishCheckBehavior = InProcessMessengerThreadingBehavior.Ignore;

        _messageSubscriptions = new Dictionary<Type, List<MessageSubscription>>();
        _messageSubscriptionsLock = new object();
    }

    /// <summary>
    /// Gets the <see cref="InProcessMessenger"/> by the given name.
    /// </summary>
    /// <param name="messengerName">The name of the messenger.</param>
    public static InProcessMessenger GetByName(string messengerName)
    {
        messengerName.EnsureNotNullOrEmpty(nameof(messengerName));

        var result = TryGetByName(messengerName);
        if (result == null) { throw new InProcessMessagingCheckException($"Unable to find Messenger {messengerName}!"); }
        return result;
    }

    /// <summary>
    /// Gets the <see cref="InProcessMessenger"/> by the given name.
    /// </summary>
    /// <param name="messengerName">The name of the messenger.</param>
    public static InProcessMessenger? TryGetByName(string messengerName)
    {
        messengerName.EnsureNotNullOrEmpty(nameof(messengerName));

        s_messengersByName.TryGetValue(messengerName, out var result);
        return result;
    }

    /// <summary>
    /// Sets all required synchronization properties.
    /// </summary>
    /// <param name="checkBehavior">Defines the checking behavior for publish calls.</param>
    /// <param name="messengerName">The name by which this messenger should be registered.</param>
    /// <param name="hostSyncContext">The synchronization context to be used.</param>
    public void ConnectToGlobalMessaging(InProcessMessengerThreadingBehavior checkBehavior, string messengerName, SynchronizationContext? hostSyncContext)
    {
        messengerName.EnsureNotNullOrEmpty(nameof(messengerName));

        if (!string.IsNullOrEmpty(_globalMessengerName))
        {
            throw new InProcessMessagingCheckException($"This messenger is already registered as '{_globalMessengerName}'!");
        }
        if (s_messengersByName.ContainsKey(messengerName))
        {
            throw new InProcessMessagingCheckException($"The name '{messengerName}' is already in use by another messenger!");
        }

        _globalMessengerName = messengerName;
        _publishCheckBehavior = checkBehavior;
        _hostSyncContext = hostSyncContext;

        if (!string.IsNullOrEmpty(messengerName))
        {
            s_messengersByName.TryAdd(messengerName, this);
        }
    }

    /// <summary>
    /// Disconnects all <see cref="InProcessMessenger"/> from global messaging.
    /// </summary>
    public static void DisconnectGlobalMessagingConnections()
    {
        var allGlobalMessengers = s_messengersByName.Values.ToArray();
        foreach (var actMessenger in allGlobalMessengers)
        {
            actMessenger.DisconnectFromGlobalMessaging();
        }
    }

    /// <summary>
    /// Clears all synchronization configuration.
    /// </summary>
    public void DisconnectFromGlobalMessaging()
    {
        if (string.IsNullOrEmpty(_globalMessengerName)) { return; }

        s_messengersByName.TryRemove(_globalMessengerName, out _);

        _publishCheckBehavior = InProcessMessengerThreadingBehavior.Ignore;
        _globalMessengerName = string.Empty;
        _hostSyncContext = null;
    }

    /// <summary>
    /// Gets a collection containing all active subscriptions.
    /// </summary>
    public List<MessageSubscription> GetAllSubscriptions()
    {
        List<MessageSubscription> result = new();

        lock (_messageSubscriptionsLock)
        {
            foreach (var actPair in _messageSubscriptions)
            {
                foreach(var actSubscription in actPair.Value)
                {
                    result.Add(actSubscription);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Waits for the given message.
    /// </summary>
    public Task<T> WaitForMessageAsync<T>()
    {
        TaskCompletionSource<T> taskComplSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        MessageSubscription? subscription = null;
        subscription = this.Subscribe<T>((message) =>
        {
            // Unsubscribe first
            // ReSharper disable once AccessToModifiedClosure
            subscription!.Unsubscribe();

            // Set the task's result
            taskComplSource.SetResult(message);
        });

        return taskComplSource.Task;
    }

    /// <summary>
    /// Subscribes all receiver-methods of the given target object to this Messenger.
    /// The messages have to be called OnMessageReceived.
    /// </summary>
    /// <param name="targetObject">The target object which is to subscribe.</param>
    public IEnumerable<MessageSubscription> SubscribeAll(object targetObject)
    {
        targetObject.EnsureNotNull(nameof(targetObject));

        Type targetObjectType = targetObject.GetType();
        List<MessageSubscription> generatedSubscriptions = new(16);
        try
        {
            foreach (MethodInfo actMethod in targetObjectType.GetMethods(
                         BindingFlags.NonPublic | BindingFlags.Public | 
                         BindingFlags.Instance | BindingFlags.InvokeMethod))
            {
                if (!actMethod.Name.Equals(METHOD_NAME_MESSAGE_RECEIVED)) { continue; }

                ParameterInfo[] parameters = actMethod.GetParameters();
                if (parameters.Length != 1) { continue; }

                if (!InProcessMessageMetadataHelper.ValidateMessageType(parameters[0].ParameterType, out _))
                {
                    continue;
                }

                Type genericAction = typeof(Action<>);
                Type delegateType = genericAction.MakeGenericType(parameters[0].ParameterType);
                generatedSubscriptions.Add(this.Subscribe(
                    actMethod.CreateDelegate(delegateType, targetObject),
                    parameters[0].ParameterType));
            }
        }
        catch(Exception)
        {
            foreach(MessageSubscription actSubscription in generatedSubscriptions)
            {
                actSubscription.Unsubscribe();
            }
            generatedSubscriptions.Clear();
        }

        return generatedSubscriptions;
    }

    /// <summary>
    /// Subscribes to the given MessageType.
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="actionOnMessage">Action to perform on incoming message.</param>
    public MessageSubscription Subscribe<TMessageType>(Action<TMessageType> actionOnMessage)
    {
        actionOnMessage.EnsureNotNull(nameof(actionOnMessage));

        Type currentType = typeof(TMessageType);
        return this.Subscribe(actionOnMessage, currentType);
    }

    /// <summary>
    /// Subscribes to the given message type.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="actionOnMessage">Action to perform on incoming message.</param>
    public MessageSubscription Subscribe(
        Delegate actionOnMessage, Type messageType)
    {
        actionOnMessage.EnsureNotNull(nameof(actionOnMessage));
        messageType.EnsureNotNull(nameof(messageType));

        InProcessMessageMetadataHelper.EnsureValidMessageType(messageType);

        MessageSubscription newOne = new(this, messageType, actionOnMessage);
        lock (_messageSubscriptionsLock)
        {
            if (_messageSubscriptions.ContainsKey(messageType))
            {
                _messageSubscriptions[messageType].Add(newOne);
            }
            else
            {
                List<MessageSubscription> newList = new();
                newList.Add(newOne);
                _messageSubscriptions[messageType] = newList;
            }
        }

        return newOne;
    }

    /// <summary>
    /// Clears the given MessageSubscription.
    /// </summary>
    /// <param name="messageSubscription">The subscription to clear.</param>
    public void Unsubscribe(MessageSubscription messageSubscription)
    {
        messageSubscription.EnsureNotNull(nameof(messageSubscription));

        if (!messageSubscription.IsDisposed)
        {
            Type messageType = messageSubscription.MessageType;

            // Remove subscription from internal list
            lock (_messageSubscriptionsLock)
            {
                if (_messageSubscriptions.ContainsKey(messageType))
                {
                    List<MessageSubscription> listOfSubscriptions = _messageSubscriptions[messageType];
                    listOfSubscriptions.Remove(messageSubscription);
                    if (listOfSubscriptions.Count == 0)
                    {
                        _messageSubscriptions.Remove(messageType);
                    }
                }
            }

            // Clear given subscription
            messageSubscription.Clear();
        }
    }

    /// <summary>
    /// Counts all message subscriptions for the given message type.
    /// </summary>
    /// <typeparam name="TMessageType">The type of the message for which to count all subscriptions.</typeparam>
    public int CountSubscriptionsForMessage<TMessageType>()
    {
        lock (_messageSubscriptionsLock)
        {
            if (_messageSubscriptions.TryGetValue(typeof(TMessageType), out var subscriptions))
            {
                return subscriptions.Count;
            }
            else
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// There is no possibility here to wait for the answer.
    /// </summary>
    public void BeginPublish<TMessageType>()
        where TMessageType : new()
    {
        this.BeginPublish(new TMessageType());
    }

    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// There is no possibility here to wait for the answer.
    /// </summary>
    /// <typeparam name="TMessageType">The type of the message type.</typeparam>
    /// <param name="message">The message.</param>
    public void BeginPublish<TMessageType>(
        TMessageType message)
    {
        _hostSyncContext.PostAlsoIfNull(() => this.Publish(message));
    }

    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// The returned task waits for all synchronous subscriptions.
    /// </summary>
    /// <typeparam name="TMessageType">The type of the message.</typeparam>
    public Task BeginPublishAsync<TMessageType>()
        where TMessageType : new()
    {
        return _hostSyncContext.PostAlsoIfNullAsync(
            this.Publish<TMessageType>);
    }

    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// The returned task waits for all synchronous subscriptions.
    /// </summary>
    /// <typeparam name="TMessageType">The type of the message.</typeparam>
    /// <param name="message">The message to be sent.</param>
    public Task BeginPublishAsync<TMessageType>(
        TMessageType message)
    {
        return _hostSyncContext.PostAlsoIfNullAsync(
            () => this.Publish(message));
    }

    /// <summary>
    /// Sends the given message to all subscribers (sync processing).
    /// </summary>
    public void Publish<TMessageType>()
        where TMessageType : new()
    {
        this.Publish(new TMessageType());
    }

    /// <summary>
    /// Sends the given message to all subscribers (sync processing).
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="message">The message to send.</param>
    public void Publish<TMessageType>(
        TMessageType message)
    {
        this.PublishInternal(
            message, true);
    }

    /// <summary>
    /// Sends the given message to all subscribers (sync processing).
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <param name="isInitialCall">Is this one the initial call to publish? (false if we are coming from async routing)</param>
    private void PublishInternal<TMessageType>(
        TMessageType message, bool isInitialCall)
    {
        InProcessMessageMetadataHelper.EnsureValidMessageTypeAndValue(message);

        try
        {
            // Check whether publish is possible
            if(_publishCheckBehavior == InProcessMessengerThreadingBehavior.EnsureCorrectSyncContextOnSyncCalls)
            {
                if (!this.CompareSynchronizationContexts())
                {
                    throw new InProcessMessagingCheckException(
                        "Unable to perform a synchronous publish call because current " +
                        "SynchronizationContext is set wrong. This indicates that the call " +
                        "comes from a wrong thread!");
                }
            }

            // Check for correct message sources
            Type currentType = typeof(TMessageType);
            if (isInitialCall)
            {
                string[] possibleSources = s_messageSources.GetOrAdd(
                    currentType, 
                    _ => InProcessMessageMetadataHelper.GetPossibleSourceMessengersOfMessageType(currentType));
                if (possibleSources.Length > 0)
                {
                    string mainThreadName = _globalMessengerName;
                    if (string.IsNullOrEmpty(mainThreadName) ||
                        (Array.IndexOf(possibleSources, mainThreadName) < 0))
                    {
                        throw new InvalidOperationException(
                            $"The message of type {currentType.FullName} can only be sent " +
                            $"by the threads [{string.Join(", ", possibleSources)}]. This Messenger " +
                            $"belongs to the thread {(!string.IsNullOrEmpty(mainThreadName) ? mainThreadName : "(empty)")}, " +
                            "so no publish possible!");
                    }
                }
            }

            // Perform synchronous message handling
            List<MessageSubscription> subscriptionsToTrigger = new(20);
            lock (_messageSubscriptionsLock)
            {
                if (_messageSubscriptions.ContainsKey(currentType))
                {
                    // Need to copy the list to avoid problems, when the list is changed during the loop and cross thread accesses
                    subscriptionsToTrigger = new List<MessageSubscription>(_messageSubscriptions[currentType]);
                }
            }

            // Trigger all found subscriptions
            List<Exception>? occurredExceptions = null;
            for (var loop = 0; loop < subscriptionsToTrigger.Count; loop++)
            {
                try
                {
                    subscriptionsToTrigger[loop].Publish(message);
                }
                catch (Exception ex)
                {
                    occurredExceptions ??= new List<Exception>();
                    occurredExceptions.Add(ex);
                }
            }

            // Perform further message routing if enabled
            if (isInitialCall)
            {
                // Get information about message routing
                var asyncTargets = s_messagesAsyncTargets.GetOrAdd(currentType, (_) => InProcessMessageMetadataHelper.GetAsyncRoutingTargetMessengersOfMessageType(currentType));
                var mainMessengerName = _globalMessengerName;
                for (var loop = 0; loop < asyncTargets.Length; loop++)
                {
                    var actAsyncTargetName = asyncTargets[loop];
                    if (mainMessengerName == actAsyncTargetName) { continue; }

                    if (s_messengersByName.TryGetValue(actAsyncTargetName, out var actAsyncTargetHandler))
                    {
                        var actSyncContext = actAsyncTargetHandler!._hostSyncContext;
                        if (actSyncContext == null) { continue; }

                        var innerHandlerForAsyncCall = actAsyncTargetHandler;
                        actSyncContext.PostAlsoIfNull(() =>
                        {
                            innerHandlerForAsyncCall.PublishInternal(message, false);
                        });
                    }
                }
            }

            // Notify all exceptions occurred during publish progress
            if (isInitialCall)
            {
                if (occurredExceptions is {Count: > 0})
                {
                    throw new MessagePublishException(typeof(TMessageType), occurredExceptions);
                }
            }
        }
        catch (Exception ex)
        {
            // Check whether we have to throw the exception globally
            var globalExceptionHandler = CustomPublishExceptionHandler;
            var doRaise = true;
            if (globalExceptionHandler != null)
            {
                try
                {
                    doRaise = !globalExceptionHandler(this, ex);
                }
                catch 
                {
                    doRaise = true;
                }
            }

            // Raise the exception to inform caller about it
            if (doRaise) { throw; }
        }
    }

    /// <summary>
    /// Compares the SynchronizationContext of the current thread and of this messenger instance.
    /// </summary>
    private bool CompareSynchronizationContexts()
    {
        var currentSynchronizationContext = SynchronizationContext.Current;
        if((currentSynchronizationContext != null) && (_hostSyncContext != null))
        {
            var syncContextEqualityChecker = this.CustomSynchronizationContextEqualityChecker;
            if (syncContextEqualityChecker != null)
            {
                return syncContextEqualityChecker(currentSynchronizationContext, _hostSyncContext);
            }
            else
            {
                return SynchronizationContext.Current == _hostSyncContext;
            }
        }
        else if((currentSynchronizationContext != null) || (_hostSyncContext != null))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}