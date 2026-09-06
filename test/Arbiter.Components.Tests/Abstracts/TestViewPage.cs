using Arbiter.CommandQuery.Definitions;
using Arbiter.Components.Abstracts;

using Microsoft.AspNetCore.Components;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// A read model identified by an integer key, used by the view page tests.
/// </summary>
public class InvoiceReadModel : IHaveIdentifier<int>
{
    public int Id { get; set; }

    public string? Number { get; set; }

    public override string ToString() => Number ?? base.ToString()!;
}

/// <summary>
/// A concrete <see cref="ViewPageBase{TKey, TReadModel}"/> exposing the protected members to the tests.
/// </summary>
public class TestViewPage : ViewPageBase<int, InvoiceReadModel>
{
    public int LoadedCount { get; private set; }

    [Parameter]
    public string? RedirectLocation { get; set; }

    public List<RedirectReason> RedirectReasons { get; } = [];

    public InvoiceReadModel? PublicModel => Model;

    public bool PublicIsBusy => IsBusy;

    public string? PublicModelDisplay => PageTitle();

    public Task PublicHandleRefresh() => HandleRefresh();

    public void PublicRedirectNotFound() => RedirectNotFound();

    protected override string? GetRedirectLocation(RedirectReason reason, int id)
    {
        RedirectReasons.Add(reason);
        return RedirectLocation;
    }

    protected override Task OnLoadedAsync(CancellationToken cancellationToken)
    {
        LoadedCount++;
        return Task.CompletedTask;
    }
}
