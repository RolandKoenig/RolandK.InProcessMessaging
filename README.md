# RolandK.InProcessMessaging <img src="assets/Logo_128.png" width="32" />
A messenger implementation which sends / receives in process messages. It is able to communicate between different areas of your application like threads, agents, etc.

## Feature overview
- [Messenger implementation for sending / receiving messages (publish/subscribe)](#messenger-implementation-for-sending--receiving-messages)
- [Waiting for specific messages](#waiting-for-specific-messages)
- [Separated interfaces for publish and subscribe](#waiting-for-specific-messages)
- Multiple messengers can communicate with each other using a routing mechanism
- Thread synchronization using SynchronizationContext

## Build
[![Continuous integration](https://github.com/RolandKoenig/RolandK.InProcessMessaging/actions/workflows/continuous-integration.yml/badge.svg)](https://github.com/RolandKoenig/RolandK.InProcessMessaging/actions/workflows/continuous-integration.yml)

## Nuget
| Package                    | Link                                                      |
|----------------------------|-----------------------------------------------------------|
| RolandK.InProcessMessaging | https://www.nuget.org/packages/RolandK.InProcessMessaging |

# Samples
## Messenger implementation for sending / receiving messages
Messages can be defined as class, struct or record. They are marked with an InProcessMessage attribute.
```csharp
[InProcessMessage]
private record DummyMessage;
```

Given we've created an InProcessMessenger before, we can easily subscribe to messages. The subscription gets called
each time the message gets published. You can unsubscribe later by calling Dispose or Unsubscribe on the
MessageSubscription object.
```csharp
var messenger = new InProcessMessenger();

//...

var subscription = messenger.Subscribe<DummyMessage>(dummyMessage => 
{
    // Logic
});
```

Now we can publish a message to notify all subscribers of that message type.
```csharp
messenger.Publish(new DummyMessage());
```

## Waiting for specific messages
You can wait for specific messages using the WaitForMessageAsync method. This method subscribes to
the specified message types. It returns the first message it receives and unsubscribes after that.
```csharp
var dummyMessage = await messenger.WaitForMessageAsync<DummyMessage>(CancellationToken.None);
```

## Separated interfaces for publish and subscribe
You are free to use the InProcessMessenger class in your project and on all places you need to send / receive
messages. Especially for environments with dependency injection it would be better to use the interfaces
IInProcessMessagePublisher and IInProcessMessageSubscriber. InProcessMessenger implements both of them. Using these interfaces
you can express in your logic classes, that you only publish messages or only subscribe to messages.