using System;
using System.Collections.Generic;
using System.Reflection;
using RolandK.InProcessMessaging.Checking;

namespace RolandK.InProcessMessaging;

public static class InProcessMessageSubscriberExtensions
{
    /// <summary>
    /// Subscribes to the given MessageType.
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe to the given message type.</param>
    /// <param name="actionOnMessage">Action to perform on incoming message.</param>
    public static MessageSubscription Subscribe<TMessageType>(
        this IInProcessMessageSubscriber subscriber,
        Action<TMessageType> actionOnMessage)
    {
        actionOnMessage.EnsureNotNull(nameof(actionOnMessage));

        var currentType = typeof(TMessageType);
        return subscriber.Subscribe(actionOnMessage, currentType);
    }

    /// <summary>
    /// Subscribes to the given MessageType.
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe to the given message type.</param>
    /// <param name="messageHandler">The target object which processes the message..</param>
    public static MessageSubscription Subscribe<TMessageType>(
        this IInProcessMessageSubscriber subscriber,
        IInProcessMessageHandler<TMessageType> messageHandler)
    {
        messageHandler.EnsureNotNull(nameof(messageHandler));
        
        var currentType = typeof(TMessageType);
        return subscriber.Subscribe(messageHandler.OnMessageReceived, currentType);
    }
    
    /// <summary>
    /// Subscribes to the given MessageType (using WeakReference).
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe to the given message type.</param>
    /// <param name="actionOnMessage">Action to perform on incoming message.</param>
    public static MessageSubscription SubscribeWeak<TMessageType>(
        this IInProcessMessageSubscriber subscriber,
        Action<TMessageType> actionOnMessage)
    {
        actionOnMessage.EnsureNotNull(nameof(actionOnMessage));

        var currentType = typeof(TMessageType);
        return subscriber.SubscribeWeak(actionOnMessage, currentType);
    }
    
    /// <summary>
    /// Subscribes to the given MessageType (using WeakReference).
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe to the given message type.</param>
    /// <param name="messageHandler">The target object which processes the message..</param>
    public static MessageSubscription SubscribeWeak<TMessageType>(
        this IInProcessMessageSubscriber subscriber,
        IInProcessMessageHandler<TMessageType> messageHandler)
    {
        messageHandler.EnsureNotNull(nameof(messageHandler));
        
        var currentType = typeof(TMessageType);
        return subscriber.SubscribeWeak(messageHandler.OnMessageReceived, currentType);
    }
    
    /// <summary>
    /// Subscribes all receiver-methods of the given target object to this Messenger.
    /// The methods have to be called OnMessageReceived.
    /// Example: void OnMessageReceived(TMessageType message)
    /// </summary>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe all defined messages.</param>
    /// <param name="targetObject">The target object which is to subscribe.</param>
    public static IEnumerable<MessageSubscription> SubscribeAll(
        this IInProcessMessageSubscriber subscriber,
        object targetObject)
    {
        return subscriber.SubscribeAllInternal(targetObject, false);
    }
    
    /// <summary>
    /// Subscribes all receiver-methods of the given target object to this Messenger.
    /// The methods have to be called OnMessageReceived.
    /// Example: void OnMessageReceived(TMessageType message)
    /// </summary>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe all defined messages.</param>
    /// <param name="targetObject">The target object which is to subscribe.</param>
    public static IEnumerable<MessageSubscription> SubscribeAllWeak(
        this IInProcessMessageSubscriber subscriber,
        object targetObject)
    {
        return subscriber.SubscribeAllInternal(targetObject, true);
    }
    
    /// <summary>
    /// Subscribes all receiver-methods of the given target object to this Messenger.
    /// The methods have to be called OnMessageReceived.
    /// Example: void OnMessageReceived(TMessageType message)
    /// </summary>
    /// <param name="subscriber">The <see cref="IInProcessMessageSubscriber"/> on which to subscribe all defined messages.</param>
    /// <param name="targetObject">The target object which is to subscribe.</param>
    /// <param name="useWeakReferences">Use weak references on subscriptions?</param>
    private static IEnumerable<MessageSubscription> SubscribeAllInternal(
        this IInProcessMessageSubscriber subscriber,
        object targetObject,
        bool useWeakReferences)
    {
        targetObject.EnsureNotNull(nameof(targetObject));

        var targetObjectType = targetObject.GetType();
        var generatedSubscriptions = new List<MessageSubscription>(16);
        try
        {
            foreach (var actMethod in targetObjectType.GetMethods(
                         BindingFlags.NonPublic | BindingFlags.Public | 
                         BindingFlags.Instance | BindingFlags.InvokeMethod))
            {
                if (!actMethod.Name.Equals(InProcessMessenger.METHOD_NAME_MESSAGE_RECEIVED)) { continue; }

                var parameters = actMethod.GetParameters();
                if (parameters.Length != 1) { continue; }

                if (!InProcessMessageMetadataHelper.ValidateMessageType(parameters[0].ParameterType, out _))
                {
                    continue;
                }

                var genericAction = typeof(Action<>);
                var delegateType = genericAction.MakeGenericType(parameters[0].ParameterType);
                var targetDelegate = actMethod.CreateDelegate(delegateType, targetObject);

                if (useWeakReferences)
                {
                    generatedSubscriptions.Add(subscriber.SubscribeWeak(
                        targetDelegate,
                        parameters[0].ParameterType));
                }
                else
                {
                    generatedSubscriptions.Add(subscriber.Subscribe(
                        targetDelegate,
                        parameters[0].ParameterType));
                }
            }
        }
        catch(Exception)
        {
            foreach(var actSubscription in generatedSubscriptions)
            {
                actSubscription.Unsubscribe();
            }
            generatedSubscriptions.Clear();
        }

        return generatedSubscriptions;
    }
}