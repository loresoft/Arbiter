---
title: Edit Page
description: Create and edit a single model using the EditPageBase component and its two-model read and update pattern
---

# Edit Page

`EditPageBase<TKey, TReadModel, TUpdateModel>` is the base component for pages that create or edit a single model. It maintains an `EditContext`, tracks unsaved changes, and manages the save, cancel, and delete operations. The same page handles both create and edit: when `Id` is the default value the page is in create mode, otherwise it loads the existing model.

| Type parameter | Description                                                            |
| :------------- | :--------------------------------------------------------------------- |
| `TKey`         | The type of the model identifier (for example `int` or `Guid`)         |
| `TReadModel`   | The server read model snapshot, including audit fields and row version |
| `TUpdateModel` | The editable model bound to the form                                   |

## The two-model pattern

`EditPageBase` separates the server state from the form state:

- **`TReadModel`** is the server snapshot loaded for an existing record. It includes generated keys, audit fields, and the row version, and is exposed as `Original`.
- **`TUpdateModel`** contains only the user-editable fields. It is bound to the form and exposed as `Model`.

For change tracking to work, `TUpdateModel` must have value equality. In the sample models this is provided by the `[Equatable]` attribute, which lets `IsClean` and `IsBusy` reflect whether the form has unsaved changes.

`EditPageBase` exposes `Model`, `Original`, `IsBusy`, `IsClean`, and `IsDirty` as shortcuts to the underlying `Store`, so pages bind to these directly rather than through `Store`.

```csharp
// Tracker.Shared/Domain/Priority/Models/PriorityUpdateModel.cs
[Equatable]
[MessagePackObject(true)]
public partial class PriorityUpdateModel
    : EntityUpdateModel
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
```

## Usage

Inherit from `EditPageBase<TKey, TReadModel, TUpdateModel>`, bind the form to `EditContext`, and bind inputs to `Model`. The save button submits the form through `HandleSave`; the delete button is shown only for existing records. This example is from `samples/EntityFramework/src/Tracker.Client/Pages/Priorities/Edit.razor`:

```razor
@page "/priorities/{id:int}"

@inherits EditPageBase<int, PriorityReadModel, PriorityUpdateModel>

<PageTitle>@PageTitle()</PageTitle>

<div class="container">
    <LoadingBlock IsLoading="EditContext == null || Model == null">
        <EditForm EditContext="EditContext" OnValidSubmit="HandleSave">
            <FluentValidator />

            <div class="card shadow mb-5">
                <div class="card-header">
                    <h5>@PageTitle()</h5>
                </div>
                <div class="card-body">
                    <ValidationSummary />

                    <div class="mb-3">
                        <label for="Name" class="form-label">
                            Name: <span class="text-danger">*</span>
                        </label>
                        <InputText @bind-Value="Model!.Name"
                                   DisplayName="Name"
                                   id="Name"
                                   class="form-control"
                                   placeholder="Name" />
                        <ValidationMessage For="@(() => Model.Name)" />
                    </div>

                    <div class="mb-3">
                        <label for="DisplayOrder" class="form-label">
                            Display Order: <span class="text-danger">*</span>
                        </label>
                        <InputNumber @bind-Value="Model!.DisplayOrder"
                                     DisplayName="Display Order"
                                     id="DisplayOrder"
                                     class="form-control" />
                        <ValidationMessage For="@(() => Model.DisplayOrder)" />
                    </div>
                </div>
                <div class="card-footer">
                    <div class="row">
                        <div class="col">
                            <BusyButton id="save-button"
                                        type="submit"
                                        Busy="IsBusy"
                                        Disabled="IsClean"
                                        class="btn btn-primary">
                                Save
                            </BusyButton>
                        </div>
                        <div class="col">
                            <Conditional Condition="!IsCreate">
                                <button id="delete-button"
                                        type="button"
                                        @onclick="HandleDelete"
                                        disabled="@IsBusy"
                                        class="btn btn-danger float-end">
                                    Delete
                                </button>
                            </Conditional>
                        </div>
                    </div>
                </div>
            </div>
        </EditForm>
    </LoadingBlock>
</div>

@code
{
    protected override string? GetRedirectLocation(RedirectReason reason, int id)
        => reason == RedirectReason.Created ? RouteLinks.Priorities.Edit(id) : RouteLinks.Priorities.List();

    protected override string? ModelDisplay => Original?.Name;
}
```

## Members

| Member         | Description                                                   |
| :------------- | :------------------------------------------------------------ |
| `Model`        | The editable `TUpdateModel` bound to the form                 |
| `Original`     | The `TReadModel` server snapshot for the loaded record        |
| `IsBusy`       | `true` while a save or delete operation is running            |
| `IsClean`      | `true` when the model has no unsaved changes                  |
| `IsDirty`      | `true` when the model has unsaved changes                     |
| `EditContext`  | The `EditContext` for validation and binding                  |
| `IsCreate`     | `true` when creating a new record (`Id` is the default value) |
| `HandleSave`   | Persists the model; bound to `OnValidSubmit`                  |
| `HandleDelete` | Deletes the record; typically shown only when `!IsCreate`     |
| `HandleCancel` | Cancels the edit and navigates away                           |
| `ModelDisplay` | Override to provide a display label for the current model     |

## Foreign key and related inputs

Inputs bind directly to `Model`. Related entities can be selected with a component such as `EntitySelect`, as shown in `samples/EntityFramework/src/Tracker.Client/Pages/Tasks/Edit.razor`:

```razor
<EntitySelect TModel="TenantReadModel"
              TValue="int"
              @bind-Value="Model!.TenantId"
              DisplayName="Tenant"
              CacheTime="TimeSpan.FromMinutes(5)"
              id="TenantId"
              class="form-select"
              placeholder="Tenant" />
<ValidationMessage For="@(() => Model.TenantId)" />
```

## Post-save navigation

Override `GetRedirectLocation(RedirectReason reason, TKey id)` to route the user after a create, delete, or cancel. A common pattern navigates to the edit page for a newly created record (so the generated key is in the URL) and back to the list for all other reasons:

```csharp
protected override string? GetRedirectLocation(RedirectReason reason, int id)
    => reason == RedirectReason.Created ? RouteLinks.Priorities.Edit(id) : RouteLinks.Priorities.List();
```

## Unsaved change guard

Because `EditPageBase` tracks unsaved changes through the `Store`, it can warn the user before navigating away from a dirty form. Binding the save button's `Disabled` state to `IsClean` also prevents saving when nothing has changed.

## Lifecycle hooks

`EditPageBase` exposes four hooks for injecting page logic into the load and save flow. Each receives a `CancellationToken` and has a default implementation that does nothing, so override only the ones you need.

| Hook                                                    | When it runs                                         | Use for                                        |
| :------------------------------------------------------ | :--------------------------------------------------- | :--------------------------------------------- |
| `OnCreatedAsync(TUpdateModel model, CancellationToken)` | After a new model is created (create or upsert path) | Applying context-dependent default values      |
| `OnLoadedAsync(CancellationToken)`                      | After the model is created or loaded                 | Loading supporting data such as lookup lists   |
| `OnSavingAsync(CancellationToken)`                      | Before the model is saved                            | Normalizing the model or vetoing the save      |
| `OnSavedAsync(CancellationToken)`                       | After the model is saved successfully                | Refreshing derived data or invalidating caches |

On the create and load path, `OnCreatedAsync` runs first (only when a new model is created) and then `OnLoadedAsync`. On save, `OnSavingAsync` runs before the save and `OnSavedAsync` runs after it succeeds.

### OnCreatedAsync

`OnCreatedAsync` runs after a new model is created, on both the create and upsert paths, and receives the new model so it can apply default values. Changes made here are **not** treated as user edits — the model is still reported as clean afterward, so the page does not open with an unsaved-changes indicator. Use it for defaults that depend on context (the current user, the current date, or a route or query string value); constant defaults belong on the model type as property initializers.

```csharp
protected override Task OnCreatedAsync(PriorityUpdateModel model, CancellationToken cancellationToken)
{
    model.DisplayOrder = 100;
    model.IsActive = true;

    return Task.CompletedTask;
}
```

### OnLoadedAsync

`OnLoadedAsync` runs after the model has been created or loaded, so the page can load any supporting data it needs:

```csharp
protected override async Task OnLoadedAsync(CancellationToken cancellationToken)
{
    await base.OnLoadedAsync(cancellationToken);

    // load related data using Dispatcher or DataService
}
```

### OnSavingAsync

`OnSavingAsync` runs before the model is saved. Return `true` to continue or `false` to abort. Aborting leaves the page unchanged and shows no notification, so an override that vetoes the save should tell the user why. Use it to normalize the model or to run checks that cannot be expressed as validation attributes:

```csharp
protected override Task<bool> OnSavingAsync(CancellationToken cancellationToken)
{
    Model!.Name = Model.Name?.Trim();

    if (string.IsNullOrWhiteSpace(Model.Name))
    {
        Notification.ShowWarning("Name is required.");
        return Task.FromResult(false);
    }

    return Task.FromResult(true);
}
```

### OnSavedAsync

`OnSavedAsync` runs after the model is saved successfully, whether or not the page is about to redirect. Use it to refresh data derived from the model or to invalidate related caches:

```csharp
protected override async Task OnSavedAsync(CancellationToken cancellationToken)
{
    await base.OnSavedAsync(cancellationToken);

    // refresh derived data or invalidate caches
}
```

## Next steps

- [List Pages](list.md) — paged lists, result sets, and nested lists
- [Component Services](services.md) — notifications and principal access
