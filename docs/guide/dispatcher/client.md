---
title: Dispatcher Client
description: Configuring the Arbiter Dispatcher client library for Blazor WASM and Server Interactive modes, including JSON and MessagePack transports
---

# Dispatcher Client

The `Arbiter.Dispatcher.Client` package provides the client-side `IDispatcher` abstraction and its concrete implementations for Blazor applications. Components and services depend only on `IDispatcher`; the registered implementation determines how requests are transported.

| Implementation | Render mode | Transport |
| :--- | :--- | :--- |
| `JsonDispatcher` | WASM | HTTP POST with JSON serialization |
| `MessagePackDispatcher` | WASM | HTTP POST with MessagePack serialization |
| `ServerDispatcher` | Server Interactive | Direct `IMediator` call (in-process) |

## Installation

```powershell
Install-Package Arbiter.Dispatcher.Client
```

OR

```shell
dotnet add package Arbiter.Dispatcher.Client
```

## Service registration

### WASM — MessagePack dispatcher (recommended)

Register the `MessagePackDispatcher` and configure the `HttpClient` base address to point at the Blazor host:

```csharp
// Tracker.Client/Program.cs (WebAssembly project)
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services
    .AddMessagePackDispatcher((sp, client) =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });

await builder.Build().RunAsync();
```

The overload that accepts `Action<IServiceProvider, HttpClient>` gives access to registered services when configuring the client. The following pattern is used in `samples/EntityFramework/src/Tracker.Client/Services/ServiceRegistration.cs`:

```csharp
services
    .AddMessagePackDispatcher((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<EnvironmentOptions>>();
        client.BaseAddress = new Uri(options.Value.BaseAddress);
    })
    .AddHttpMessageHandler<ProgressBarHandler>();
```

Both overloads return `IHttpClientBuilder`, allowing additional message handlers to be chained.

### WASM — JSON dispatcher

Use `AddJsonDispatcher` when MessagePack is not available or not preferred:

```csharp
builder.Services
    .AddJsonDispatcher(client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });
```

### Server Interactive — ServerDispatcher

Register the `ServerDispatcher` in the Blazor host project for Server Interactive rendering. `ServerDispatcher` wraps `IMediator` directly and makes no HTTP calls:

```csharp
// Tracker.Web/Program.cs (host project — Server Interactive rendering path)
builder.Services.AddServerDispatcher();
```

### Blazor Auto render mode

For Blazor Auto render mode, register the appropriate dispatcher in each project so that each render environment's DI container resolves `IDispatcher` correctly.

**Host project** (`Tracker.Web`):

```csharp
// Used when components render on the server (Interactive Server)
builder.Services.AddServerDispatcher();

// Also register the server endpoint to receive WASM requests
builder.Services.AddDispatcherService();
```

**WASM client project** (`Tracker.Client`):

```csharp
// Used when components render in WebAssembly
builder.Services
    .AddMessagePackDispatcher((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<EnvironmentOptions>>();
        client.BaseAddress = new Uri(options.Value.BaseAddress);
    });
```

The actual pattern from the sample application selects the dispatcher based on a tag string passed at registration time:

```csharp
// Tracker.Client/Services/ServiceRegistration.cs
public static void Register(IServiceCollection services, ISet<string> tags)
{
    if (tags.Contains("WebAssembly"))
    {
        services
            .AddMessagePackDispatcher((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<EnvironmentOptions>>();
                client.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .AddHttpMessageHandler<ProgressBarHandler>();
    }

    if (tags.Contains("Server"))
        services.AddServerDispatcher();
}
```

The host calls `AddTrackerClient("Server")` and the WASM project calls `AddTrackerClient("WebAssembly")`, so each environment registers only the correct dispatcher.

### Common dispatcher services

`AddMessagePackDispatcher`, `AddJsonDispatcher`, and `AddServerDispatcher` all call `AddDispatcherServices()` internally, which registers:

- `IDispatcherDataService` → `DispatcherDataService` (transient)
- `ModelStateManager<>` (scoped open generic)
- `ModelStateLoader<,>` (scoped open generic)
- `ModelStateEditor<,,>` (scoped open generic)

You can call `AddDispatcherServices()` directly if you need only those services without registering a dispatcher implementation.

## The IDispatcher interface

All dispatcher implementations satisfy `IDispatcher`:

```csharp
public interface IDispatcher
{
    ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
```

Inject `IDispatcher` into any component or service. The DI container resolves the correct transport automatically.

## Sending commands and queries

### Query by identifier

```csharp
public class UserDetailPage : ComponentBase
{
    [Inject] public required IDispatcher Dispatcher { get; set; }

    private UserReadModel? _user;

    protected override async Task OnInitializedAsync()
    {
        var query = new EntityIdentifierQuery<int, UserReadModel>(principal, userId);
        _user = await Dispatcher
            .Send<EntityIdentifierQuery<int, UserReadModel>, UserReadModel>(
                query,
                CancellationToken.None);
    }
}
```

### Paged query

```csharp
var entityQuery = new EntityQuery
{
    Page = 1,
    PageSize = 20,
    Sort = [new EntitySort { Name = "name", Direction = "asc" }],
};

var query = new EntityPagedQuery<UserReadModel>(principal, entityQuery);

var result = await dispatcher
    .Send<EntityPagedQuery<UserReadModel>, EntityPagedResult<UserReadModel>>(
        query,
        cancellationToken);
```

### Create command

```csharp
var createModel = new UserCreateModel { Name = "Jane Doe", Email = "jane@example.com" };
var command = new EntityCreateCommand<UserCreateModel, UserReadModel>(principal, createModel);

var newUser = await dispatcher
    .Send<EntityCreateCommand<UserCreateModel, UserReadModel>, UserReadModel>(
        command,
        cancellationToken);
```

### Update command

```csharp
var updateModel = new UserUpdateModel { Name = "Jane Smith" };
var command = new EntityUpdateCommand<int, UserUpdateModel, UserReadModel>(principal, userId, updateModel);

var updated = await dispatcher
    .Send<EntityUpdateCommand<int, UserUpdateModel, UserReadModel>, UserReadModel>(
        command,
        cancellationToken);
```

### Delete command

```csharp
var command = new EntityDeleteCommand<int, UserReadModel>(principal, userId);

var deleted = await dispatcher
    .Send<EntityDeleteCommand<int, UserReadModel>, UserReadModel>(
        command,
        cancellationToken);
```

> **Note:** For everyday data access, prefer `IDispatcherDataService` over calling `IDispatcher.Send` directly. `DispatcherDataService` wraps these patterns with principal resolution, optional caching, and a higher-level API. See [State management](state.md) for details.

## Cancellation and timeouts

Pass a `CancellationToken` to any `Send` call. The token is forwarded through serialization, the HTTP request, and the handler pipeline:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

var result = await dispatcher
    .Send<EntityIdentifierQuery<int, UserReadModel>, UserReadModel>(
        query,
        cts.Token);
```

For page-level cancellation in Blazor components, create a `CancellationTokenSource` in `OnInitializedAsync` and cancel it in `Dispose`.

## Error handling

Remote dispatchers (`JsonDispatcher`, `MessagePackDispatcher`) throw `HttpRequestException` when the server returns a non-2xx response. If the server returns a Problem Details body, the exception message is populated from `ProblemDetails.Title` and `ProblemDetails.Detail`.

```csharp
try
{
    var user = await dispatcher
        .Send<EntityIdentifierQuery<int, UserReadModel>, UserReadModel>(
            query,
            cancellationToken);
}
catch (HttpRequestException ex)
{
    // ex.StatusCode contains the HTTP status code
    // ex.Message contains the problem title and detail
    logger.LogError(ex, "Failed to load user {UserId}", userId);
}
```

`ServerDispatcher` propagates mediator exceptions (e.g. `DomainException`, `ValidationException`) directly—no HTTP translation occurs.

## The request type header

When using `JsonDispatcher` or `MessagePackDispatcher`, the client automatically sets the `X-Message-Request-Type` HTTP header to the assembly-qualified name of the request type before posting to the server:

```csharp
// Set internally by RemoteDispatcherBase.SendCore
httpRequest.Headers.Add(DispatcherConstants.RequestTypeHeader, requestName);
// DispatcherConstants.RequestTypeHeader == "X-Message-Request-Type"
```

This header tells the server which CLR type to deserialize the body into. Application code does not set this header manually. See [Server](server.md) for the server-side handling details.

## Caching

Queries that implement `ICacheResult` are automatically cached by `RemoteDispatcherBase` when an `HybridCache` service is registered in DI. The cache key, cache tag, and sliding expiration are read from the request object:

```csharp
// Optional: register HybridCache (e.g. in the WASM project or host)
builder.Services.AddHybridCache();
```

Commands that implement `ICacheExpire` automatically invalidate matching cache entries by key and tag after a successful dispatch.

## Blazor Auto render mode — client perspective

In Blazor Auto render mode, a component starts in Server Interactive mode and later activates in WebAssembly. The `IDispatcher` registered for each environment determines how requests are sent:

- During server-side rendering: `ServerDispatcher` is resolved, and calls go directly to `IMediator`.
- After WebAssembly activation: `MessagePackDispatcher` (or `JsonDispatcher`) is resolved, and calls go over HTTP to `POST /api/dispatcher/send`.

Because both dispatchers implement the same `IDispatcher` interface, component code does not change between render modes.

> **Note:** Ensure the host project registers both `AddServerDispatcher()` (for Interactive Server) and `AddDispatcherService()` (to serve WASM requests). The WASM client project registers only `AddMessagePackDispatcher(...)`.
