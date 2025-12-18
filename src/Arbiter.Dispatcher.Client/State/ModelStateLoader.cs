using Arbiter.CommandQuery.Definitions;

namespace Arbiter.Dispatcher.State;

/// <summary>
/// Provides state management functionality for read-only operations on models that implement <see cref="IHaveIdentifier{TKey}"/>.
/// Extends <see cref="ModelStateManager{TModel}"/> to include asynchronous data loading capabilities from a data service.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify models</typeparam>
/// <typeparam name="TModel">The type of the model being managed. Must be a reference type that implements <see cref="IHaveIdentifier{TKey}"/> and has a parameterless constructor</typeparam>
/// <remarks>
/// The <see cref="ModelStateLoader{TKey, TModel}"/> class is designed for scenarios where you need to load and manage
/// read-only data from a data store. It provides automatic loading with duplicate request prevention,
/// busy state tracking, and change notifications inherited from the base <see cref="ModelStateManager{TModel}"/> class.
/// This is particularly useful in Blazor applications where components need to react to data loading states.
/// </remarks>
/// <example>
/// <code>
/// // Create a state manager for User entities with string keys
/// var userState = new ModelStateLoader&lt;string, User&gt;(dataService);
///
/// // Subscribe to state changes
/// userState.OnStateChanged += (sender, args) => StateHasChanged();
///
/// // Load a user by ID
/// await userState.Load("user123");
///
/// // Check if loading is in progress
/// if (userState.IsBusy)
/// {
///     // Show loading indicator
/// }
/// else if (userState.Model != null)
/// {
///     // Display the loaded user data
/// }
/// </code>
/// </example>
public class ModelStateLoader<TKey, TModel> : ModelStateManager<TModel>
    where TModel : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelStateLoader{TKey, TModel}"/> class.
    /// </summary>
    /// <param name="dataService">The data service used to load models from the data store</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataService"/> is <c>null</c></exception>
    public ModelStateLoader(IDispatcherDataService dataService)
    {
        DataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    /// <summary>
    /// Gets the data service used to load models from the data store.
    /// </summary>
    /// <value>
    /// The <see cref="IDispatcherDataService"/> instance used for data operations.
    /// </value>
    public IDispatcherDataService DataService { get; }

    /// <summary>
    /// Gets a value indicating whether the state is currently performing a data operation.
    /// </summary>
    /// <value>
    /// <c>true</c> if a load operation is currently in progress; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is automatically set to <c>true</c> when <see cref="Load"/> is called and reset to <c>false</c>
    /// when the operation completes, regardless of success or failure. It can be used to show loading indicators
    /// in the user interface.
    /// </remarks>
    public bool IsBusy { get; protected set; }

    /// <summary>
    /// Asynchronously loads a model with the specified identifier from the data service.
    /// </summary>
    /// <param name="id">The identifier of the model to load</param>
    /// <param name="force">
    /// <c>true</c> to force a reload even if the model is already loaded with the same identifier;
    /// <c>false</c> to skip loading if the same model is already present
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous load operation</returns>
    /// <remarks>
    /// <para>
    /// This method implements intelligent caching behavior. If <paramref name="force"/> is <c>false</c> and a model
    /// with the same identifier is already loaded, the method returns immediately without making a data service call.
    /// </para>
    /// <para>
    /// During the load operation, <see cref="IsBusy"/> is set to <c>true</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/>
    /// is called to notify subscribers. Upon completion, <see cref="IsBusy"/> is reset to <c>false</c> and
    /// <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called again.
    /// </para>
    /// <para>
    /// If the load operation succeeds, the loaded model is automatically set using the inherited <see cref="ModelStateManager{TModel}.Model"/> property.
    /// If the operation fails, the model remains unchanged and the exception is propagated to the caller.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c></exception>
    /// <exception cref="InvalidOperationException">Thrown when the data service is not properly configured</exception>
    /// <example>
    /// <code>
    /// // Load a model for the first time
    /// await stateLoader.Load("user123");
    ///
    /// // This call will be skipped because the same ID is already loaded
    /// await stateLoader.Load("user123");
    ///
    /// // Force reload even though the same ID is already loaded
    /// await stateLoader.Load("user123", force: true);
    /// </code>
    /// </example>
    public async Task Load(TKey id, bool force = false)
    {
        // don't load if already loaded
        if (!force && Model != null && EqualityComparer<TKey>.Default.Equals(id, Model.Id))
            return;

        try
        {
            IsBusy = true;
            NotifyStateChanged();

            // load modal
            Model = await DataService.Get<TKey, TModel>(id).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Asynchronously loads a model with the specified globally unique alternate key from the data service.
    /// </summary>
    /// <param name="key">The globally unique alternate key of the model to load</param>
    /// <param name="force">
    /// <c>true</c> to force a reload even if the model is already loaded with the same alternate key;
    /// <c>false</c> to skip loading if the same model is already present
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous load operation</returns>
    /// <remarks>
    /// <para>
    /// This method loads models using their alternate key (<see cref="IHaveKey.Key"/>) rather than their primary identifier.
    /// If <paramref name="force"/> is <c>false</c> and a model with the same alternate key is already loaded,
    /// the method returns immediately without making a data service call.
    /// </para>
    /// <para>
    /// During the load operation, <see cref="IsBusy"/> is set to <c>true</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/>
    /// is called to notify subscribers. Upon completion, <see cref="IsBusy"/> is reset to <c>false</c> and
    /// <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called again.
    /// </para>
    /// <para>
    /// If the load operation succeeds, the loaded model is automatically set using the inherited <see cref="ModelStateManager{TModel}.Model"/> property.
    /// If the operation fails, the model remains unchanged and the exception is propagated to the caller.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is an empty <see cref="Guid"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when the data service is not properly configured</exception>
    public async Task LoadKey(Guid key, bool force = false)
    {
        // don't load if already loaded
        if (!force && Model != null && Model is IHaveKey keyed && keyed.Key == key)
            return;

        try
        {
            IsBusy = true;
            NotifyStateChanged();

            // load modal
            Model = await DataService
                .GetKey<TModel>(key)
                .ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }
}
