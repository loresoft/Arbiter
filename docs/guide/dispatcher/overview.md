---
title: Dispatcher Overview
description: Overview of the Arbiter Dispatcher libraries for Blazor, with full support for WASM and Server Interactive render modes
---

# Dispatcher Overview

The Arbiter Dispatcher libraries provide a unified `IDispatcher` abstraction for sending commands and queries from Blazor components. The Dispatcher is designed specifically for Blazor applications and includes full support for **Blazor Auto render mode**, automatically wiring the correct transport based on whether a component is rendered in WebAssembly or Server Interactive mode.

| Library | Package | Description |
| :--- | :--- | :--- |
| [Arbiter.Dispatcher.Server](#server-package) | [![Arbiter.Dispatcher.Server](https://img.shields.io/nuget/v/Arbiter.Dispatcher.Server.svg)](https://www.nuget.org/packages/Arbiter.Dispatcher.Server/) | ASP.NET Core endpoint that receives and routes dispatcher messages from Blazor WASM clients |
| [Arbiter.Dispatcher.Client](#client-package) | [![Arbiter.Dispatcher.Client](https://img.shields.io/nuget/v/Arbiter.Dispatcher.Client.svg)](https://www.nuget.org/packages/Arbiter.Dispatcher.Client/) | Client-side dispatcher implementations for WASM (JSON/MessagePack) and Server Interactive modes |

## Architecture

The Dispatcher introduces a thin abstraction—`IDispatcher`—over `IMediator`. Blazor components always depend on `IDispatcher`, never on the underlying transport. The correct implementation is registered at startup depending on the render mode:

```text
Blazor Component
      │
      ▼
  IDispatcher
      ├── WASM mode:
      │     JsonDispatcher / MessagePackDispatcher
      │       ──► HTTP POST /api/dispatcher/send
      │             ──► DispatcherEndpoint
      │                   ──► IMediator ──► Handler
      │
      └── Server Interactive mode:
            ServerDispatcher
              ──► IMediator ──► Handler (in-process, no HTTP hop)
```

### Command and query flow — WASM mode

1. A Blazor WASM component calls `IDispatcher.Send(request)`.
2. `JsonDispatcher` or `MessagePackDispatcher` serializes the request and posts it to `POST /api/dispatcher/send` on the server.
3. The `X-Message-Request-Type` HTTP header carries the assembly-qualified type name so the server can deserialize the payload correctly.
4. `DispatcherEndpoint` resolves the CLR type, deserializes the body, injects the current `ClaimsPrincipal`, and forwards the request to `IMediator`.
5. The mediator runs the handler pipeline and returns the response.
6. The response is serialized (JSON or MessagePack) and streamed back to the client.

### Command and query flow — Server Interactive mode

1. A Blazor Server component calls `IDispatcher.Send(request)`.
2. `ServerDispatcher` delegates directly to `IMediator` in-process—no HTTP hop occurs.
3. The mediator runs the handler pipeline and returns the response.

### Blazor Auto render mode

In Blazor Auto render mode, a component may pre-render on the server and later activate in WebAssembly. Register **both** `ServerDispatcher` and the chosen remote dispatcher in the same application:

- In the Blazor **host project** (e.g. `Tracker.Web`): register `AddServerDispatcher()` for Server Interactive rendering and `AddDispatcherService()` to serve WASM clients.
- In the **WASM client project** (e.g. `Tracker.Client`): register `AddMessagePackDispatcher(...)` for WebAssembly rendering.

The DI container for each render environment resolves `IDispatcher` to the appropriate implementation automatically.

## Installation

### Server package

Install `Arbiter.Dispatcher.Server` in the Blazor host (server) project:

```powershell
Install-Package Arbiter.Dispatcher.Server
```

OR

```shell
dotnet add package Arbiter.Dispatcher.Server
```

### Client package

Install `Arbiter.Dispatcher.Client` in both the WASM client project and the Blazor host project:

```powershell
Install-Package Arbiter.Dispatcher.Client
```

OR

```shell
dotnet add package Arbiter.Dispatcher.Client
```

## Minimal configuration

### WASM client project (`Tracker.Client/Program.cs`)

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register the MessagePack dispatcher for WebAssembly rendering
builder.Services
    .AddMessagePackDispatcher((sp, client) =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });

await builder.Build().RunAsync();
```

### Blazor host project (`Tracker.Web/Program.cs`)

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Register the server dispatcher for Server Interactive rendering
builder.Services.AddServerDispatcher();

// Register the DispatcherEndpoint to serve WASM clients
builder.Services.AddDispatcherService();

var app = builder.Build();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Client.Routes).Assembly);

// Map the /api/dispatcher/send endpoint; apply authorization as needed
app.MapDispatcherService().RequireAuthorization();

app.Run();
```

> **Note:** `AddDispatcherService()` registers the `DispatcherEndpoint` singleton and MessagePack serializer options. `MapDispatcherService()` maps the `POST /api/dispatcher/send` route. Both are required when WASM clients are in use.

## Next steps

- [Server configuration](server.md) — routing, serialization, security, and diagnostics for the server-side dispatcher endpoint.
- [Client configuration](client.md) — DI registration, sending commands and queries, and choosing between JSON and MessagePack.
- [State management](state.md) — using `ModelStateManager`, `ModelStateLoader`, `ModelStateEditor`, and `DispatcherDataService` in Blazor components.
