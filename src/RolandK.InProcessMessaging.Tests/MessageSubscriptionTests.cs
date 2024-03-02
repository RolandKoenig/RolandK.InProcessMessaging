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
    public void Subscribe_and_Unsubscribe()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscribedAction = (DummyMessage message) => { };
        using var subscription = messenger.Subscribe(subscribedAction);
        subscription.Unsubscribe();
        
        // Assert
        Assert.True(subscription.IsDisposed);
        Assert.Equal(0, messenger.CountSubscriptionsForMessage<DummyMessage>());
    }
    
    [Fact]
    public void Subscribe_and_Unsubscribe_on_InProcessMessenger()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscribedAction = (DummyMessage message) => { };
        var subscription = messenger.Subscribe(subscribedAction);
        messenger.Unsubscribe(subscription);
        
        // Assert
        Assert.True(subscription.IsDisposed);
        Assert.Equal(0, messenger.CountSubscriptionsForMessage<DummyMessage>());
    }
    
    [Fact]
    public void Subscribe_and_Unsubscribe_multiple_times()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscribedAction = (DummyMessage message) => { };
        using var subscription = messenger.Subscribe(subscribedAction);
        subscription.Unsubscribe();
        subscription.Unsubscribe(); // Multiple unsubscribes should have no effect (also no exception)
        subscription.Unsubscribe();
        
        // Assert
        Assert.True(subscription.IsDisposed);
        Assert.Equal(0, messenger.CountSubscriptionsForMessage<DummyMessage>());
    }
    
    [Fact]
    public void Subscribe_and_Dispose_multiple_times()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscribedAction = (DummyMessage message) => { };
        using var subscription = messenger.Subscribe(subscribedAction);
        subscription.Dispose();
        subscription.Dispose(); // Multiple unsubscribes should have no effect (also no exception)
        subscription.Dispose();
        
        // Assert
        Assert.True(subscription.IsDisposed);
        Assert.Equal(0, messenger.CountSubscriptionsForMessage<DummyMessage>());
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
        var subscriberObject = new DummyMessageSubscriberWithInterface();
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
    public void SubscribeAllWeakOnDerivedClass_and_Publish()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriberObject = new DummyDerivedMessageSubscriber();
        var subscriptions = messenger.SubscribeAllWeak(subscriberObject);
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<ThirdDummyMessage>();
        
        // Assert
        Assert.Equal(4, subscriptions.Count());
        Assert.Equal(1, subscriberObject.CountDummyMessage);
        Assert.Equal(2, subscriberObject.CountAnotherDummyMessage);
        Assert.Equal(1, subscriberObject.CountDummyMessageOnDerivedClass);
        Assert.Equal(1, subscriberObject.CountDummyMessage2);
    }
    
    [Fact]
    public async Task SubscribeAllWeak_then_collect_subscriber()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriptions = await Task.Run(() =>
        {
            var subscriberObject = new DummyMessageSubscriber();
            return messenger.SubscribeAllWeak(subscriberObject);
        });
        
        GC.Collect();
        GC.Collect();
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        // Assert
        Assert.Equal(2, subscriptions.Count());
        Assert.Equal(2, subscriptions.Count(x => x.IsDisposed));
    }
    
    [Fact]
    public void WaitForMessage_with_Publish()
    {
        // Arrange
        var messenger = new InProcessMessenger();

        // Act
        var messageWaiter = messenger.WaitForMessageAsync<DummyMessage>(CancellationToken.None);

        var message = new DummyMessage();
        messenger.Publish(message);

        // Assert
        Assert.Equal(TaskStatus.RanToCompletion, messageWaiter.Status);
        Assert.Equal(message, messageWaiter.Result);
        Assert.Equal(0, messenger.CountSubscriptionsForMessage<DummyMessage>());
    }

    [Fact]
    public async Task WaitForMessage_without_Publish_then_timeout()
    {
        // Arrange
        var messenger = new InProcessMessenger();

        // Act
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));
        var messageWaiter = messenger.WaitForMessageAsync<DummyMessage>(cancellationTokenSource.Token);

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => messageWaiter);
    }
    
    [Fact]
    public void SubscribeAll_and_UnsubscribeAll()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        // Act
        var subscriberObject = new DummyMessageSubscriber();
        var subscriptions = messenger.SubscribeAll(subscriberObject);
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        subscriptions.UnsubscribeAll();
        
        messenger.Publish<DummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        messenger.Publish<AnotherDummyMessage>();
        
        // Assert
        Assert.Equal(2, subscriptions.Count());
        Assert.Equal(1, subscriberObject.CountDummyMessage);
        Assert.Equal(2, subscriberObject.CountAnotherDummyMessage);
    }
    
    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private record DummyMessage;

    [InProcessMessage]
    private record AnotherDummyMessage;

    [InProcessMessage]
    private record ThirdDummyMessage;

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

    private class DummyDerivedMessageSubscriber : DummyMessageSubscriber
    {
        public int CountDummyMessageOnDerivedClass { get; set; }
        
        public int CountDummyMessage2 { get; set; }
        
        private void OnMessageReceived(ThirdDummyMessage message)
        {
            this.CountDummyMessageOnDerivedClass++;
        }
        
        private void OnMessageReceived(DummyMessage message)
        {
            this.CountDummyMessage2++;
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