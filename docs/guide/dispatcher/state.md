---
title: State Management
description: Using ModelStateManager, ModelStateLoader, ModelStateEditor, and DispatcherDataService for Blazor component state management
---

# State Management

The `Arbiter.Dispatcher.Client` package provides a set of classes designed for managing local state in Blazor components. These classes integrate with `IDispatcherDataService` to load, edit, and persist data through the dispatcher, and they raise `OnStateChanged` events that components subscribe to in order to trigger re-renders.

## DispatcherDataService

`DispatcherDataService` is a high-level data access service that wraps `IDispatcher` and exposes convenient methods for common CRUD operations. It is registered automatically when you call `AddMessagePackDispatcher`, `AddJsonDispatcher`, `AddServerDispatcher`, or `AddDispatcherServices`.

### Methods

| Method | Description |
| :--- | :--- |
| `Get<TKey, TModel>(id, cacheTime?)` | Retrieve a single model by primary key |
| `GetKey<TModel>(guid, cacheTime?)` | Retrieve a single model by alternate GUID key |
| `Get<TKey, TModel>(ids, cacheTime?)` | Retrieve a list of models by primary keys |
| `All<TModel>(sortField?, cacheTime?)` | Retrieve all models with optional sort field |
| `Page<TModel>(entityQuery?, cacheTime?)` | Retrieve a paged result |
| `Search<TModel>(searchText, entityQuery?)` | Search models (requires `ISupportSearch`) |
| `Create<TCreateModel, TReadModel>(createModel)` | Create a new entity |
| `Update<TKey, TUpdateModel, TReadModel>(id, updateModel)` | Update an existing entity |
| `Save<TKey, TUpdateModel, TReadModel>(id, updateModel)` | Upsert (create or update) |
| `Delete<TKey, TReadModel>(id)` | Delete an entity |
| `GetUser()` | Return the current `ClaimsPrincipal` (override to supply) |

All methods accept an optional `CancellationToken`.

### Subclassing DispatcherDataService

Override `GetUser()` to supply the authenticated user from the Blazor authentication state. The following pattern is taken directly from `samples/EntityFramework/src/Tracker.Client/Services/DataService.cs`:

```csharp
[RegisterTransient]
public class DataService : DispatcherDataService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public DataService(
        IDispatcher dispatcher,
        AuthenticationStateProvider authenticationStateProvider)
        : base(dispatcher)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public override async ValueTask<ClaimsPrincipal?> GetUser(
        CancellationToken cancellationToken = default)
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return state?.User;
    }
}
```

Register the concrete subclass so that `IDispatcherDataService` resolves to it:

```csharp
// Replace the default registration with your concrete type
builder.Services.AddTransient<IDispatcherDataService, DataService>();
```

## ModelStateManager\<TModel\>

`ModelStateManager<TModel>` is the base state container. It holds a single `TModel` instance and raises `OnStateChanged` whenever the model is updated.

```csharp
public class ModelStateManager<TModel>
    where TModel : new()
{
    public event EventHandler<EventArgs>? OnStateChanged;

    public TModel? Model { get; protected set; }

    public virtual void Set(TModel? model);      // sets Model and raises OnStateChanged
    public virtual void Clear();                 // sets Model to default and raises OnStateChanged
    public virtual void New();                   // sets Model to new TModel() and raises OnStateChanged
    public virtual void NotifyStateChanged();    // raises OnStateChanged without changing Model
}
```

`ModelStateManager<TModel>` is registered as a **scoped open generic**, so each Blazor circuit (Server Interactive) or browser session (WASM) gets its own instance.

### Purpose

Use `ModelStateManager<TModel>` when you need to track a single piece of UI state—a selected item, an active filter, or a partially filled form—and notify subscribers when it changes.

### Lifecycle in a component

Subscribe to `OnStateChanged` in `OnInitialized` and unsubscribe in `Dispose` to avoid memory leaks and stale callbacks:

```csharp
@implements IDisposable

@code {
    [Inject] public required ModelStateManager<TaskFilter> FilterState { get; set; }

    protected override void OnInitialized()
    {
        FilterState.OnStateChanged += HandleStateChanged;
        FilterState.New(); // initialise with a fresh model
    }

    private void HandleStateChanged(object? sender, EventArgs e)
        => InvokeAsync(StateHasChanged);

    public void Dispose()
        => FilterState.OnStateChanged -= HandleStateChanged;
}
```

## ModelStateLoader\<TKey, TModel\>

`ModelStateLoader<TKey, TModel>` extends `ModelStateManager<TModel>` with asynchronous data loading from `IDispatcherDataService`. It also tracks a busy state for showing loading indicators.

```csharp
public class ModelStateLoader<TKey, TModel> : ModelStateManager<TModel>
    where TModel : class, IHaveIdentifier<TKey>, new()
{
    public IDispatcherDataService DataService { get; }
    public bool IsBusy { get; protected set; }

    public async ValueTask Load(TKey id, bool force = false);
    public async ValueTask LoadKey(Guid key, bool force = false);
}
```

### Duplicate-request prevention

`Load` skips the data service call if the current `Model.Id` already matches the requested `id` (unless `force: true` is passed). This prevents redundant round-trips when a component re-renders with the same route parameter.

### Example — read-only detail component

```razor
@inject ModelStateLoader<int, UserReadModel> Loader
@implements IDisposable

@if (Loader.IsBusy)
{
    <p>Loading...</p>
}
else if (Loader.Model is not null)
{
    <h2>@Loader.Model.Name</h2>
    <p>@Loader.Model.Email</p>
}

@code {
    [Parameter, EditorRequired] public int UserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Loader.OnStateChanged += HandleStateChanged;
        await Loader.Load(UserId);
    }

    private void HandleStateChanged(object? sender, EventArgs e)
        => InvokeAsync(StateHasChanged);

    public void Dispose()
        => Loader.OnStateChanged -= HandleStateChanged;
}
```

## ModelStateEditor\<TKey, TReadModel, TUpdateModel\>

`ModelStateEditor<TKey, TReadModel, TUpdateModel>` extends `ModelStateManager<TUpdateModel>` with full CRUD support and change tracking. It maintains both the original read model (`Original`) and the editable update model (`Model`), and uses hash-code comparison to detect unsaved changes.

```csharp
public class ModelStateEditor<TKey, TReadModel, TUpdateModel> : ModelStateManager<TUpdateModel>
    where TKey : notnull
    where TReadModel : class, IHaveIdentifier<TKey>, new()
    where TUpdateModel : class, new()
{
    public IDispatcherDataService DataService { get; }
    public IMapper Mapper { get; }
    public IDispatcher Dispatcher => DataService.Dispatcher;

    public TReadModel? Original { get; protected set; }  // the unmodified read model
    public int EditHash { get; protected set; }           // hash when last loaded or saved

    public bool IsBusy { get; protected set; }
    public bool IsDirty { get; }   // Model hash != EditHash
    public bool IsClean { get; }   // Model hash == EditHash

    public async ValueTask Load(TKey id, bool force = false);
    public async ValueTask LoadKey(Guid key, bool force = false);
    public async ValueTask Save();
    public async ValueTask Delete();
    public ValueTask Cancel();
}
```

### Change tracking

`IsDirty` returns `true` when `Model.GetHashCode()` differs from `EditHash`—the hash recorded at the time the model was last loaded or saved. Ensure your update model type overrides `GetHashCode()` for reliable change detection.

### Save behaviour

`Save()` calls `IDispatcherDataService.Save(key, model)`, which sends an `EntityUpdateCommand` with `upsert: true`. After a successful save, `Original` and `Model` are updated from the returned read model and `EditHash` is reset, so `IsDirty` returns `false` immediately.

### Cancel behaviour

`Cancel()` resets `Model` back to a freshly mapped copy of `Original`, discarding any in-progress edits without a server round-trip.

### DI registration

`ModelStateEditor` is registered as a scoped open generic automatically by `AddDispatcherServices`:

```csharp
services.TryAdd(ServiceDescriptor.Scoped(
    typeof(ModelStateEditor<,,>),
    typeof(ModelStateEditor<,,>)));
```

Inject the closed generic into a component or code-behind class:

```csharp
[Inject]
public required ModelStateEditor<int, UserReadModel, UserUpdateModel> Store { get; set; }
```

## Complete component example

The following example mirrors the `StorePageBase` / `EditPageBase` pattern from `samples/EntityFramework/src/Tracker.Client/Components/Abstracts/`. It shows a Blazor edit page that uses `ModelStateEditor` together with a concrete `DataService` to load, edit, save, and delete a `User` entity.

### DI setup

```csharp
// Registers IDispatcher, IDispatcherDataService, and all state manager open generics
builder.Services
    .AddMessagePackDispatcher((sp, client) =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });

// Replace the default DispatcherDataService with your subclass
builder.Services.AddTransient<IDispatcherDataService, DataService>();
```

### Component (`UserEditPage.razor`)

```razor
@page "/users/{Id:int}/edit"
@implements IDisposable

@inject ModelStateEditor<int, UserReadModel, UserUpdateModel> Store
@inject NotificationService Notification
@inject NavigationManager Navigation

@if (Store.IsBusy)
{
    <p>Loading...</p>
}
else if (Store.Model is not null)
{
    <h1>@(IsCreate ? "Create User" : $"Edit User — {Store.Model.Name}")</h1>

    @if (Store.IsDirty)
    {
        <span class="badge badge-warning">Unsaved changes</span>
    }

    <EditForm Model="Store.Model" OnValidSubmit="HandleSave">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <label>
            Name
            <InputText @bind-Value="Store.Model.Name" />
        </label>

        <label>
            Email
            <InputText @bind-Value="Store.Model.Email" />
        </label>

        <button type="submit" disabled="@Store.IsBusy">Save</button>
        <button type="button" @onclick="HandleCancel">Cancel</button>

        @if (!IsCreate)
        {
            <button type="button" @onclick="HandleDelete">Delete</button>
        }
    </EditForm>
}

@code {
    [Parameter, EditorRequired] public int Id { get; set; }

    private bool IsCreate => Id == default;

    protected override async Task OnInitializedAsync()
    {
        Store.OnStateChanged += HandleStateChanged;

        try
        {
            if (IsCreate)
                Store.New();
            else
                await Store.Load(Id);

            if (Store.Model is null)
                Navigation.NavigateTo("/users");
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
        }
    }

    private void HandleStateChanged(object? sender, EventArgs e)
        => InvokeAsync(StateHasChanged);

    private async Task HandleSave()
    {
        try
        {
            await Store.Save();
            Notification.ShowSuccess("User saved successfully");

            if (IsCreate)
                Navigation.NavigateTo("/users");
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
        }
    }

    private async Task HandleDelete()
    {
        try
        {
            await Store.Delete();
            Notification.ShowSuccess("User deleted successfully");
            Navigation.NavigateTo("/users");
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
        }
    }

    private async Task HandleCancel()
        => await Store.Cancel();

    public void Dispose()
        => Store.OnStateChanged -= HandleStateChanged;
}
```

## Loading states and UI patterns

### Busy indicator

All state classes that perform async operations set `IsBusy = true` before the operation and reset it in a `finally` block, then raise `OnStateChanged`. Subscribe to `OnStateChanged` to refresh the component and reflect the busy state in the UI:

```razor
@if (Store.IsBusy)
{
    <LoadingSpinner />
}
```

### Error state

Catch exceptions from `Load`, `Save`, and `Delete` in the component and display them using your notification service or an inline error element:

```csharp
private async Task HandleSave()
{
    try
    {
        await Store.Save();
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
    {
        Notification.ShowError("Validation failed: " + ex.Message);
    }
    catch (Exception ex)
    {
        Notification.ShowError(ex);
    }
}
```

### List page with DispatcherDataService

For read-only list pages, inject `IDispatcherDataService` (or your concrete subclass) directly and call `Page` or `All`. The following pattern is adapted from `samples/EntityFramework/src/Tracker.Client/Components/Abstracts/ListPageBase.cs`:

```razor
@inject DataService DataService

@code {
    private EntityPagedResult<UserReadModel> _result = EntityPagedResult<UserReadModel>.Empty;

    protected async ValueTask<DataResult<UserReadModel>> LoadData(DataRequest request)
    {
        try
        {
            var query = request.ToQuery();
            var results = await DataService.Page<UserReadModel>(query);
            return results.ToResult();
        }
        catch (Exception ex)
        {
            Notification.ShowError(ex);
            return new DataResult<UserReadModel>(0, []);
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
```

## Best practices

- **Subscribe and unsubscribe** `OnStateChanged` in `OnInitialized`/`Dispose` to avoid memory leaks and stale callbacks.
- **Use `IsBusy`** to disable form controls and show loading indicators during async operations.
- **Check `IsDirty`** before navigating away from an edit page and prompt the user to save or discard changes.
- **Prefer `ModelStateEditor`** for edit forms; prefer `ModelStateLoader`** for read-only detail views; use `ModelStateManager` directly for simple local UI state.
- **Override `GetUser()`** in your `DispatcherDataService` subclass to supply the authenticated `ClaimsPrincipal` to all commands and queries.

## Related topics

- [Client configuration](client.md) — registering `IDispatcher` and choosing a transport.
- [Server configuration](server.md) — the server-side endpoint that receives WASM dispatcher requests.
