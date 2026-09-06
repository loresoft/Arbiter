using Arbiter.Components.Abstracts;
using Arbiter.Mapping;

using Microsoft.AspNetCore.Components;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// The update model bound to the edit form in the edit page tests.
/// </summary>
public record InvoiceUpdateModel
{
    public string? Number { get; set; }
}

/// <summary>
/// An <see cref="IMapper"/> that converts between <see cref="InvoiceReadModel"/> and <see cref="InvoiceUpdateModel"/>.
/// </summary>
internal sealed class InvoiceMapper : IMapper
{
    public TDestination? Map<TSource, TDestination>(TSource? source)
    {
        return source switch
        {
            null => default,
            InvoiceReadModel read => (TDestination)(object)new InvoiceUpdateModel { Number = read.Number },
            InvoiceUpdateModel update => (TDestination)(object)new InvoiceReadModel { Number = update.Number },
            _ => throw new NotSupportedException($"Mapping from '{typeof(TSource)}' is not supported."),
        };
    }

    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source is InvoiceReadModel read && destination is InvoiceUpdateModel update)
        {
            update.Number = read.Number;
            return;
        }

        throw new NotSupportedException($"Mapping from '{typeof(TSource)}' is not supported.");
    }

    public IQueryable<TDestination> ProjectTo<TSource, TDestination>(IQueryable<TSource> source)
        => throw new NotSupportedException();
}

/// <summary>
/// A concrete <see cref="EditPageBase{TKey, TReadModel, TUpdateModel}"/> exposing the protected members to the tests.
/// </summary>
public class TestEditPage : EditPageBase<int, InvoiceReadModel, InvoiceUpdateModel>
{
    public int LoadedCount { get; private set; }

    public int CreatedCount { get; private set; }

    public int SavedCount { get; private set; }

    [Parameter]
    public bool AllowSaving { get; set; } = true;

    [Parameter]
    public string? RedirectLocation { get; set; }

    [Parameter]
    public bool EnableUpsert { get; set; }

    public List<RedirectReason> RedirectReasons { get; } = [];

    public InvoiceUpdateModel? PublicModel => Model;

    public InvoiceReadModel? PublicOriginal => Original;

    public bool PublicIsCreate => IsCreate;

    public bool PublicIsDirty => IsDirty;

    public string PublicEditLabel() => EditLabel();

    public string PublicEditTitle() => EditTitle();

    public Task PublicHandleSave() => HandleSave();

    public Task PublicHandleCancel(string? redirect = null) => HandleCancel(redirect);

    public Task PublicHandleRefresh() => HandleRefresh();

    public void ClearEditContext() => EditContext = null;

    protected override bool AllowUpsert => EnableUpsert;

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

    protected override Task OnCreatedAsync(InvoiceUpdateModel model, CancellationToken cancellationToken)
    {
        CreatedCount++;
        return Task.CompletedTask;
    }

    protected override Task<bool> OnSavingAsync(CancellationToken cancellationToken)
        => Task.FromResult(AllowSaving);

    protected override Task OnSavedAsync(CancellationToken cancellationToken)
    {
        SavedCount++;
        return Task.CompletedTask;
    }
}
