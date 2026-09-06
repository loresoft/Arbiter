using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides the shared behavior for pages that display models in a <see cref="DataComponentBase{TReadModel}"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model the page works with</typeparam>
/// <remarks>
/// This class owns the reference to the data component, subscribes to its events once the reference is
/// available and releases them on dispose, so derived pages only need to assign
/// <see cref="DataComponent"/> using an <c>@ref</c> on the component.
/// <para>
/// Derived pages decide how the items are loaded. <see cref="ListPageBase{TKey, TReadModel, TListModel}"/>
/// loads a page at a time through the component data provider, while
/// <see cref="ResultPageBase{TReadModel}"/> loads the whole result set at once.
/// </para>
/// </remarks>
public abstract class DataPageBase<TReadModel> : ModelComponentBase<TReadModel>
    where TReadModel : class
{
    private DataComponentBase<TReadModel>? _subscribedComponent;

    /// <summary>
    /// Gets or sets the data service used to load models from the data store.
    /// </summary>
    [Inject]
    protected IDispatcherDataService DataService { get; set; } = default!;

    /// <summary>
    /// Gets the dispatcher used to send custom requests.
    /// </summary>
    protected IDispatcher Dispatcher => DataService.Dispatcher;


    /// <summary>
    /// Gets or sets the data component displaying the models.
    /// </summary>
    /// <remarks>
    /// Assign this property using an <c>@ref</c> on the component. The page subscribes to the component events
    /// once the reference is available, including when the component is rendered conditionally after the first
    /// render.
    /// <para>
    /// This is the only assignable component reference; <see cref="DataGrid"/> and <see cref="DataList"/> are
    /// read-only helpers that return this same instance cast to a specific component type.
    /// </para>
    /// </remarks>
    protected DataComponentBase<TReadModel>? DataComponent { get; set; }


    /// <summary>
    /// Gets <see cref="DataComponent"/> cast to a <see cref="DataGrid{TItem}"/>.
    /// </summary>
    /// <value>
    /// The same instance as <see cref="DataComponent"/> when it is a <see cref="DataGrid{TItem}"/>; otherwise
    /// <see langword="null"/>, which is also the case before the component reference has been assigned.
    /// </value>
    /// <remarks>
    /// This is a read-only convenience accessor; it cannot be assigned and holds no state of its own. Assign the
    /// grid using an <c>@ref</c> to <see cref="DataComponent"/>, which is not populated until the page has
    /// rendered.
    /// </remarks>
    protected DataGrid<TReadModel>? DataGrid => DataComponent as DataGrid<TReadModel>;

    /// <summary>
    /// Gets <see cref="DataComponent"/> cast to a <see cref="DataList{TItem}"/>.
    /// </summary>
    /// <value>
    /// The same instance as <see cref="DataComponent"/> when it is a <see cref="DataList{TItem}"/>; otherwise
    /// <see langword="null"/>, which is also the case before the component reference has been assigned.
    /// </value>
    /// <remarks>
    /// This is a read-only convenience accessor; it cannot be assigned and holds no state of its own. Assign the
    /// list using an <c>@ref</c> to <see cref="DataComponent"/>, which is not populated until the page has
    /// rendered.
    /// </remarks>
    protected DataList<TReadModel>? DataList => DataComponent as DataList<TReadModel>;


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
    /// query string therefore reloads the component with the new query, while a parameter change that does not
    /// affect the query leaves the component untouched.
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
    /// from them. Because it runs before the component renders, the query is included in the first request made
    /// by the component data provider. It is called again whenever those values change, for example when the
    /// user navigates to the same page with a different query string, so the query stays in step with the URL.
    /// An override must therefore return the query for the current parameter values rather than depend on being
    /// called once. The returned query is compared with the current <see cref="DefaultQuery"/> and only replaces
    /// it when it differs, so returning an equivalent query does not reload the component.
    /// <para>
    /// This describes the filter implied by the URL and is applied by the component rather than sent to the data
    /// store directly. In <see cref="ListPageBase{TKey, TReadModel, TListModel}"/> the component includes it in the
    /// <see cref="DataRequest"/> passed to the data provider, so it narrows the page requested from the server. In
    /// <see cref="ResultPageBase{TReadModel}"/> the component filters the result already loaded by
    /// <see cref="ResultPageBase{TReadModel}.CreateEntityQuery"/> in memory, so it can only narrow what that query
    /// returned.
    /// </para>
    /// <para>
    /// Use <see cref="ResultPageBase{TReadModel}.CreateEntityQuery"/> to decide which models are fetched, and this
    /// method for the filter carried by the route or query string. Filters the user changes on the page belong in
    /// neither; call the component <c>ApplyFilter</c> method for those.
    /// </para>
    /// </remarks>
    protected virtual QueryRule? CreateDefaultQuery() => null;

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


    /// <summary>
    /// Called after the data component has finished refreshing so the page can load any additional data it
    /// requires. The default implementation does nothing.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// This method is raised from <see cref="OnDataRefreshed"/> for every refresh. Because it runs after the
    /// component has rendered its items, a failure is logged and reported to the user but does not affect the
    /// data already displayed. The component is re-rendered once this method completes.
    /// </remarks>
    protected virtual Task OnLoadedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles the <see cref="DataComponentBase{TReadModel}.DataRefreshed"/> event by running
    /// <see cref="OnLoadedAsync(CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// The event is synchronous, so the load is observed in the background and cannot be awaited by the component.
    /// </remarks>
    protected virtual void OnDataRefreshed() => Observe(OnLoadedAsync);


    /// <summary>
    /// Subscribes to the events raised by the specified data component.
    /// </summary>
    /// <param name="dataComponent">The component to subscribe to, or <see langword="null"/> when there is nothing to subscribe to</param>
    /// <remarks>
    /// Derived classes that subscribe to events declared by a more derived component type should override this
    /// method, subscribe to their own events and then call <c>base.SubscribeComponent(dataComponent)</c>.
    /// </remarks>
    protected virtual void SubscribeComponent(DataComponentBase<TReadModel>? dataComponent)
    {
        if (dataComponent == null)
            return;

        dataComponent.DataRefreshed += OnDataRefreshed;
    }

    /// <summary>
    /// Releases the events subscribed to by <see cref="SubscribeComponent(DataComponentBase{TReadModel})"/>.
    /// </summary>
    /// <param name="dataComponent">The component to unsubscribe from, or <see langword="null"/> when nothing is subscribed</param>
    protected virtual void UnsubscribeComponent(DataComponentBase<TReadModel>? dataComponent)
    {
        if (dataComponent == null)
            return;

        dataComponent.DataRefreshed -= OnDataRefreshed;
    }


    /// <inheritdoc />
    protected override void DisposeManagedResources()
    {
        UnsubscribeComponent(_subscribedComponent);
        _subscribedComponent = null;

        base.DisposeManagedResources();
    }
}
