using System;

namespace RolandK.InProcessMessaging;

public class InProcessMessageSource<TMessageType>
{
    private Action<TMessageType>? _customTarget;

    public string SourceMessengerName { get; }

    public InProcessMessageSource(string sourceMessengerName)
    {
        this.SourceMessengerName = sourceMessengerName;
    }

    /// <summary>
    /// Attach a custom handler here to avoid calling a globally registered <see cref="InProcessMessenger"/>.
    /// </summary>
    /// <param name="customTarget">A custom message target.</param>
    public void UnitTesting_ReplaceByCustomMessageTarget(Action<TMessageType> customTarget)
    {
        _customTarget = customTarget;
    }

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
}