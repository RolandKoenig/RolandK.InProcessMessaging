using System;
using System.Threading.Tasks;

namespace RolandK.InProcessMessaging;

/// <summary>
/// A source for messages which calls a globally connected messenger.
/// </summary>
/// <typeparam name="TMessageType">The type of message published from this source.</typeparam>
public class InProcessMessageSource<TMessageType>
{
    private Action<TMessageType>? _customTarget;

    public string SourceMessengerName { get; }

    /// <summary>
    /// Creates a new <see cref="InProcessMessageSource{TMessageType}"/>
    /// </summary>
    /// <param name="sourceMessengerName">The name of the globally connected messenger to call on publish.</param>
    public InProcessMessageSource(string sourceMessengerName)
    {
        this.SourceMessengerName = sourceMessengerName;
    }

    /// <summary>
    /// Attach a custom handler here to avoid calling a globally registered <see cref="InProcessMessenger"/>.
    /// This method is meant for unittesting to mock subscribers.
    /// </summary>
    /// <param name="customTarget">A custom message target.</param>
    public void UnitTesting_ReplaceByCustomMessageTarget(Action<TMessageType> customTarget)
    {
        _customTarget = customTarget;
    }

    /// <summary>
    /// Sends the given message to all subscribers (sync processing).
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void Publish(TMessageType message)
    {
        if(_customTarget != null)
        {
            _customTarget(message);
            return;
        }

        InProcessMessenger.GetByName(this.SourceMessengerName)
            .Publish(message);
    }

    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// There is no possibility here to wait for the answer.
    /// </summary>
    /// <param name="message">The message.</param>
    public void BeginPublish(TMessageType message)
    {
        if(_customTarget != null)
        {
            _customTarget(message);
            return;
        }

        InProcessMessenger.GetByName(this.SourceMessengerName)
            .BeginPublish(message);
    }
    
    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// The returned task waits for all synchronous subscriptions.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    public Task BeginPublishAsync(TMessageType message)
    {
        if(_customTarget != null)
        {
            _customTarget(message);
            return Task.CompletedTask;
        }

        return InProcessMessenger.GetByName(this.SourceMessengerName)
            .BeginPublishAsync(message);
    }
}