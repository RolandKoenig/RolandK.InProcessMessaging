using Microsoft.Extensions.DependencyInjection;

namespace RolandK.InProcessMessaging.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void Publish_Subscribe_using_DependencyInjection()
    {
        // Arrange
        var messenger = new InProcessMessenger();

        var services = new ServiceCollection();
        services.AddSingleton<IInProcessMessagePublisher>(messenger);
        services.AddSingleton<IInProcessMessageSubscriber>(messenger);

        using var serviceProvider = services.BuildServiceProvider();

        // Act
        var countReceivedMessages = 0;
        
        var subscriber = serviceProvider.GetRequiredService<IInProcessMessageSubscriber>();
        using var subscription = subscriber.Subscribe<DummyMessage>(_ => countReceivedMessages++);

        var publisher = serviceProvider.GetRequiredService<IInProcessMessagePublisher>();
        publisher.Publish<DummyMessage>();

        // Assert
        Assert.Equal(1, countReceivedMessages);
    }
    
    //*************************************************************************
    //*************************************************************************
    //*************************************************************************
    [InProcessMessage]
    private record DummyMessage;
}