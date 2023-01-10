using System;

namespace RolandK.InProcessMessaging;

public class MessagePossibleSourceAttribute : Attribute
{
    public string[] PossibleSourceMessengers { get; }

    public MessagePossibleSourceAttribute(params string[] possibleSourceMessengers)
    {
        this.PossibleSourceMessengers = possibleSourceMessengers;
    }
}