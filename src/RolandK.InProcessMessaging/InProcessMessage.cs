namespace RolandK.InProcessMessaging;

/// <summary>
/// Base class of all messages sent and received through <see cref="InProcessMessenger"/> class.
/// </summary>
[InProcessMessage]
public record InProcessMessage
{
    /// <summary>
    /// Gets a list containing all target messengers for message routing.
    /// An empty list means that no routing logic applies.
    /// </summary>
    public string[] GetAsyncRoutingTargetMessengersOfMessageType()
    {
        return InProcessMessageMetadataHelper
            .GetAsyncRoutingTargetMessengersOfMessageType(this.GetType());
    }

    /// <summary>
    /// Gets a list containing all possible source messengers of this message.
    /// An empty list means that every messenger can fire this message
    /// </summary>
    public string[] GetPossibleSourceMessengersOfMessageType()
    {
        return InProcessMessageMetadataHelper
            .GetPossibleSourceMessengersOfMessageType(this.GetType());
    }
}