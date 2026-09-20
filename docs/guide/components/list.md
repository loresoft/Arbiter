---
title: List Pages
description: Display collections of models using the ListPageBase, ResultPageBase, and NestedListPageBase components
---

# List Pages

`Arbiter.Components` provides three base components for displaying collections of models. Choose the one that matches how the data is loaded and displayed.

| Base component                                                   | Loads                                  | Use for                                                                |
| :--------------------------------------------------------------- | :------------------------------------- | :--------------------------------------------------------------------- |
| `ListPageBase<TKey, TReadModel>`                                 | One page at a time                     | Paged lists where the read and list models are the same                |
| `ListPageBase<TKey, TReadModel, TListModel>`                     | One page at a time                     | Paged lists with a separate, lighter list model                        |
| `ResultPageBase<TReadModel>`                                     | The full result set at once            | Search, charts, or export where the component handles paging in memory |
| `NestedListPageBase<TKey, TParentModel, TReadModel, TListModel>` | A parent model plus a paged child list | Related records displayed under a parent                               |

## Paged list

`ListPageBase<TKey, TReadModel>` loads pages through a `DataGrid`. The base component provides `LoadData` as the grid's `DataProvider` and `DataComponent` as the grid reference. The following example is from `samples/EntityFramework/src/Tracker.Client/Pages/Priorities/List.razor`:

```razor
@page "/priorities"

@inherits ListPageBase<int, PriorityReadModel>

<PageTitle>Priorities</PageTitle>

<DataGrid TItem="PriorityReadModel"
          DataProvider="LoadData"
          Filterable="true"
          Sortable="true"
          @ref="DataComponent">
    <DataColumns>
        <DataColumn Property="x => x.Name" Title="Name" Width="280px">
            <Template Context="item">
                <a href="@RouteLinks.Priorities.Edit(item.Id)"
                   title="Edit Priority @item.Name">
                    @item.Name
                </a>
            </Template>
        </DataColumn>

        <DataColumn Property="x => x.DisplayOrder" Title="Order" SortIndex="0" />
        <DataColumn Property="x => x.IsActive" Title="Active" />

        <DataColumn Property="x => x.Id" Title="Action" Filterable="false" Width="100px">
            <Template Context="item">
                <button type="button"
                        class="btn btn-sm"
                        title="Delete Priority @item.Name"
                        @onclick="() => HandleDelete(item)">
                    <i class="bi bi-trash3"></i>
                </button>
            </Template>
        </DataColumn>
    </DataColumns>
    <DataPagination Context="grid">
        <DataPager PageSize="10" />
        <DataSizer />
        <div>@grid.Pager.StartItem - @grid.Pager.EndItem of @grid.Pager.Total</div>
    </DataPagination>
</DataGrid>
```

| Member               | Description                                                   |
| :------------------- | :------------------------------------------------------------ |
| `LoadData`           | The `DataProvider` the `DataGrid` calls for each page request |
| `DataComponent`      | The `DataGrid` reference wired with `@ref`                    |
| `DataGrid`           | Convenience accessor for the underlying grid                  |
| `HandleDelete(item)` | Deletes the given row                                         |

### Default query and filters

Override `CreateDefaultQuery()` to seed the grid with filters — for example, from query string parameters. This example is from `samples/EntityFramework/src/Tracker.Client/Pages/Tasks/List.razor`:

```razor
@page "/tasks"

@inherits ListPageBase<int, TaskReadModel>

<DataGrid TItem="TaskReadModel"
          DataProvider="LoadData"
          Query="@DefaultQuery"
          Filterable="true"
          Sortable="true"
          @ref="DataComponent">
    @* columns *@
</DataGrid>

@code
{
    [SupplyParameterFromQuery(Name = "priority")]
    private int? PriorityId { get; set; }

    protected override QueryRule? CreateDefaultQuery()
    {
        if (!PriorityId.HasValue)
            return null;

        var filter = new QueryFilter
        {
            Field = nameof(TaskReadModel.PriorityId),
            Operator = QueryOperators.Equal,
            Value = PriorityId.Value
        };

        return new QueryGroup
        {
            Id = nameof(CreateDefaultQuery),
            Logic = QueryLogic.And,
            Filters = [filter]
        };
    }
}
```

Filters can also be applied dynamically at runtime by calling `DataGrid.ApplyFilter` and `DataGrid.RemoveFilter` from input change handlers, allowing custom toolbar controls such as date ranges and lookups to drive the query.

### Adding a fixed constraint

`CreateDefaultQuery` seeds a filter the user can still change, whereas `CombineFilter(EntityFilter?)` adds a constraint the user cannot remove. The base `LoadData` calls it on every page request, after the grid state has been converted to a filter and before the query is sent:

```csharp
var query = request.ToQuery();
query.Filter = CombineFilter(query.Filter);
```

Override `CombineFilter` to combine the grid's filter with the constraint, using `FilterLogic.And` so the user's own grid filters are preserved. This example always restricts the list to active records:

```csharp
protected override EntityFilter? CombineFilter(EntityFilter? gridFilter)
{
    var requiredFilter = new EntityFilter
    {
        Name = nameof(PriorityReadModel.IsActive),
        Operator = FilterOperators.Equal,
        Value = true
    };

    if (gridFilter == null)
        return requiredFilter;

    return new EntityFilter
    {
        Logic = FilterLogic.And,
        Filters = [requiredFilter, gridFilter]
    };
}
```

Returning `gridFilter` unchanged leaves the grid in control of the filter, while discarding it also discards whatever the user typed into the grid filter row — so combine both. The [nested list](#nested-list) uses the same method to restrict a child list to its parent record.

### Loading additional data

Override `OnLoadedAsync(CancellationToken)` to load supporting data after the grid finishes refreshing. It runs after every refresh, once the component has rendered its items, so a failure is logged and reported but does not affect the data already displayed:

```csharp
protected override async Task OnLoadedAsync(CancellationToken cancellationToken)
{
    await base.OnLoadedAsync(cancellationToken);

    // load supporting data using Dispatcher or DataService
}
```

## Separate list model

`ListPageBase<TKey, TReadModel, TListModel>` uses a distinct `TListModel` for the grid, which is useful when the list view needs only a subset of fields or denormalized display values. The page inherits with three type parameters and binds the grid to the list model:

```razor
@page "/banks"

@inherits ListPageBase<int, BankConfigurationReadModel, BankConfigurationListModel>

<DataGrid TItem="BankConfigurationListModel"
          DataProvider="LoadData"
          Filterable="true"
          Sortable="true"
          StateKey="BankConfigurationGrid"
          @ref="DataComponent">
    @* columns bound to BankConfigurationListModel *@
</DataGrid>
```

No custom code is required; `LoadData` loads the `TListModel` page through the dispatcher.

## Full result set

`ResultPageBase<TReadModel>` loads the entire result set at once and lets the component handle paging, sorting, and filtering in memory. It is a good fit for search pages, charts, and exports. The base component exposes `DataLoader` as the grid's loader, `Data` as the loaded set, and `RefreshData()` to reload. This example is from `samples/EntityFramework/src/Tracker.Client/Pages/Tasks/Search.razor`:

```razor
@page "/tasks/search"

@using Arbiter.CommandQuery.Queries

@inherits ResultPageBase<TaskReadModel>

<input type="search"
       class="form-control"
       placeholder="Search Tasks"
       value="@SearchText"
       @oninput="HandleSearchChanged" />

<DataGrid TItem="TaskReadModel"
          DataLoader="DataLoader"
          Filterable="false"
          Sortable="true"
          @ref="DataComponent">
    @* columns *@
</DataGrid>

<div class="card-footer">
    @Data.Count Task(s) found
</div>

@code
{
    private string? SearchText { get; set; }

    protected override EntityQuery? CreateEntityQuery()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return null;

        return EntityFilterBuilder.CreateSearchQuery<TaskReadModel>(SearchText, pageSize: 100);
    }

    private async Task HandleSearchChanged(ChangeEventArgs args)
    {
        SearchText = args.Value as string;
        await RefreshData();
    }
}
```

| Member                | Description                                                        |
| :-------------------- | :----------------------------------------------------------------- |
| `DataLoader`          | The loader the `DataGrid` or `DataList` calls to load the full set |
| `Data`                | The loaded, read-only result set                                   |
| `CreateEntityQuery()` | Override to build the query for the result set                     |
| `RefreshData()`       | Reloads the result set with the current query                      |

## Nested list

`NestedListPageBase<TKey, TParentModel, TReadModel, TListModel>` displays a paged list of related records under a parent model. The `Id` route parameter identifies the **parent**, which the base component loads before loading the child list. The following example lists the devices for a bank:

```razor
@page "/banks/{id:int}/devices"

@inherits NestedListPageBase<int, BankConfigurationReadModel, DeviceConfigurationReadModel, DeviceConfigurationListModel>

<PageTitle>@PageTitle("Devices")</PageTitle>

<DataGrid TItem="DeviceConfigurationListModel"
          DataProvider="LoadData"
          Filterable="true"
          Sortable="true"
          @ref="DataComponent">
    <DataColumns>
        <DataColumn Property="x => x.DeviceId" Title="Device Id" SortIndex="0" />
        <DataColumn Property="x => x.Description" Title="Description" />
        <DataColumn Property="x => x.IsActive" Title="Active" Width="80px" />
    </DataColumns>
</DataGrid>
```

### Restricting the list to the parent

The base component does not know how the child records relate to the parent, so the page restricts the list by overriding `CombineFilter(EntityFilter?)`. This method is called on every page request, after the grid state has been converted to a filter and before the query is sent. Build a parent filter from `Id` and combine it with the grid's filter using `FilterLogic.And` so the user's own grid filters are preserved:

```razor
@code
{
    protected override EntityFilter? CombineFilter(EntityFilter? gridFilter)
    {
        var parentFilter = new EntityFilter
        {
            Name = "DeviceConfigurationBanks.Any(it.BankConfigurationId in @0)",
            Operator = FilterOperators.Expression,
            Value = new[] { Id }
        };

        if (gridFilter == null)
            return parentFilter;

        return new EntityFilter
        {
            Logic = FilterLogic.And,
            Filters = [parentFilter, gridFilter]
        };
    }
}
```

When the relationship is a direct foreign key on the child record, the parent filter is a simple field equality instead:

```csharp
var parentFilter = new EntityFilter
{
    Name = nameof(DeviceConfigurationListModel.BankConfigurationId),
    Operator = FilterOperators.Equal,
    Value = Id
};
```

Returning `gridFilter` unchanged would leave the list unfiltered by the parent, while discarding it would also drop whatever the user typed into the grid filter row — so combine both. A two-parameter convenience overload, `NestedListPageBase<TKey, TParentModel, TListModel>`, is available when the read and list models are the same.

## Next steps

- [Component Services](services.md) — notifications, principal access, and base address resolution
- [Component Extensions](extensions.md) — navigation, data grid, and string helpers
