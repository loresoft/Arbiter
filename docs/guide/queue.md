---
title: Background Queue
description: Background request queues for mediator commands
---

# Background Queue

`Arbiter.Queue` provides a background queue abstraction for mediator requests. Queued work is represented as an `IRequest` and processed through `IMediator`, so existing request handlers and pipeline behaviors still apply.

## In-process Queue

Use the in-process queue for server-hosted applications that need local fire-and-forget processing.

```csharp
services.AddBackgroundQueue();
services.AddTransient<IRequestHandler<SendWelcomeEmail, Unit>, SendWelcomeEmailHandler>();
```

```csharp
public sealed record SendWelcomeEmail(Guid UserId) : IRequest<Unit>;
```

```csharp
public sealed class SendWelcomeEmailHandler(IEmailSender emailSender)
    : IRequestHandler<SendWelcomeEmail, Unit>
{
    public async ValueTask<Unit> Handle(
        SendWelcomeEmail request,
        CancellationToken cancellationToken = default)
    {
        await emailSender.SendWelcomeEmailAsync(request.UserId, cancellationToken);
        return default;
    }
}
```

```csharp
await backgroundQueue.Enqueue(new SendWelcomeEmail(userId), cancellationToken);
```

## Service Bus Queue

`Arbiter.Messaging.ServiceBus` provides a durable Service Bus-backed implementation of `IBackgroundQueue`.

```csharp
services.AddServiceBus(
    serviceName: "ServiceBus",
    nameOrConnectionString: "AzureWebJobsServiceBus",
    configureBus: bus => bus.AddQueue("background-work"));

services.AddServiceBusBackgroundQueue(
    serviceName: "ServiceBus",
    queueName: "background-work");

services.AddTransient<IRequestHandler<SendWelcomeEmail, Unit>, SendWelcomeEmailHandler>();
```

The Service Bus implementation serializes the runtime request type and message body, then the processor deserializes the request and dispatches it through `IMediator`.

`ServiceBusBackgroundQueue` is the enqueue-side implementation. `ServiceBusBackgroundService` is the reusable processing implementation used by `AddServiceBusBackgroundProcessor(...)`; it can also be injected into an Azure Functions app and called from a Service Bus trigger.

```csharp
using Arbiter.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;

public sealed class BackgroundWorkFunction(ServiceBusBackgroundService backgroundService)
{
    [Function(nameof(BackgroundWorkFunction))]
    public Task RunAsync(
        [ServiceBusTrigger("background-work", AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        return backgroundService.ProcessMessageAsync(
            message,
            completeMessage: messageActions.CompleteMessageAsync,
            deadLetterMessage: messageActions.DeadLetterMessageAsync,
            cancellationToken);
    }
}
```

When using manual settlement in an Azure Functions Service Bus trigger, set `AutoCompleteMessages = false` so the function runtime does not also try to complete the message after `ServiceBusBackgroundService` settles it.

Blazor WebAssembly clients should enqueue background work through a server API. Hosted workers and Service Bus processors should run in a server process.
