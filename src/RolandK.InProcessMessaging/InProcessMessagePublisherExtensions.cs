using System.Threading.Tasks;

namespace RolandK.InProcessMessaging;

public static class InProcessMessagePublisherExtensions
{
    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// There is no possibility here to wait for the answer.
    /// </summary>
    public static void BeginPublish<TMessageType>(this IInProcessMessagePublisher messagePublisher)
        where TMessageType : new ()
    {
        messagePublisher.BeginPublish(new TMessageType());
    }

    /// <summary>
    /// Sends the given message to all subscribers (async processing).
    /// The returned task waits for all synchronous subscriptions.
    /// </summary>
    /// <typeparam name="TMessageType">The type of the message.</typeparam>
    public static Task BeginPublishAsync<TMessageType>(this IInProcessMessagePublisher messagePublisher)
        where TMessageType : new()
    {
        return messagePublisher.BeginPublishAsync(new TMessageType());
    }

    /// <summary>
    /// Sends the given message to all subscribers (sync processing).
    /// </summary>
    public static void Publish<TMessageType>(this IInProcessMessagePublisher messagePublisher)
        where TMessageType : new()
    {
        messagePublisher.Publish(new TMessageType());
    }
}
