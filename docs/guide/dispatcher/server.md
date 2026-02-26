---
title: Dispatcher Server
description: Configuring the Arbiter Dispatcher server-side endpoint for receiving and processing Blazor WASM dispatcher requests
---

# Dispatcher Server

The `Arbiter.Dispatcher.Server` package provides the server-side infrastructure for receiving and processing dispatcher requests sent by Blazor WebAssembly clients. It exposes a single HTTP endpoint that accepts both JSON and MessagePack payloads and routes them through the mediator pipeline.

## Overview

The `DispatcherEndpoint` class handles incoming `POST /api/dispatcher/send` requests. It:

- Reads the `X-Message-Request-Type` header to determine the CLR type to deserialize
- Deserializes the request body using either JSON (`application/json`) or MessagePack (`application/x-msgpack`)
- Applies the current `ClaimsPrincipal` to any request that implements `IRequestPrincipal`
- Forwards the deserialized request to `IMediator`
- Returns the response serialized in the same format as the request

## Installation

```powershell
Install-Package Arbiter.Dispatcher.Server
```

OR

```shell
dotnet add package Arbiter.Dispatcher.Server
```

## Service registration

Register the `DispatcherEndpoint` singleton and MessagePack serializer options using the `AddDispatcherService` extension method:

```csharp
builder.Services.AddDispatcherService();
```

## Endpoint mapping

Map the `/api/dispatcher/send` route using `MapDispatcherService`. Apply authorization policies using standard ASP.NET Core endpoint conventions:

```csharp
// Returns IEndpointConventionBuilder for further customization
app.MapDispatcherService().RequireAuthorization();
```

This maps a single `POST /api/dispatcher/send` endpoint that accepts both JSON and MessagePack bodies.

### Complete `Program.cs` example

The following example is adapted from `samples/EntityFramework/src/Tracker.Web/Program.cs`:

```csharp
using Arbiter.CommandQuery.Endpoints;
using Arbiter.Dispatcher.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Register entity command/query endpoints
builder.Services.AddEndpointRoutes();

// Register the DispatcherEndpoint and MessagePack serializer options
builder.Services.AddDispatcherService();

// Configure JSON serializer options for domain types
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.AddDomainOptions());

var app = builder.Build();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Client.Routes).Assembly);

// Map REST endpoints for entity operations
app.MapEndpointRoutes();

// Map the dispatcher endpoint and require authorization
app.MapDispatcherService().RequireAuthorization();

app.Run();
```

## Request type header

Because the dispatcher endpoint accepts requests for any registered handler type, the server needs to know which CLR type to deserialize the request body into. The client sets the `X-Message-Request-Type` HTTP header to the assembly-qualified type name of the request before posting:

```text
X-Message-Request-Type: Arbiter.CommandQuery.Queries.EntityIdentifierQuery`2[[System.Int32, ...],[Tracker.Domain.UserReadModel, ...]], Arbiter.CommandQuery
```

The server resolves this value using `Type.GetType(requestTypeName)`. If the header is missing, empty, or references an unknown type, the endpoint returns HTTP 400 with a Problem Details body.

> **Note:** The `X-Message-Request-Type` header is set automatically by `JsonDispatcher` and `MessagePackDispatcher`. Application code does not need to set it manually. See [Client](client.md) for details.

## Serialization formats

The endpoint selects a format based on the `Content-Type` header of the incoming request:

| Content-Type | Format | Client dispatcher |
| :--- | :--- | :--- |
| `application/json` | JSON (System.Text.Json) | `JsonDispatcher` |
| `application/x-msgpack` | MessagePack (MessagePack-CSharp) | `MessagePackDispatcher` |

When no `Content-Type` header is present the endpoint defaults to JSON.

### MessagePack response type header

For MessagePack responses the server also writes the `X-Message-Response-Type` header containing the portable type name of the response object. The `MessagePackDispatcher` client reads this header when deserializing error responses (Problem Details).

## Security

### Authorization

Apply authorization policies on the endpoint convention builder returned by `MapDispatcherService()`:

```csharp
// Require any authenticated user
app.MapDispatcherService().RequireAuthorization();

// Require a specific policy
app.MapDispatcherService().RequireAuthorization("RequireAdminRole");
```

### Principal injection

Requests that implement `IRequestPrincipal` automatically receive the current `ClaimsPrincipal` from the HTTP context. This means server-side handler logic—audit field population, tenant filtering, soft-delete visibility—behaves identically to a direct mediator call from a controller or minimal API endpoint.

### Antiforgery

The `DispatcherEndpoint` does not apply antiforgery validation by default. If your application requires antiforgery protection, enable the standard ASP.NET Core antiforgery middleware and call `UseAntiforgery()` before the dispatcher endpoint is mapped:

```csharp
builder.Services.AddAntiforgery();

// ...

app.UseAntiforgery();
app.MapDispatcherService().RequireAuthorization();
```

## Error handling

All errors are returned as RFC 7807 Problem Details responses. The format (JSON or MessagePack) matches the `Content-Type` of the original request.

| Scenario | HTTP status | Detail |
| :--- | :--- | :--- |
| Missing `X-Message-Request-Type` header | 400 | `"Missing X-Message-Request-Type header"` |
| Unknown type name in header | 400 | `"Unable to resolve request type: <name>"` |
| Deserialization failure | 400 | `"Failed to deserialize request of type: <name>"` |
| `DomainException` in handler | Configured status code | Exception message |
| Unhandled exception | 500 | Exception message |

## Performance considerations

### MessagePack vs JSON

MessagePack serialization produces smaller payloads and faster serialize/deserialize times compared to JSON. For applications with high request volumes or large response payloads, prefer `MessagePackDispatcher` on the client side.

### HTTP/2

Both `JsonDispatcher` and `MessagePackDispatcher` prefer HTTP/2 by default (`HttpVersion.Version20` with `RequestVersionOrLower`). Ensure the server is configured to accept HTTP/2 (the default in Kestrel). HTTP/2 multiplexing reduces latency for concurrent dispatcher calls.

### Response compression

Enable response compression in the host application to reduce payload sizes over the wire, especially for JSON responses:

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// ...

app.UseResponseCompression();
```

## Logging and diagnostics

The `DispatcherEndpoint` uses structured logging at the following levels:

| Level | Event |
| :--- | :--- |
| `Warning` | Missing or empty `X-Message-Request-Type` header |
| `Warning` | Unable to resolve request type from header value |
| `Warning` | Failed to deserialize request body |
| `Error` | Unhandled exception during dispatch |

Enable mediator tracing and metrics via `Arbiter.Mediation.OpenTelemetry` to gain visibility into handler execution times and throughput:

```csharp
builder.Services.AddMediatorDiagnostics();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMediatorInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
    )
    .WithMetrics(metrics => metrics
        .AddMediatorInstrumentation()
        .AddAspNetCoreInstrumentation()
    );
```

## End-to-end example — WASM mode

The following trace illustrates how a query flows from a Blazor WASM component through the dispatcher endpoint to an Entity Framework handler.

### Client (WASM)

```csharp
// IDispatcher injected into a component or service
var query = new EntityIdentifierQuery<int, UserReadModel>(principal, userId);

var user = await dispatcher.Send<EntityIdentifierQuery<int, UserReadModel>, UserReadModel>(
    query,
    cancellationToken);
```

### HTTP request

```text
POST /api/dispatcher/send HTTP/2
Content-Type: application/x-msgpack
X-Message-Request-Type: Arbiter.CommandQuery.Queries.EntityIdentifierQuery`2[[System.Int32,...],[Tracker.Domain.UserReadModel,...]], Arbiter.CommandQuery

<MessagePack-encoded body>
```

### Server (`DispatcherEndpoint`)

1. Reads `X-Message-Request-Type` → resolves `EntityIdentifierQuery<int, UserReadModel>`
2. Deserializes MessagePack body → populates the query object
3. Applies `ClaimsPrincipal` → `query.Principal = httpContext.User`
4. Calls `IMediator.Send(query)` → routes to `EntityIdentifierQueryHandler`

### Handler

```csharp
// EntityIdentifierQueryHandler (registered via AddEntityQueries<TContext, TEntity, TKey, TReadModel>)
// Queries the database, applies tenant/soft-delete filters, maps to UserReadModel
// Returns UserReadModel? to the endpoint
```

### HTTP response

```text
HTTP/2 200 OK
Content-Type: application/x-msgpack
X-Message-Response-Type: Tracker.Domain.UserReadModel, Tracker.Shared

<MessagePack-encoded body>
```
