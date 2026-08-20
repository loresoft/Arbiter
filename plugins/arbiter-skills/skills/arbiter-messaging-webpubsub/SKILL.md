---
name: arbiter-messaging-webpubsub
description: Use when integrating Arbiter with Azure Web PubSub — registering AddWebPubSub with a connection string or TokenCredential, publishing JSON messages to hub groups, writing hosted listener processors, or enabling distributed HybridCache expiration over Web PubSub.
---

# Arbiter.Messaging.WebPubSub

Bridges Arbiter to Azure Web PubSub hubs and groups. Hubs and groups are ephemeral, so no durable entity has to be provisioned or cleaned up per application instance — useful for fan-out scenarios such as distributed cache invalidation.

## Install

```bash
dotnet add package Arbiter.Messaging.WebPubSub
```

Requires `Azure.Messaging.WebPubSub` and `Azure.Messaging.WebPubSub.Client` (transitive).

## Register

```csharp
using Arbiter.Messaging.WebPubSub;

services.AddWebPubSub(
    serviceName: null,                          // optional logical key
    nameOrConnectionString: "WebPubSub",        // config key OR raw connection string
    configureHubs: hubs => hubs
        .AddHub("notifications")                 // hub only
        .AddHub("cacheExpire", "app-instance")); // hub plus the groups listeners join
```

Identity-based authentication uses the `TokenCredential` overload:

```csharp
services.AddWebPubSub(
    serviceName: null,
    endpoint: "https://my-service.webpubsub.azure.com",
    credential: new DefaultAzureCredential(),
    configureHubs: hubs => hubs.AddHub("notifications"));
```

`nameOrConnectionString` is resolved first as a `ConnectionStrings:<name>` key, then as a root configuration value, then as a literal value.

A `WebPubSubServiceClient` is registered as a keyed singleton per declared hub name.

## Publish

```csharp
public sealed class UserCreatedPublisher(WebPubSubHubContext context)
    : WebPubSubPublisherBase(context)
{
    public Task PublishAsync(UserCreated message, CancellationToken cancellationToken = default)
        => PublishToGroupAsJsonAsync(
            "app-instance",
            message,
            UserCreatedSerializerContext.Default.UserCreated,
            cancellationToken);
}

services.AddWebPubSubPublisher<UserCreatedPublisher>(
    serviceName: null,
    hubName: "notifications",
    groups: ["app-instance"]);
```

`WebPubSubPublisherBase` receives a resolved `WebPubSubHubContext`, so derived publishers use formatted hub and group names without resolving keyed service clients directly.

## Consume

Derive from `WebPubSubProcessorBase` and register it; it runs as an `IHostedService` that connects on host start, joins the configured groups, and disposes on stop:

```csharp
public sealed class UserCreatedProcessor : WebPubSubProcessorBase
{
    public UserCreatedProcessor(
        WebPubSubHubContext context,
        IHostApplicationLifetime lifetime,
        ILogger<UserCreatedProcessor> logger)
        : base(context, lifetime, logger) { }

    protected override async Task ProcessGroupMessageAsync(WebPubSubGroupMessageEventArgs args)
    {
        var message = args.ReadFromJson<UserCreated>();
        // ...
    }

    protected override Task ProcessServerMessageAsync(WebPubSubServerMessageEventArgs args)
        => Task.CompletedTask;
}

services.AddWebPubSubProcessor<UserCreatedProcessor>(serviceName: null, hubName: "notifications");
```

When no groups are passed, the groups declared for the hub in `AddWebPubSub` are used.

## Distributed cache expiration

```csharp
// Both publisher and subscriber
services.AddWebPubSubCacheExpire(
    serviceName: null,
    hubName: "cacheExpire",
    groupName: "app-instance");
```

`AddWebPubSubCacheExpirePublisher` and `AddWebPubSubCacheExpireSubscriber` register the halves independently. The publisher expires the local `HybridCache` first, then publishes a `CacheExpireMessage`; the subscriber skips messages carrying its own `SourceId`.

## Notes

- Hub names must match `` ^[A-Za-z][A-Za-z0-9_`,.[\]]{0,127}$ `` — `WebPubSubOptions.NameSuffix` is appended with an underscore. `GroupSuffix` uses a hyphen.
- Web PubSub does not persist messages and has no dead-letter queue; messages published while a listener is disconnected are lost. Use `Arbiter.Messaging.ServiceBus` when guaranteed delivery is required.
- Malformed payloads are logged and dropped by `CacheExpireProcessor`.

## Reference

- Source: https://github.com/loresoft/Arbiter/tree/main/src/Arbiter.Messaging.WebPubSub
