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

    [Fact]
    public void SubscribeAll_and_Publish()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriberObject = new DummyMessageSubscriber();
        var subscriptions = messenger.SubscribeAll(subscriberObject);
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        // Assert
        Assert.Equal(2, subscriptions.Count());
        Assert.Equal(1, subscriberObject.CountDummyMessage);
        Assert.Equal(2, subscriberObject.CountAnotherDummyMessage);
    }
    
    [Fact]
    public void SubscribeAll_with_handler_interfaces_and_Publish()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriberObject = new DummyMessageSubscriber();
        var subscriptions = messenger.SubscribeAll(subscriberObject);
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        // Assert
        Assert.Equal(2, subscriptions.Count());
        Assert.Equal(1, subscriberObject.CountDummyMessage);
        Assert.Equal(2, subscriberObject.CountAnotherDummyMessage);
    }
    
    [Fact]
    public void SubscribeAllWeak_and_Publish()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriberObject = new DummyMessageSubscriber();
        var subscriptions = messenger.SubscribeAllWeak(subscriberObject);
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        // Assert
        Assert.Equal(2, subscriptions.Count());
        Assert.Equal(1, subscriberObject.CountDummyMessage);
        Assert.Equal(2, subscriberObject.CountAnotherDummyMessage);
    }
    
    [Fact]
    public async Task SubscribeAllWeak_then_collect_subscriber()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriptions = await Task.Run(new Func<IEnumerable<MessageSubscription>>(() =>
        {
            var subscriberObject = new DummyMessageSubscriber();
            return messenger.SubscribeAllWeak(subscriberObject);
        }));
        
        GC.Collect();
        GC.Collect();
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        // Assert
        Assert.Equal(2, subscriptions.Count());
        Assert.Equal(2, subscriptions.Count(x => x.IsDisposed));
    }
    
    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private record DummyMessage;

    [InProcessMessage]
    private record AnotherDummyMessage;

    private class DummyMessageSubscriber
    {
        public int CountDummyMessage { get; set; }
        
        public int CountAnotherDummyMessage { get; set; }
        
        private void OnMessageReceived(DummyMessage message)
        {
            this.CountDummyMessage++;
        }

        private void OnMessageReceived(AnotherDummyMessage message)
        {
            this.CountAnotherDummyMessage++;
        }
    }
    
    private class DummyMessageSubscriberWithInterface :
        IInProcessMessageHandler<DummyMessage>,
        IInProcessMessageHandler<AnotherDummyMessage>
    {
        public int CountDummyMessage { get; set; }
        
        public int CountAnotherDummyMessage { get; set; }
        
        public void OnMessageReceived(DummyMessage message)
        {
            this.CountDummyMessage++;
        }

        public void OnMessageReceived(AnotherDummyMessage message)
        {
            this.CountAnotherDummyMessage++;
        }
    }
}