using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Abstracts;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// A concrete <see cref="NestedListPageBase{TKey, TParentModel, TReadModel, TListModel}"/> exposing the protected
/// members to the tests.
/// </summary>
public class TestNestedListPage : NestedListPageBase<int, InvoiceReadModel, PurchaseOrderReadModel, PurchaseOrderListModel>
{
    public EntityFilter? CombinedFilter { get; set; }

    public bool UseCombinedFilter { get; set; }

    [Parameter]
    public QueryRule? Query { get; set; }

    public int LoadedCount { get; private set; }

    public int ListLoadedCount { get; private set; }

    public InvoiceReadModel? PublicModel => Model;

    public QueryRule? PublicDefaultQuery => DefaultQuery;

    public DataComponentBase<PurchaseOrderListModel>? PublicDataComponent => DataComponent;

    public ValueTask<DataResult<PurchaseOrderListModel>> PublicLoadData(DataRequest request) => LoadData(request);

    public EntityFilter? PublicCombineFilter(EntityFilter? gridFilter) => CombineFilter(gridFilter);

    public string? PublicGetDisplayName(PurchaseOrderListModel model) => GetDisplayName(model);

    public Task PublicHandleRefresh() => HandleRefresh();

    public Task PublicRefreshList() => RefreshList();

    public Task PublicOnListLoadedAsync() => OnListLoadedAsync(CancellationToken.None);

    protected override QueryRule? CreateDefaultQuery() => Query;

    protected override EntityFilter? CombineFilter(EntityFilter? gridFilter)
        => UseCombinedFilter ? CombinedFilter : base.CombineFilter(gridFilter);

    protected override Task OnLoadedAsync(CancellationToken cancellationToken)
    {
        LoadedCount++;
        return Task.CompletedTask;
    }

    protected override Task OnListLoadedAsync(CancellationToken cancellationToken)
    {
        ListLoadedCount++;
        return Task.CompletedTask;
    }
}
