---
name: arbiter-dispatcher
description: Use when wiring the Blazor IDispatcher abstraction from Arbiter.Dispatcher — MessagePackDispatcher / JsonDispatcher for WebAssembly, ServerDispatcher for Server Interactive, AddDispatcherService + MapDispatcherService on the host, or component state helpers ModelStateManager / ModelStateLoader / ModelStateEditor. Trigger on Blazor Auto render mode setup.
---

# Arbiter.Dispatcher

Unified `IDispatcher` abstraction so Blazor components depend on a single interface regardless of render mode. The right transport is chosen at startup:

| Render mode | Implementation | Transport |
| --- | --- | --- |
| WebAssembly | `MessagePackDispatcher` / `JsonDispatcher` | HTTP POST → `/api/dispatcher/send` |
| Server Interactive | `ServerDispatcher` | Direct in-process `IMediator` |

## Install

```bash
# Server (host) project
dotnet add package Arbiter.Dispatcher.Server

# Client (WASM and/or shared component library)
dotnet add package Arbiter.Dispatcher.Client
```

## Register

**Server host project**

```csharp
using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Server;

builder.Services.AddCommandQuery();
builder.Services.AddServerDispatcher();        // in-process IDispatcher for Server Interactive
builder.Services.AddDispatcherService();       // dispatcher HTTP endpoint for WASM clients

var app = builder.Build();
app.MapDispatcherService().RequireAuthorization();
```

**WebAssembly client project**

```csharp
using Arbiter.Dispatcher;

// MessagePack is recommended (faster, smaller payloads)
builder.Services.AddMessagePackDispatcher((sp, client) =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

// Or JSON if MessagePack interop is a problem:
// builder.Services.AddJsonDispatcher((sp, client) => { ... });
```

For **Blazor Auto** render mode, register both — each render environment resolves the right `IDispatcher`:

```csharp
// Host project
builder.Services.AddServerDispatcher();
builder.Services.AddDispatcherService();

// WASM client project
builder.Services.AddMessagePackDispatcher((sp, client) =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
```

## Sending commands and queries

```csharp
// Low-level
var user = await dispatcher.Send<EntityIdentifierQuery<int, UserReadModel>, UserReadModel>(
    new EntityIdentifierQuery<int, UserReadModel>(principal, id), ct);

// High-level — IDispatcherDataService wraps the common CRUD shapes
var user  = await dataService.Get  <int, UserReadModel>(id);
var page  = await dataService.Page <UserReadModel>(entityQuery);
var saved = await dataService.Save <int, UserUpdateModel, UserReadModel>(id, updateModel);
```

## Component state — load / edit / save / delete

```razor
@inject ModelStateEditor<int, UserReadModel, UserUpdateModel> Store
@implements IDisposable

@code {
    [Parameter] public int Id { get; set; }
    [Parameter] public bool IsCreate { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Store.OnStateChanged += HandleStateChanged;
        if (IsCreate) Store.New();
        else          await Store.Load(Id);
    }

    private void HandleStateChanged(object? s, EventArgs e) => InvokeAsync(StateHasChanged);

    public void Dispose() => Store.OnStateChanged -= HandleStateChanged;
}

<!--
Store.Model    bound to the edit form (TUpdateModel)
Store.IsBusy   load/save/delete in progress
Store.IsDirty  unsaved changes
Store.Save()   Store.Delete()   Store.Cancel()
-->
```

Lighter-weight helpers if you don't need the full edit lifecycle:

- `ModelStateManager<TModel>` — single-model holder with change notifications.
- `ModelStateLoader<TKey, TModel>` — load-only flow.
- `ModelStateEditor<TKey, TReadModel, TUpdateModel>` — full load/edit/save/delete.

Register them once per closed type:

```csharp
services.AddScoped<ModelStateEditor<int, UserReadModel, UserUpdateModel>>();
```

## Reference

- Overview: https://github.com/loresoft/Arbiter/blob/main/docs/guide/dispatcher/overview.md
- Server: https://github.com/loresoft/Arbiter/blob/main/docs/guide/dispatcher/server.md
- Client: https://github.com/loresoft/Arbiter/blob/main/docs/guide/dispatcher/client.md
- State: https://github.com/loresoft/Arbiter/blob/main/docs/guide/dispatcher/state.md
