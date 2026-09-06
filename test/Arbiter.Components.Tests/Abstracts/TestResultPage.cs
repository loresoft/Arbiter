using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Abstracts;

using LoreSoft.Blazor.Controls;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// A concrete <see cref="DataComponentBase{TItem}"/> used only as the reference a page subscribes to.
/// </summary>
/// <remarks>
/// The component is never rendered by the tests, so the services it requires are left unset.
/// </remarks>
public class TestDataComponent : DataComponentBase<PurchaseOrderReadModel>
{
    [SetsRequiredMembers]
    public TestDataComponent()
    {
        JavaScript = default!;
        DownloadService = default!;
        StorageService = default!;
    }
}

/// <summary>
/// A concrete <see cref="ResultPageBase{TReadModel}"/> exposing the protected members to the tests.
/// </summary>
public class TestResultPage : ResultPageBase<PurchaseOrderReadModel>
{
    public List<DataComponentBase<PurchaseOrderReadModel>?> Subscribed { get; } = [];

    public List<DataComponentBase<PurchaseOrderReadModel>?> Unsubscribed { get; } = [];

    public int LoadedCount { get; private set; }

    public EntityQuery? Query { get; set; }

    protected override EntityQuery? CreateEntityQuery() => Query;

    public IReadOnlyList<PurchaseOrderReadModel> PublicData => Data;

    public Task<IEnumerable<PurchaseOrderReadModel>> PublicDataLoader() => DataLoader();

    public Task<IReadOnlyList<PurchaseOrderReadModel>> PublicLoadData(CancellationToken cancellationToken = default)
        => LoadData(cancellationToken);

    public void PublicOnDataRefreshed() => OnDataRefreshed();

    public void SetDataComponent(DataComponentBase<PurchaseOrderReadModel>? dataComponent)
        => DataComponent = dataComponent;

    public void TriggerAfterRender(bool firstRender = false) => OnAfterRender(firstRender);

    protected override Task OnLoadedAsync(CancellationToken cancellationToken)
    {
        LoadedCount++;
        return Task.CompletedTask;
    }

    protected override void SubscribeComponent(DataComponentBase<PurchaseOrderReadModel>? dataComponent)
    {
        Subscribed.Add(dataComponent);
        base.SubscribeComponent(dataComponent);
    }

    protected override void UnsubscribeComponent(DataComponentBase<PurchaseOrderReadModel>? dataComponent)
    {
        Unsubscribed.Add(dataComponent);
        base.UnsubscribeComponent(dataComponent);
    }
}
