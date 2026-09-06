using Arbiter.CommandQuery.Queries;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides a base class for pages that display models in a <see cref="DataComponentBase{TItem}"/> such as a
/// list or a chart, where the whole result set is loaded at once rather than a page at a time.
/// </summary>
/// <typeparam name="TReadModel">The type of the model displayed by the page</typeparam>
/// <remarks>
/// <para>
/// The data component requests its items through <see cref="DataLoader"/>, which delegates to
/// <see cref="LoadData(CancellationToken)"/>. Derived pages shape the request by overriding
/// <see cref="CreateEntityQuery"/> and reload the component by calling <see cref="RefreshData"/>.
/// </para>
/// <para>
/// The component events are subscribed to once
/// <see cref="DataPageBase{TReadModel}.DataComponent"/> is available and are released on dispose, so
/// derived pages only need to assign that property using an <c>@ref</c> on the component.
/// </para>
/// <para>
/// Use <see cref="ListPageBase{TKey, TReadModel, TListModel}"/> instead when the data should be paged, sorted
/// and filtered on the server by a <see cref="DataGrid{TItem}"/>.
/// </para>
/// </remarks>
public abstract class ResultPageBase<TReadModel> : DataPageBase<TReadModel>
    where TReadModel : class
{
    private readonly DebounceAction _debouncer = new();

    /// <summary>
    /// Gets or sets the models returned by the last load.
    /// </summary>
    /// <value>The loaded models, or an empty list when nothing has been loaded or the load failed</value>
    /// <remarks>
    /// The result is retained so that operations acting on the whole result set, such as exporting or
    /// summarizing, do not have to query the data store again.
    /// </remarks>
    protected IReadOnlyList<TReadModel> Data { get; set; } = [];


    /// <summary>
    /// Loads the models for the data component, reporting any failure to the user.
    /// </summary>
    /// <returns>The loaded models, or an empty sequence when the load failed</returns>
    /// <remarks>
    /// Assign this method to the component <c>DataLoader</c> parameter. The whole result set is loaded at once
    /// and the component sorts, filters and pages it in memory. A failure is logged and reported through
    /// <see cref="ModelComponentBase{TReadModel}.Notification"/>, and an empty result is returned so the component
    /// remains in a valid state. Only a failure requests a re-render, because a successful load is already
    /// rendered by the component and observed by
    /// <see cref="DataPageBase{TReadModel}.OnDataRefreshed"/>.
    /// </remarks>
    protected async Task<IEnumerable<TReadModel>> DataLoader()
    {
        var cancellationToken = CancellationToken;

        try
        {
            // need to keep reference to data
            Data = await LoadData(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
            Data = [];
        }
        catch (Exception ex)
        {
            Data = [];

            Logger.LogError(ex, "Error loading {ModelLabel} data: {ErrorMessage}", ModelLabel, ex.Message);
            Notification.ShowError(ex);

            // the component renders the empty result on its own, but the page still needs to show the failure
            await StateChangedAsync();
        }

        return Data;
    }


    /// <summary>
    /// Loads the models from the data store.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>The models matching the query returned by <see cref="CreateEntityQuery"/></returns>
    /// <remarks>
    /// Override to load the models a different way, for example by sending a custom request. Override
    /// <see cref="CreateEntityQuery"/> instead when only the sort or filter needs to change.
    /// </remarks>
    protected virtual async Task<IReadOnlyList<TReadModel>> LoadData(CancellationToken cancellationToken = default)
    {
        // query used to load data; null means load all
        var query = CreateEntityQuery();

        // using page to optionally limit the result set
        var results = await DataService.Page<TReadModel>(query, cancellationToken: cancellationToken);

        return results.Data ?? [];
    }

    /// <summary>
    /// Creates the <see cref="EntityQuery"/> sent to the data store to load the models.
    /// </summary>
    /// <returns>
    /// The query to send, or <see langword="null"/> to load every model the current user is allowed to see. The default
    /// implementation returns <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Override to restrict the result set, for example by applying a filter or a sort. The whole result is loaded
    /// into memory, so a page displaying a large table should apply a filter or page the data instead.
    /// <para>
    /// This is server side: the returned query is part of the request sent by
    /// <see cref="LoadData(CancellationToken)"/>, so it decides which models are fetched. It is evaluated on every
    /// load, which makes it the place for filters the page changes itself, such as a search box, followed by a call
    /// to <see cref="RefreshData"/>.
    /// </para>
    /// <para>
    /// This differs from <see cref="DataPageBase{TReadModel}.CreateDefaultQuery"/>, which returns a
    /// <see cref="QueryRule"/> bound to the component <c>Query</c> parameter and describes the filter implied by the
    /// URL. That rule filters what the component displays from the models already loaded here, so a model excluded by
    /// this query can never be shown by the default query. The two can be used together, for example loading a
    /// tenant's models here and filtering by a query string value there.
    /// </para>
    /// </remarks>
    protected virtual EntityQuery? CreateEntityQuery() => null;


    /// <summary>
    /// Reloads the data component, discarding any cached result.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Calls are debounced, so this can be called from a rapidly changing input such as a search box without
    /// issuing a request for every keystroke. Does nothing when
    /// <see cref="DataPageBase{TReadModel}.DataComponent"/> has not been assigned. The reference is read
    /// again after the debounce delay, so a page disposed or navigated away from while the delay was pending does
    /// not refresh. A failure is logged and reported through
    /// <see cref="ModelComponentBase{TReadModel}.Notification"/> rather than thrown at the caller, because this is
    /// usually awaited by an event handler that cannot report it.
    /// </remarks>
    protected async Task RefreshData()
    {
        if (DataComponent == null)
            return;

        await _debouncer.Debounce(RefreshComponentAsync);
    }


    private async Task RefreshComponentAsync()
    {
        // the component may have been released or the page disposed during the debounce delay
        var dataComponent = DataComponent;
        if (IsDisposed || dataComponent == null)
            return;

        try
        {
            await dataComponent.RefreshAsync(forceReload: true);
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
        }
        catch (ObjectDisposedException)
        {
            // component was disposed while the refresh was running
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refreshing {ModelLabel} data: {ErrorMessage}", ModelLabel, ex.Message);
            Notification.ShowError(ex);
        }
    }
}
