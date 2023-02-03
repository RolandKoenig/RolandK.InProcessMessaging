namespace RolandK.InProcessMessaging.Tests;

[Collection(TestConstants.SINGLE_TEST_COLLECTION_NAME)]
public class InProcessMessengerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Publish_OneMessenger(int callCount)
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        var receivedCallCount = 0;
        using var registration = messenger.Subscribe<DummyMessage>(_ => receivedCallCount++);
        
        // Act
        for (var loop = 0; loop < callCount; loop++)
        {
            messenger.Publish(new DummyMessage());
        }

        // Assert
        Assert.Equal(callCount, receivedCallCount);
        Assert.Equal(1, messenger.CountSubscriptions);
        Assert.Equal(1, messenger.CountSubscriptionsForMessage<DummyMessage>());
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Publish_OneMessenger_Weak(int callCount)
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        var receivedCallCount = 0;
        var myAction = new Action<DummyMessage>(_ => receivedCallCount++);
        using var registration = messenger.Subscribe<DummyMessage>(myAction);
        
        // Act
        for (var loop = 0; loop < callCount; loop++)
        {
            messenger.Publish(new DummyMessage());
        }

        // Assert
        Assert.Equal(callCount, receivedCallCount);
        Assert.Equal(1, messenger.CountSubscriptions);
        Assert.Equal(1, messenger.CountSubscriptionsForMessage<DummyMessage>());

        GC.KeepAlive(myAction);
    }

    [Fact]
    public void PublishAfterDisposingSubscription_OneMessenger()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        var receivedCallCount = 0;
        var registration = messenger.Subscribe<DummyMessage>(_ => receivedCallCount++);
        
        // Act
        messenger.Publish(new DummyMessage());
        registration.Dispose();
        messenger.Publish(new DummyMessage());
        
        // Assert
        Assert.Equal(1, receivedCallCount);
        Assert.Equal(0, messenger.CountSubscriptions);
        Assert.Equal(0, messenger.CountSubscriptionsForMessage<DummyMessage>());
    }
    
    [Fact]
    public void PublishAfterUnsubscribingSubscription_OneMessenger()
    {
        // Arrange
        var messenger = new InProcessMessenger();
        
        var receivedCallCount = 0;
        var registration = messenger.Subscribe<DummyMessage>(_ => receivedCallCount++);
        
        // Act
        messenger.Publish(new DummyMessage());
        registration.Unsubscribe();
        messenger.Publish(new DummyMessage());
        
        // Assert
        Assert.Equal(1, receivedCallCount);
    }

    [Fact]
    public void ConnectToGlobalMessaging_OneMessenger()
    {
        try
        {
            // Arrange
            var messenger = new InProcessMessenger();
        
            // Act
            var syncContext = new SynchronizationContext();
            messenger.ConnectToGlobalMessaging(InProcessMessengerThreadingBehavior.Ignore, "TestMessenger", syncContext);
            
            // Assert
            Assert.True(messenger.IsConnectedToGlobalMessaging);
            Assert.Equal("TestMessenger", messenger.MessengerName);
            Assert.Equal(syncContext, messenger.HostSyncContext);
            Assert.Equal(InProcessMessengerThreadingBehavior.Ignore, messenger.ThreadingBehavior);
        }
        finally
        {
            InProcessMessenger.DisconnectAllGlobalMessagingConnections();
        }
    }
    
    [Fact]
    public void ConnectAndDisconnectGlobalMessaging_OneMessenger()
    {
        try
        {
            // Arrange
            var messenger = new InProcessMessenger();
        
            // Act
            var syncContext = new SynchronizationContext();
            messenger.ConnectToGlobalMessaging(InProcessMessengerThreadingBehavior.Ignore, "TestMessenger", syncContext);
            messenger.DisconnectFromGlobalMessaging();

            // Assert
            Assert.False(messenger.IsConnectedToGlobalMessaging);
            Assert.Empty(messenger.MessengerName);
            Assert.Null(messenger.HostSyncContext);
        }
        finally
        {
            InProcessMessenger.DisconnectAllGlobalMessagingConnections();
        }
    }
    
    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private record DummyMessage;
}