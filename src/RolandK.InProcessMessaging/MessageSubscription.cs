using System;
using System.Reflection;

namespace RolandK.InProcessMessaging;

/// <summary>
/// This class holds all information about a message subscriptions. It implements IDisposable for unsubscribing
/// from the <see cref="InProcessMessenger"/>.
/// </summary>
public sealed class MessageSubscription : IDisposable
{
    // Main members for publishing
    private InProcessMessenger? _messenger;
    private Type? _messageType;
    
    // Members for normal reference to target
    private Delegate? _targetHandler;

    // Members for weak reference to target
    private WeakReference? _weakTargetObject;
    private MethodInfo? _weakTargetMethod;

    /// <summary>
    /// Is this subscription valid?
    /// </summary>
    public bool IsDisposed =>
        (_messenger == null) ||
        (_messageType == null);

    /// <summary>
    /// Gets the corresponding Messenger object.
    /// </summary>
    public InProcessMessenger Messenger => _messenger ?? throw new ObjectDisposedException(nameof(MessageSubscription));

    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    public Type MessageType => _messageType ?? throw new ObjectDisposedException(nameof(MessageSubscription));

    /// <summary>
    /// Gets the name of the message type.
    /// </summary>
    public string MessageTypeName => _messageType?.Name ?? throw new ObjectDisposedException(nameof(MessageSubscription));

    /// <summary>
    /// Gets the name of the target object.
    /// </summary>
    public object TargetObject
    {
        get
        {
            if (_targetHandler != null) { return _targetHandler.Target; }

            var weakTarget = _weakTargetObject?.Target;
            if (weakTarget != null) { return weakTarget; }

            throw new ObjectDisposedException(nameof(MessageSubscription));
        }
    }

    /// <summary>
    /// Gets the name of the target method.
    /// </summary>
    public string TargetMethodName
    {
        get
        {
            if (_targetHandler != null) { return _targetHandler.Method.Name; }

            if (_weakTargetMethod != null) { return _weakTargetMethod.Name; }
            
            throw new ObjectDisposedException(nameof(MessageSubscription));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSubscription"/> class.
    /// This subscription keeps the target object alive.
    /// </summary>
    /// <param name="messenger">The messenger.</param>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="targetHandler">The target handler.</param>
    internal MessageSubscription(
        InProcessMessenger messenger, Type messageType, 
        Delegate targetHandler)
    {
        _messenger = messenger;
        _messageType = messageType;
        _targetHandler = targetHandler;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageSubscription"/> class.
    /// This subscription does not keep the target object alive.
    /// </summary>
    /// <param name="messenger">The messenger.</param>
    /// <param name="messageType">Type of the message.</param>
    /// <param name="weakTargetObject">A <see cref="WeakReference"/> to the target object.</param>
    /// <param name="weakTargetMethod">The method to call on the target object.</param>
    internal MessageSubscription(
        InProcessMessenger messenger, Type messageType, 
        WeakReference weakTargetObject, MethodInfo weakTargetMethod)
    {
        _messenger = messenger;
        _messageType = messageType;
        _weakTargetObject = weakTargetObject;
        _weakTargetMethod = weakTargetMethod;
    }

    /// <summary>
    /// Unsubscribes this subscription.
    /// </summary>
    public void Unsubscribe()
    {
        this.Dispose();
    }

    /// <summary>
    /// Sends the given message to the target.
    /// </summary>
    /// <typeparam name="TMessageType">Type of the message.</typeparam>
    /// <param name="message">The message to be published.</param>
    internal void Publish<TMessageType>(TMessageType message)
    {
        // Call this subscription
        if (!this.IsDisposed)
        {
            if (_targetHandler != null)
            {
                _targetHandler.DynamicInvoke(message!);
            }
            else if ((_weakTargetObject != null) &&
                     (_weakTargetMethod != null))
            {
                var targetObject = _weakTargetObject.Target;
                if (targetObject != null)
                {
                    _weakTargetMethod.Invoke(targetObject, new object[] { message! });
                }
                else
                {
                    this.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// Clears this message.
    /// </summary>
    internal void Clear()
    {
        _messenger = null;
        _messageType = null;
        _targetHandler = null;
        _weakTargetMethod = null;
        _weakTargetObject = null;
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public void Dispose()
    {
        if (!this.IsDisposed)
        {
            _messenger?.Unsubscribe(this);
        }
    }
}