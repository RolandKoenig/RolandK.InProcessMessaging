using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RolandK.InProcessMessaging;

public interface IInProcessMessageSubscriber
{
    /// <summary>
    /// Subscribes to the given message type.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="actionOnMessage">Action to perform on incoming message.</param>
    MessageSubscription Subscribe(
        Delegate actionOnMessage, Type messageType);

    /// <summary>
    /// Subscribes to the given message type (using WeakReference).
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="actionOnMessage">Action to perform on incoming message.</param>
    MessageSubscription SubscribeWeak(
        Delegate actionOnMessage, Type messageType);

    /// <summary>
    /// Waits for the given message.
    /// </summary>
    Task<T> WaitForMessageAsync<T>();

    /// <summary>
    /// Clears the given MessageSubscription.
    /// </summary>
    /// <param name="messageSubscription">The subscription to clear.</param>
    void Unsubscribe(MessageSubscription messageSubscription);
}