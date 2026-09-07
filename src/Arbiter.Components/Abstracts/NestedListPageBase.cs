using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Queries;
using Arbiter.Components.Extensions;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides a base class for pages that display a single parent model together with a paged list of related
/// models, where the same model type is used for the list and for reading a single related record.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify a model</typeparam>
/// <typeparam name="TParentModel">The type of the read model loaded by <see cref="ViewPageBase{TKey, TReadModel}.Id"/></typeparam>
/// <typeparam name="TListModel">The type of the model displayed in the list and read from the data store</typeparam>
/// <remarks>
/// This is a convenience base class equivalent to
/// <see cref="NestedListPageBase{TKey, TParentModel, TReadModel, TListModel}"/> with the list and read models
/// being the same type.
/// </remarks>
public abstract class NestedListPageBase<TKey, TParentModel, TListModel> : NestedListPageBase<TKey, TParentModel, TListModel, TListModel>
    where TKey : notnull
    where TParentModel : class, IHaveIdentifier<TKey>, new()
    where TListModel : class, IHaveIdentifier<TKey>;

/// <summary>
/// Provides a base class for pages that display a single parent model together with a paged list of related
/// models in a <see cref="DataGrid{TItem}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify a model</typeparam>
/// <typeparam name="TParentModel">The type of the read model loaded by <see cref="ViewPageBase{TKey, TReadModel}.Id"/></typeparam>
/// <typeparam name="TReadModel">The type of the read model the list items belong to</typeparam>
/// <typeparam name="TListModel">The type of the model displayed in the list</typeparam>
/// <remarks>
/// The parent model is loaded by <see cref="ViewPageBase{TKey, TReadModel}"/> when
/// <see cref="ViewPageBase{TKey, TReadModel}.Id"/> is set or changed, while the data grid requests each page of
/// related models through <see cref="LoadData(DataRequest)"/>. Derived pages restrict the list to the parent
/// record by overriding <see cref="CombineFilter(EntityFilter?)"/>.
/// <para>
/// The grid events are subscribed to once <see cref="DataComponent"/> is available and are released on dispose,
/// so derived pages only need to assign <see cref="DataComponent"/> using an <c>@ref</c> on the grid.
/// </para>
/// </remarks>
public abstract class NestedListPageBase<TKey, TParentModel, TReadModel, TListModel> : ViewPageBase<TKey, TParentModel>
    where TKey : notnull
    where TParentModel : class, IHaveIdentifier<TKey>, new()
    where TReadModel : class
    where TListModel : class, IHaveIdentifier<TKey>
{
    private DataComponentBase<TListModel>? _subscribedComponent;

    /// <summary>
    /// Gets or sets the service used to display modal dialogs.
    /// </summary>
    [Inject]
    protected ModalService Modal { get; set; } = default!;


    /// <summary>
    /// Gets or sets the data component displaying the related models.
    /// </summary>
    /// <remarks>
    /// Assign this property using an <c>@ref</c> on the component. The page subscribes to the component events
    /// once the reference is available, including when the component is rendered conditionally after the first
    /// render.
    /// </remarks>
    protected DataComponentBase<TListModel>? DataComponent { get; set; }

    /// <summary>
    /// Gets <see cref="DataComponent"/> cast to a <see cref="DataGrid{TItem}"/>.
    /// </summary>
    /// <value>
    /// The same instance as <see cref="DataComponent"/> when it is a <see cref="DataGrid{TItem}"/>; otherwise
    /// <see langword="null"/>, which is also the case before the component reference has been assigned.
    /// </value>
    protected DataGrid<TListModel>? DataGrid => DataComponent as DataGrid<TListModel>;

    /// <summary>
    /// Gets <see cref="DataComponent"/> cast to a <see cref="DataList{TItem}"/>.
    /// </summary>
    /// <value>
    /// The same instance as <see cref="DataComponent"/> when it is a <see cref="DataList{TItem}"/>; otherwise
    /// <see langword="null"/>, which is also the case before the component reference has been assigned.
    /// </value>
    protected DataList<TListModel>? DataList => DataComponent as DataList<TListModel>;


    /// <summary>
    /// Gets the query applied to the data component before it loads.
    /// </summary>
    /// <value>
    /// The query built by <see cref="CreateDefaultQuery"/> for the current page parameters, or
    /// <see langword="null"/> when no query is required. The default implementation of
    /// <see cref="CreateDefaultQuery"/> returns <see langword="null"/>, so this is <see langword="null"/> unless
    /// a derived page overrides it.
    /// </value>
    /// <remarks>
    /// <para>
    /// Bind this to the component <c>Query</c> parameter, for example
    /// <c>&lt;DataGrid Query="@DefaultQuery" ... /&gt;</c>, so the query is part of the very first request the
    /// component makes rather than causing a second load. The value is available before the first render, so the
    /// component never loads without it.
    /// </para>
    /// <para>
    /// The value is recomputed in <see cref="OnParametersSet"/> whenever the page parameters change, and is only
    /// replaced when the new query differs from the current one. Navigating to the same page with a different
    /// parent identifier therefore reloads the component with the new query, while a parameter change that does
    /// not affect the query leaves the component untouched.
    /// </para>
    /// <para>
    /// This represents the query implied by the URL, so it should not be used for filters the user changes on the
    /// page; call the component <c>ApplyFilter</c> method for those instead.
    /// </para>
    /// </remarks>
    protected QueryRule? DefaultQuery { get; private set; }

    /// <summary>
    /// Creates the query applied to the data component before it loads for the first time.
    /// </summary>
    /// <returns>
    /// The query to apply, or <see langword="null"/> to load without an initial filter. The default
    /// implementation returns <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// This is called from <see cref="OnParametersSet"/>, after route values and parameters marked with
    /// <see cref="SupplyParameterFromQueryAttribute"/> have been assigned, so an override can build the query
    /// from them, typically restricting the list to the parent record identified by
    /// <see cref="ViewPageBase{TKey, TReadModel}.Id"/>. Because it runs before the component renders, the query is
    /// included in the first request made by the component data provider. It is called again whenever those values
    /// change, so an override must return the query for the current parameter values rather than depend on being
    /// called once. The returned query is compared with the current <see cref="DefaultQuery"/> and only replaces
    /// it when it differs, so returning an equivalent query does not reload the component.
    /// <para>
    /// This query is applied by the component and is included in the <see cref="DataRequest"/> passed to
    /// <see cref="LoadData(DataRequest)"/>, whereas <see cref="CombineFilter(EntityFilter?)"/> changes the query
    /// sent to the data store after the grid state has been converted.
    /// </para>
    /// </remarks>
    protected virtual QueryRule? CreateDefaultQuery() => null;


    /// <summary>
    /// Loads a page of related data for the specified request.
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
    /// Override to restrict the list to the parent record identified by
    /// <see cref="ViewPageBase{TKey, TReadModel}.Id"/>, typically by combining both filters with
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
    /// Where <see cref="ModelComponentBase{TReadModel}.ModelLabel"/> names the kind of record, this names the
    /// record itself. It is included in the confirmation prompt and the notification raised by
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
    /// Deletes the specified related model after confirmation and refreshes the list.
    /// </summary>
    /// <param name="model">The model to delete</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// The user is asked to confirm the delete before it is sent. Refreshing the grid also raises
    /// <see cref="OnListLoadedAsync(CancellationToken)"/>.
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

            await RefreshList();
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

    /// <inheritdoc />
    /// <remarks>
    /// Reloads the parent model and then the list displayed by <see cref="DataComponent"/>.
    /// </remarks>
    protected override async Task HandleRefresh()
    {
        await base.HandleRefresh();

        await RefreshList();
    }

    /// <summary>
    /// Reloads the list displayed by <see cref="DataComponent"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Nothing is refreshed when the component reference has not been assigned yet.
    /// </remarks>
    protected Task RefreshList() => DataComponent?.RefreshAsync() ?? Task.CompletedTask;


    /// <summary>
    /// Called after the data component has finished refreshing so the page can load any additional data it
    /// requires. The default implementation does nothing.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// This method is raised from <see cref="OnDataRefreshed"/> for every refresh of the list, whereas
    /// <see cref="ViewPageBase{TKey, TReadModel}.OnLoadedAsync(CancellationToken)"/> is raised after the parent
    /// model has been loaded. Because it runs after the component has rendered its items, a failure is logged and
    /// reported to the user but does not affect the data already displayed.
    /// </remarks>
    protected virtual Task OnListLoadedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the <see cref="DataComponentBase{TReadModel}.DataRefreshed"/> event by running
    /// <see cref="OnListLoadedAsync(CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// The event is synchronous, so the load is observed in the background and cannot be awaited by the component.
    /// </remarks>
    protected virtual void OnDataRefreshed() => Observe(OnListLoadedAsync);

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


    /// <summary>
    /// Subscribes to the events raised by the specified data component.
    /// </summary>
    /// <param name="dataComponent">The component to subscribe to, or <see langword="null"/> when there is nothing to subscribe to</param>
    /// <remarks>
    /// Derived classes that subscribe to events declared by a more derived component type should override this
    /// method, subscribe to their own events and then call <c>base.SubscribeComponent(dataComponent)</c>.
    /// </remarks>
    protected virtual void SubscribeComponent(DataComponentBase<TListModel>? dataComponent)
    {
        if (dataComponent == null)
            return;

        if (dataComponent is DataGrid<TListModel> dataGrid)
        {
            dataGrid.StateSaving += OnStateSaving;
            dataGrid.StateLoaded += OnStateLoaded;
            dataGrid.StateResetting += OnStateResetting;
        }

        dataComponent.DataRefreshed += OnDataRefreshed;
    }

    /// <summary>
    /// Releases the events subscribed to by <see cref="SubscribeComponent(DataComponentBase{TListModel})"/>.
    /// </summary>
    /// <param name="dataComponent">The component to unsubscribe from, or <see langword="null"/> when nothing is subscribed</param>
    protected virtual void UnsubscribeComponent(DataComponentBase<TListModel>? dataComponent)
    {
        if (dataComponent == null)
            return;

        if (dataComponent is DataGrid<TListModel> dataGrid)
        {
            dataGrid.StateSaving -= OnStateSaving;
            dataGrid.StateLoaded -= OnStateLoaded;
            dataGrid.StateResetting -= OnStateResetting;
        }

        dataComponent.DataRefreshed -= OnDataRefreshed;
    }


    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var defaultQuery = CreateDefaultQuery();

        // only replace the query when it differs, a new instance makes the component reload
        if (!EqualityComparer<QueryRule>.Default.Equals(DefaultQuery, defaultQuery))
            DefaultQuery = defaultQuery;

        base.OnParametersSet();
    }

    /// <inheritdoc />
    protected override void OnAfterRender(bool firstRender)
    {
        // the component reference may be assigned after the first render when it is conditionally rendered
        if (!ReferenceEquals(_subscribedComponent, DataComponent))
        {
            UnsubscribeComponent(_subscribedComponent);
            SubscribeComponent(DataComponent);

            _subscribedComponent = DataComponent;
        }

        base.OnAfterRender(firstRender);
    }

    /// <inheritdoc />
    protected override void DisposeManagedResources()
    {
        UnsubscribeComponent(_subscribedComponent);
        _subscribedComponent = null;

        base.DisposeManagedResources();
    }
}
