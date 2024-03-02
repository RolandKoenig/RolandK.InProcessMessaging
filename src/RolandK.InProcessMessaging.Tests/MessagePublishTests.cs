namespace RolandK.InProcessMessaging.Tests;

public class MessagePublishTests
{
    [Fact]
    public async Task Publish_MessageObject()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        using var subscription = messenger.Subscribe<DummyMessage>(_ => { });

        var messageWaitTask = messenger.WaitForMessageAsync<DummyMessage>(CancellationToken.None);

        messenger.Publish(new DummyMessage());

        await Task.WhenAny(
            Task.Delay(5000),
            messageWaitTask);
        
        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, messageWaitTask.Status);
    }

    [Fact]
    public async Task Publish_Generic()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        using var subscription = messenger.Subscribe<DummyMessage>(_ => { });

        var messageWaitTask = messenger.WaitForMessageAsync<DummyMessage>(CancellationToken.None);

        messenger.Publish<DummyMessage>();

        await Task.WhenAny(
            Task.Delay(5000),
            messageWaitTask);
        
        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, messageWaitTask.Status);
    }

    [Fact]
    public async Task BeginPublish_MessageObject()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        using var subscription = messenger.Subscribe<DummyMessage>(_ => { });

        var messageWaitTask = messenger.WaitForMessageAsync<DummyMessage>(CancellationToken.None);

        messenger.BeginPublish(new DummyMessage());

        await Task.WhenAny(
            Task.Delay(5000),
            messageWaitTask);
        
        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, messageWaitTask.Status);
    }

    [Fact]
    public async Task BeginPublish_Generic()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        using var subscription = messenger.Subscribe<DummyMessage>(_ => { });

        var messageWaitTask = messenger.WaitForMessageAsync<DummyMessage>(CancellationToken.None);

        messenger.BeginPublish<DummyMessage>();

        await Task.WhenAny(
            Task.Delay(5000),
            messageWaitTask);
        
        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, messageWaitTask.Status);
    }

    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private record DummyMessage;
}
