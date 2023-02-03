namespace RolandK.InProcessMessaging;

public interface IInProcessMessageHandler<in TMessageType>
{
    /// <summary>
    /// Handles the given message.
    /// </summary>
    /// <param name="message">The message to be handled.</param>
    void OnMessageReceived(TMessageType message);
}