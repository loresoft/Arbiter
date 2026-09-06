using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Extensions;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides a base class for pages that display a paged list of models in a <see cref="DataGrid{TItem}"/>
/// where the same model type is used for the list and for reading a single record.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify a model</typeparam>
/// <typeparam name="TReadModel">The type of the model displayed in the list and read from the data store</typeparam>
/// <remarks>
/// This is a convenience base class equivalent to
/// <see cref="ListPageBase{TKey, TReadModel, TListModel}"/> with the list and read models being the same type.
/// </remarks>
public abstract class ListPageBase<TKey, TReadModel> : ListPageBase<TKey, TReadModel, TReadModel>
    where TKey : notnull
    where TReadModel : class, IHaveIdentifier<TKey>;

/// <summary>
/// Provides a base class for pages that display a paged list of models in a <see cref="DataGrid{TItem}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify a model</typeparam>
/// <typeparam name="TReadModel">The type of the read model the list items belong to</typeparam>
/// <typeparam name="TListModel">The type of the model displayed in the list</typeparam>
/// <remarks>
/// The data grid requests each page through <see cref="LoadData(DataRequest)"/>, which converts the grid state
/// into an <see cref="EntityQuery"/>. Derived pages adjust the query by overriding
/// <see cref="CombineFilter(EntityFilter?)"/> and load supporting data by overriding
/// <see cref="DataPageBase{TReadModel}.OnLoadedAsync(CancellationToken)"/>, which is raised after every
/// grid refresh.
/// <para>
/// The grid events are subscribed to once <see cref="DataPageBase{TReadModel}.DataComponent"/> is available and are released on dispose, so
/// derived pages only need to assign <see cref="DataPageBase{TReadModel}.DataComponent"/> using an <c>@ref</c> on the grid.
/// </para>
/// </remarks>
public abstract class ListPageBase<TKey, TReadModel, TListModel> : DataPageBase<TListModel>
    where TKey : notnull
    where TReadModel : class
    where TListModel : class, IHaveIdentifier<TKey>
{
    /// <summary>
    /// Gets or sets the service used to display modal dialogs.
    /// </summary>
    [Inject]
    protected ModalService Modal { get; set; } = default!;


    /// <summary>
    /// Loads a page of data for the specified request.
    /// </summary>
    /// <param name="request">The paging, sorting and filtering options requested by the data grid</param>
    /// <returns>The page of data matching the request</returns>
    /// <remarks>
    /// Assign this method to the grid <c>DataProvider</c> parameter. A failure is logged and reported to the
    /// user, and an empty result is returned so the grid remains in a valid state.
    /// </remarks>
    protected virtual async ValueTask<DataResult<TListModel>> LoadData(DataRequest request)
    {
        var cancellationToken = CancellationToken;

        try
        {
            var query = request.ToQuery();
            query.Filter = CombineFilter(query.Filter);

            var results = await DataService.Page<TListModel>(query, cancellationToken: cancellationToken);

            return results.ToResult();
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
            return DataResult<TListModel>.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading {ModelLabel} list: {ErrorMessage}", ModelLabel, ex.Message);
            Notification.ShowError(ex);

            return DataResult<TListModel>.Empty;
        }
    }

    /// <summary>
    /// Combines the filter built by the data grid with the constraints required by the page.
    /// </summary>
    /// <param name="gridFilter">The filter built from the data grid state, or <see langword="null"/> when the grid is unfiltered</param>
    /// <returns>
    /// The filter to send with the query, or <see langword="null"/> to query without a filter. The default
    /// implementation returns <paramref name="gridFilter"/> unchanged.
    /// </returns>
    /// <remarks>
    /// This is called for every page requested by the grid, after the grid state has been converted to an
    /// <see cref="EntityQuery"/> and before the query is sent, so an override runs on each load rather than once.
    /// Override to add a constraint the user cannot remove, such as restricting the list to the selected parent
    /// record, typically by combining both filters with
    /// <see cref="EntityFilterBuilder.CreateGroup(IEnumerable{EntityFilter})"/>. Returning
    /// <paramref name="gridFilter"/> unchanged leaves the grid in control of the filter, while discarding it also
    /// discards what the user typed into the grid filter row.
    /// </remarks>
    protected virtual EntityFilter? CombineFilter(EntityFilter? gridFilter) => gridFilter;


    /// <summary>
    /// Gets the human readable name of the specified list item, for example <c>PO-10432</c>.
    /// </summary>
    /// <param name="model">The list item to describe</param>
    /// <returns>The display name of <paramref name="model"/></returns>
    /// <remarks>
    /// Where <see cref="ModelComponentBase{TReadModel}.ModelLabel"/> names the kind of record, this names the record
    /// itself. It is included in the confirmation prompt and the notification raised by
    /// <see cref="HandleDelete(TListModel)"/>, so it should be short and should identify the record to a user,
    /// such as a name, title or reference number rather than a surrogate key. The default relies on
    /// <typeparamref name="TListModel"/> overriding <see cref="object.ToString"/>; override this method to select
    /// a specific property instead.
    /// </remarks>
    protected virtual string? GetDisplayName(TListModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return model.ToString();
    }


    /// <summary>
    /// Deletes the specified model after confirmation and refreshes the list.
    /// </summary>
    /// <param name="model">The model to delete</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// The user is asked to confirm the delete before it is sent. Refreshing the grid also raises
    /// <see cref="DataPageBase{TReadModel}.OnLoadedAsync(CancellationToken)"/>.
    /// </remarks>
    protected virtual async Task HandleDelete(TListModel model)
    {
        if (model == null)
            return;

        try
        {
            var name = $"{ModelLabel} '{GetDisplayName(model)}'";
            if (!await Modal.ConfirmDelete(name))
                return;

            var cancellationToken = CancellationToken;

            await DataService.Delete<TKey, TReadModel>(model.Id, cancellationToken);

            Notification.ShowSuccess($"{name} deleted successfully");

            await HandleRefresh();
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {ModelLabel} '{ModelId}': {ErrorMessage}", ModelLabel, model.Id, ex.Message);
            Notification.ShowError(ex);
        }
        finally
        {
            await StateChangedAsync();
        }
    }

    /// <summary>
    /// Reloads the list.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Does nothing when <see cref="DataPageBase{TReadModel}.DataComponent"/> has not been assigned. Refreshing the grid also raises
    /// <see cref="DataPageBase{TReadModel}.OnLoadedAsync(CancellationToken)"/>.
    /// </remarks>
    protected virtual async Task HandleRefresh()
    {
        if (DataComponent == null)
            return;

        await DataComponent.RefreshAsync();
    }


    /// <summary>
    /// Handles the <see cref="DataGrid{TItem}.StateResetting"/> event.
    /// The default implementation does nothing.
    /// </summary>
    protected virtual void OnStateResetting()
    {
    }

    /// <summary>
    /// Handles the <see cref="DataGrid{TItem}.StateLoaded"/> event.
    /// The default implementation does nothing.
    /// </summary>
    /// <param name="state">The loaded data grid state</param>
    protected virtual void OnStateLoaded(DataGridState state)
    {
    }

    /// <summary>
    /// Handles the <see cref="DataGrid{TItem}.StateSaving"/> event.
    /// The default implementation does nothing.
    /// </summary>
    /// <param name="state">The data grid state being saved</param>
    protected virtual void OnStateSaving(DataGridState state)
    {
    }


    /// <inheritdoc />
    protected override void SubscribeComponent(DataComponentBase<TListModel>? dataComponent)
    {
        if (dataComponent is DataGrid<TListModel> dataGrid)
        {
            dataGrid.StateSaving += OnStateSaving;
            dataGrid.StateLoaded += OnStateLoaded;
            dataGrid.StateResetting += OnStateResetting;
        }

        base.SubscribeComponent(dataComponent);
    }

    /// <inheritdoc />
    protected override void UnsubscribeComponent(DataComponentBase<TListModel>? dataComponent)
    {
        if (dataComponent is DataGrid<TListModel> dataGrid)
        {
            dataGrid.StateSaving -= OnStateSaving;
            dataGrid.StateLoaded -= OnStateLoaded;
            dataGrid.StateResetting -= OnStateResetting;
        }

        base.UnsubscribeComponent(dataComponent);
    }
}
