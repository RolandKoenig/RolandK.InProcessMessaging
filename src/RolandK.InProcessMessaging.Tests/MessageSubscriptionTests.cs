namespace RolandK.InProcessMessaging.Tests;

[Collection(TestConstants.SINGLE_TEST_COLLECTION_NAME)]
public class MessageSubscriptionTests
{
    [Fact]
    public void DisposeExceptionsAfterDispose()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        using var subscription = messenger.Subscribe<DummyMessage>(_ => { });
        subscription.Dispose();
        
        // Assert
        Assert.True(subscription.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => subscription.TargetObject);
        Assert.Throws<ObjectDisposedException>(() => subscription.TargetMethodName);
        Assert.Throws<ObjectDisposedException>(() => subscription.MessageType);
        Assert.Throws<ObjectDisposedException>(() => subscription.MessageTypeName);
        Assert.Throws<ObjectDisposedException>(() => subscription.Messenger);
    }
    
    [Fact]
    public void DisposeExceptionsAfterUnsubscribe()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        using var subscription = messenger.Subscribe<DummyMessage>(_ => { });
        subscription.Unsubscribe();
        
        // Assert
        Assert.True(subscription.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => subscription.TargetObject);
        Assert.Throws<ObjectDisposedException>(() => subscription.TargetMethodName);
        Assert.Throws<ObjectDisposedException>(() => subscription.MessageType);
        Assert.Throws<ObjectDisposedException>(() => subscription.MessageTypeName);
        Assert.Throws<ObjectDisposedException>(() => subscription.Messenger);
    }
    
    [Fact]
    public void SubscribeToMessenger()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscribedAction = (DummyMessage message) => { };
        using var subscription = messenger.Subscribe(subscribedAction);
        
        // Assert
        Assert.Equal(messenger, subscription.Messenger);
        Assert.False(subscription.IsDisposed);
        Assert.Equal(typeof(DummyMessage), subscription.MessageType);
        Assert.Equal(nameof(DummyMessage), subscription.MessageTypeName);
        Assert.Equal(subscribedAction.Target, subscription.TargetObject);
        Assert.Equal(subscribedAction.Method.Name, subscription.TargetMethodName);
    }
    
    [Fact]
    public void SubscribeWeakToMessenger()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscribedAction = (DummyMessage message) => { };
        using var subscription = messenger.SubscribeWeak(subscribedAction);
        
        // Assert
        Assert.Equal(messenger, subscription.Messenger);
        Assert.False(subscription.IsDisposed);
        Assert.Equal(typeof(DummyMessage), subscription.MessageType);
        Assert.Equal(nameof(DummyMessage), subscription.MessageTypeName);
        Assert.Equal(subscribedAction.Target, subscription.TargetObject);
        Assert.Equal(subscribedAction.Method.Name, subscription.TargetMethodName);
    }
    
    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private record DummyMessage;
}