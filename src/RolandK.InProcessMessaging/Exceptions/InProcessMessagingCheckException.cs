using System;

namespace RolandK.InProcessMessaging.Exceptions;

public class InProcessMessagingCheckException : Exception
{
    public InProcessMessagingCheckException(string message)
        : base(message)
    {

    }

    public InProcessMessagingCheckException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
