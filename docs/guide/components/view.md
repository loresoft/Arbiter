---
title: View Page
description: Display a single read model loaded by its identifier using the ViewPageBase component
---

# View Page

`ViewPageBase<TKey, TReadModel>` is the base component for pages that display a single read model loaded by its identifier. It loads the model when the `Id` parameter is set or changed, handles the not-found scenario, and exposes the loaded model and busy state to the page markup.

| Type parameter | Description                                                    |
| :------------- | :------------------------------------------------------------- |
| `TKey`         | The type of the model identifier (for example `int` or `Guid`) |
| `TReadModel`   | The read model returned by the query and displayed on the page |

## Usage

Set the `@page` route with an `Id` route parameter, inherit from `ViewPageBase<TKey, TReadModel>`, and render the `Model` once it is loaded. The base component loads the model through the dispatcher; the page only needs to display it.

```razor
@page "/priorities/{id:int}/view"

@inherits ViewPageBase<int, PriorityReadModel>

<PageTitle>@PageTitle()</PageTitle>

<div class="container">
    <LoadingBlock IsLoading="Model == null">
        <div class="card shadow">
            <div class="card-header">
                <h5>@ModelDisplay</h5>
            </div>
            <div class="card-body">
                <dl class="row">
                    <dt class="col-sm-3">Name</dt>
                    <dd class="col-sm-9">@Model?.Name</dd>

                    <dt class="col-sm-3">Description</dt>
                    <dd class="col-sm-9">@Model?.Description</dd>

                    <dt class="col-sm-3">Display Order</dt>
                    <dd class="col-sm-9">@Model?.DisplayOrder</dd>
                </dl>
            </div>
            <div class="card-footer">
                <a class="btn btn-primary"
                   href="@RouteLinks.Priorities.Edit(Model!.Id)">
                    Edit
                </a>
            </div>
        </div>
    </LoadingBlock>
</div>

@code
{
    protected override string? ModelDisplay => Model?.Name;
}
```

## Members

The following members are available to the page from `ViewPageBase<TKey, TReadModel>` and its base components.

| Member         | Description                                                        |
| :------------- | :----------------------------------------------------------------- |
| `Id`           | The route or query parameter identifying the model to load         |
| `Model`        | The loaded `TReadModel`, or `null` while loading or when not found |
| `IsBusy`       | `true` while the model is loading                                  |
| `Dispatcher`   | The `IDispatcher` used to send commands and queries                |
| `DataService`  | The `IDispatcherDataService` used to load models                   |
| `ModelDisplay` | Override to provide a display label for the current model          |
| `PageTitle()`  | Returns the computed page title for the current model              |

## Handling not found

When the model cannot be loaded, the component navigates using `GetRedirectLocation(RedirectReason.NotFound, id)`. Override the method to control where the page redirects for each `RedirectReason`:

```csharp
protected override string? GetRedirectLocation(RedirectReason reason, int id)
    => reason == RedirectReason.NotFound ? RouteLinks.Priorities.List() : null;
```

Returning `null` keeps the user on the current page.

## Loading additional data

Override `OnLoadedAsync()` to load supporting data after the primary model is retrieved:

```csharp
protected override async ValueTask OnLoadedAsync()
{
    await base.OnLoadedAsync();

    // load related data using Dispatcher or DataService
}
```

## Next steps

- [Edit Page](edit.md) — create and edit models with the two-model pattern
- [List Pages](list.md) — paged lists, result sets, and nested lists
