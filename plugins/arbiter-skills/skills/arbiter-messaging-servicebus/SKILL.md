---
name: arbiter-messaging-servicebus
description: Use when integrating Arbiter with Azure Service Bus — registering AddServiceBus with a connection string and ServiceBusOptions, publishing INotifications to a topic/queue, or consuming Service Bus messages as Arbiter requests/notifications.
---

# Arbiter.Messaging.ServiceBus

Bridges Arbiter's mediator pattern to Azure Service Bus topics/queues. Lets you publish notifications across processes and dispatch incoming messages through the local `IMediator`.

## Install

```bash
dotnet add package Arbiter.Messaging.ServiceBus
```

Requires `Azure.Messaging.ServiceBus` (transitive).

## Register

```csharp
using Arbiter.Messaging.ServiceBus;

services.AddServiceBus(
    serviceName: null,                                  // optional logical key
    nameOrConnectionString: "ServiceBus",               // config key OR raw connection string
    configure: opts =>
    {
        // Map notification/request types to topics or queues
        opts.MapTopic<UserCreated>("user-events");
        opts.MapQueue<ProcessOrder>("orders");
    });
```

`nameOrConnectionString` is resolved first as a `ConnectionStrings:<name>` key, then as a literal connection string.

## Publish

```csharp
public class UserCreated : INotification
{
    public int UserId { get; init; }
}

// In your handler / service — same Publish API as in-process notifications
await mediator.Publish(new UserCreated { UserId = 42 }, ct);
// Goes to the topic configured by opts.MapTopic<UserCreated>(...)
```

## Consume

Incoming Service Bus messages are deserialized to the mapped type and dispatched through `IMediator`, so you write a normal handler:

```csharp
public class HandleUserCreated : INotificationHandler<UserCreated>
{
    public ValueTask Handle(UserCreated notification, CancellationToken ct)
    {
        // ...
        return ValueTask.CompletedTask;
    }
}

services.AddTransient<INotificationHandler<UserCreated>, HandleUserCreated>();
```

The hosted Service Bus listener starts automatically with the host; one subscription per mapped topic/queue.

## Notes

- The default subscription name comes from `ServiceBusOptions` — override per type if you run multiple consumers.
- For request/response over Service Bus, map a `IRequest<TResponse>` to a queue; the listener will reply on the session/reply-to.
- Combine with `arbiter-opentelemetry` to get end-to-end traces from publisher → consumer.

## Reference

- Source: https://github.com/loresoft/Arbiter/tree/main/src/Arbiter.Messaging.ServiceBus
