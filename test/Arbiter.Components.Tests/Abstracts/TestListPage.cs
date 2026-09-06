using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Abstracts;

using LoreSoft.Blazor.Controls;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// A list model identified by an integer key.
/// </summary>
public record PurchaseOrderListModel : IHaveIdentifier<int>
{
    public int Id { get; set; }

    public string? Number { get; init; }

    public override string ToString() => Number ?? base.ToString()!;
}

/// <summary>
/// A concrete <see cref="ListPageBase{TKey, TReadModel, TListModel}"/> exposing the protected members to the tests.
/// </summary>
public class TestListPage : ListPageBase<int, PurchaseOrderReadModel, PurchaseOrderListModel>
{
    public EntityFilter? CombinedFilter { get; set; }

    public bool UseCombinedFilter { get; set; }

    public ValueTask<DataResult<PurchaseOrderListModel>> PublicLoadData(DataRequest request) => LoadData(request);

    public string? PublicGetDisplayName(PurchaseOrderListModel model) => GetDisplayName(model);

    public EntityFilter? PublicCombineFilter(EntityFilter? gridFilter) => CombineFilter(gridFilter);

    protected override EntityFilter? CombineFilter(EntityFilter? gridFilter)
        => UseCombinedFilter ? CombinedFilter : base.CombineFilter(gridFilter);
}
