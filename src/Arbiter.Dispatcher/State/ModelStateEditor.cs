using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;

namespace Arbiter.CommandQuery.State;

/// <summary>
/// Provides state management functionality for editable models that supports full CRUD operations.
/// Manages both the original read model and the editable update model, with comprehensive change tracking and dirty state detection.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify models. Must be non-null.</typeparam>
/// <typeparam name="TReadModel">The type of the read model retrieved from the data store. Must be a reference type that implements <see cref="IHaveIdentifier{TKey}"/> and has a parameterless constructor.</typeparam>
/// <typeparam name="TUpdateModel">The type of the update model used for editing operations. Must be a reference type with a parameterless constructor.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="ModelStateEditor{TKey, TReadModel, TUpdateModel}"/> class is designed for complex editing scenarios where you need
/// to maintain separation between read and update models while providing comprehensive change tracking capabilities.
/// It automatically handles model conversion using the provided mapper and tracks changes using hash codes.
/// </para>
/// <para>
/// This class is particularly useful in Blazor applications where you need to:
/// <list type="bullet">
/// <item>Load data for editing from a data service</item>
/// <item>Track whether the data has been modified (dirty state)</item>
/// <item>Save changes back to the data store</item>
/// <item>Delete existing records</item>
/// <item>Create new records</item>
/// <item>Provide loading indicators during async operations</item>
/// </list>
/// </para>
/// <para>
/// The class maintains two model instances: the <see cref="Original"/> read model represents the unmodified state
/// from the data store, while the <see cref="ModelStateManager{TModel}.Model"/> property contains the editable update model.
/// Change tracking is performed by comparing hash codes between the current model state and the last saved state.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a state manager for User entities with int keys
/// var userEditState = new ModelStateEditor&lt;int, UserReadModel, UserUpdateModel&gt;(dataService, mapper);
///
/// // Subscribe to state changes for UI updates
/// userEditState.OnStateChanged += (sender, args) => StateHasChanged();
///
/// // Load a user for editing
/// await userEditState.Load(123);
///
/// // Check if user is currently being loaded
/// if (userEditState.IsBusy)
/// {
///     // Show loading indicator
/// }
///
/// // Check if user has unsaved changes
/// if (userEditState.IsDirty)
/// {
///     // Show save/discard options
/// }
///
/// // Save changes
/// await userEditState.Save();
///
/// // Delete the user
/// await userEditState.Delete();
/// </code>
/// </example>
public class ModelStateEditor<TKey, TReadModel, TUpdateModel> : ModelStateManager<TUpdateModel>
    where TKey : notnull
    where TReadModel : class, IHaveIdentifier<TKey>, new()
    where TUpdateModel : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelStateEditor{TKey, TReadModel, TUpdateModel}"/> class.
    /// </summary>
    /// <param name="dataService">The data service used for CRUD operations on the data store</param>
    /// <param name="mapper">The mapper used to convert between read and update models</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="dataService"/> or <paramref name="mapper"/> is <c>null</c>
    /// </exception>
    public ModelStateEditor(IDispatcherDataService dataService, IMapper mapper)
    {
        DataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Gets the data service used for CRUD operations on the data store.
    /// </summary>
    /// <value>
    /// The <see cref="IDispatcherDataService"/> instance used for data operations including get, save, and delete operations.
    /// </value>
    public IDispatcherDataService DataService { get; }

    /// <summary>
    /// Gets the mapper used to convert between read and update models.
    /// </summary>
    /// <value>
    /// The <see cref="IMapper"/> instance used to map between <typeparamref name="TReadModel"/> and <typeparamref name="TUpdateModel"/>.
    /// </value>
    public IMapper Mapper { get; }

    /// <summary>
    /// Gets the dispatcher from the data service for sending custom requests.
    /// </summary>
    /// <value>
    /// The <see cref="IDispatcher"/> instance from the <see cref="DataService"/> that can be used for custom request handling.
    /// </value>
    public IDispatcher Dispatcher => DataService.Dispatcher;

    /// <summary>
    /// Gets the original read model that was loaded from the data store.
    /// This represents the unmodified state of the model before any edits were made.
    /// </summary>
    /// <value>
    /// The original <typeparamref name="TReadModel"/> instance as loaded from the data store,
    /// or <c>null</c> if no model has been loaded or the state has been cleared.
    /// </value>
    /// <remarks>
    /// This property is used to maintain a reference to the original data for comparison purposes
    /// and to provide the identifier for save and delete operations.
    /// </remarks>
    public TReadModel? Original { get; protected set; }

    /// <summary>
    /// Gets the hash code of the model when it was last saved or loaded.
    /// Used for change tracking to determine if the model has been modified.
    /// </summary>
    /// <value>
    /// The hash code of the update model at the time it was last saved or loaded.
    /// A value of 0 indicates no model is currently loaded or the state has been cleared.
    /// </value>
    /// <remarks>
    /// This hash is automatically updated whenever a model is loaded, saved, or explicitly set.
    /// It is used by the <see cref="IsDirty"/> and <see cref="IsClean"/> properties to determine
    /// if the current model state differs from the last saved state.
    /// </remarks>
    public int EditHash { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the state is currently performing a data operation.
    /// </summary>
    /// <value>
    /// <c>true</c> if a load, save, or delete operation is currently in progress; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is automatically managed during asynchronous operations and can be used
    /// to show loading indicators in the user interface or to prevent concurrent operations.
    /// </remarks>
    public bool IsBusy { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the current model has been modified since it was last saved or loaded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the current model's hash code differs from the <see cref="EditHash"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property uses hash code comparison to detect changes in the model. It will return <c>true</c>
    /// if any modifications have been made to the model since it was last loaded or saved.
    /// </para>
    /// <para>
    /// Note that this is a shallow comparison based on the model's <see cref="object.GetHashCode"/> implementation.
    /// For accurate change detection, ensure that the update model type properly implements <see cref="object.GetHashCode"/>.
    /// </para>
    /// </remarks>
    public bool IsDirty => Model?.GetHashCode() != EditHash;

    /// <summary>
    /// Gets a value indicating whether the current model has not been modified since it was last saved or loaded.
    /// </summary>
    /// <value>
    /// <c>true</c> if the current model's hash code matches the <see cref="EditHash"/>; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is the logical inverse of <see cref="IsDirty"/> and provides a convenient way
    /// to check if the model is in a clean (unmodified) state.
    /// </remarks>
    public bool IsClean => Model?.GetHashCode() == EditHash;

    /// <summary>
    /// Sets the update model to the specified value and updates the original model and edit hash accordingly.
    /// </summary>
    /// <param name="model">The update model to set. Can be <c>null</c> to clear the current model.</param>
    /// <remarks>
    /// <para>
    /// When a non-null model is provided, this method:
    /// <list type="number">
    /// <item>Sets the <see cref="ModelStateManager{TModel}.Model"/> property to the provided update model</item>
    /// <item>Maps the update model to a read model and stores it in <see cref="Original"/></item>
    /// <item>Calculates and stores the model's hash code in <see cref="EditHash"/> for change tracking</item>
    /// <item>Triggers the <see cref="ModelStateManager{TModel}.OnStateChanged"/> event</item>
    /// </list>
    /// </para>
    /// <para>
    /// When <c>null</c> is provided, all properties are reset to their default values.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the mapper fails to convert between the update and read model types.
    /// </exception>
    public override void Set(TUpdateModel? model)
    {
        if (model is null)
        {
            Model = null;
            Original = null;
            EditHash = 0;
        }
        else
        {
            Model = model;

            // convert update to read model
            Original = Mapper.Map<TUpdateModel, TReadModel>(model);

            // save hash to track changes
            EditHash = Model.GetHashCode();
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the current model and resets all state properties to their default values.
    /// </summary>
    /// <remarks>
    /// This method resets the <see cref="ModelStateManager{TModel}.Model"/>, <see cref="Original"/>, and <see cref="EditHash"/>
    /// properties and triggers the <see cref="ModelStateManager{TModel}.OnStateChanged"/> event to notify subscribers.
    /// </remarks>
    public override void Clear()
    {
        SetModel(default);
        NotifyStateChanged();
    }

    /// <summary>
    /// Creates a new read model instance and sets it as the current model for editing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method creates a new instance of <typeparamref name="TReadModel"/> using the parameterless constructor,
    /// then converts it to an update model for editing. This is typically used when creating new records.
    /// </para>
    /// <para>
    /// After calling this method, <see cref="IsDirty"/> will return <c>false</c> since the model represents
    /// a clean state for a new entity.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the read model type does not have an accessible parameterless constructor or if the mapping fails.
    /// </exception>
    public override void New()
    {
        SetModel(new TReadModel());
        NotifyStateChanged();
    }

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
    /// During the load operation:
    /// <list type="number">
    /// <item><see cref="IsBusy"/> is set to <c>true</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called</item>
    /// <item>The model is retrieved from the data service</item>
    /// <item>The model is converted to an update model and change tracking is initialized</item>
    /// <item><see cref="IsBusy"/> is reset to <c>false</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called again</item>
    /// </list>
    /// </para>
    /// <para>
    /// After a successful load, <see cref="IsDirty"/> will return <c>false</c> since the model represents
    /// the current state from the data store.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <c>null</c></exception>
    /// <exception cref="InvalidOperationException">Thrown when the data service is not properly configured</exception>
    /// <exception cref="InvalidOperationException">Thrown when the mapping between read and update models fails</exception>
    /// <example>
    /// <code>
    /// // Load a model for the first time
    /// await stateEdit.Load(123);
    ///
    /// // This call will be skipped because the same ID is already loaded
    /// await stateEdit.Load(123);
    ///
    /// // Force reload even though the same ID is already loaded
    /// await stateEdit.Load(123, force: true);
    /// </code>
    /// </example>
    public async Task Load(TKey id, bool force = false)
    {
        // don't load if already loaded
        if (!force && Original != null && EqualityComparer<TKey>.Default.Equals(id, Original.Id))
            return;

        try
        {
            IsBusy = true;
            NotifyStateChanged();

            // load read modal
            var readModel = await DataService
                .Get<TKey, TReadModel>(id)
                .ConfigureAwait(false);

            SetModel(readModel);
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
    /// During the load operation:
    /// <list type="number">
    /// <item><see cref="IsBusy"/> is set to <c>true</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called</item>
    /// <item>The model is retrieved from the data service using the alternate key</item>
    /// <item>The model is converted to an update model and change tracking is initialized</item>
    /// <item><see cref="IsBusy"/> is reset to <c>false</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called again</item>
    /// </list>
    /// </para>
    /// <para>
    /// After a successful load, <see cref="IsDirty"/> will return <c>false</c> since the model represents
    /// the current state from the data store.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is an empty <see cref="Guid"/></exception>
    /// <exception cref="InvalidOperationException">Thrown when the data service is not properly configured</exception>
    /// <exception cref="InvalidOperationException">Thrown when the mapping between read and update models fails</exception>
    public async Task LoadKey(Guid key, bool force = false)
    {
        // don't load if already loaded
        if (!force && Original != null && Original is IHaveKey keyed && keyed.Key == key)
            return;

        try
        {
            IsBusy = true;
            NotifyStateChanged();

            // load read modal
            var readModel = await DataService
                .GetKey<TReadModel>(key)
                .ConfigureAwait(false);

            SetModel(readModel);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Asynchronously saves the current model to the data store. If the model has an identifier, it will be updated; otherwise, it will be created.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous save operation</returns>
    /// <remarks>
    /// <para>
    /// This method performs an upsert operation:
    /// <list type="bullet">
    /// <item>If <see cref="Original"/> contains a model with a valid identifier, an update operation is performed</item>
    /// <item>If <see cref="Original"/> is <c>null</c> or has a default identifier, a create operation is performed</item>
    /// </list>
    /// </para>
    /// <para>
    /// During the save operation:
    /// <list type="number">
    /// <item><see cref="IsBusy"/> is set to <c>true</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called</item>
    /// <item>The current update model is sent to the data service for saving</item>
    /// <item>The returned read model is converted back to an update model and change tracking is reset</item>
    /// <item><see cref="IsBusy"/> is reset to <c>false</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called again</item>
    /// </list>
    /// </para>
    /// <para>
    /// After a successful save, <see cref="IsDirty"/> will return <c>false</c> since the model state
    /// matches the saved state in the data store.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the data service save operation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when the mapping between models fails</exception>
    /// <example>
    /// <code>
    /// // Modify the loaded model
    /// stateEdit.Model.Name = "Updated Name";
    ///
    /// // Check if changes need to be saved
    /// if (stateEdit.IsDirty)
    /// {
    ///     // Save the changes
    ///     await stateEdit.Save();
    /// }
    /// </code>
    /// </example>
    public async Task Save()
    {
        if (Model is null)
            return;

        try
        {
            IsBusy = true;
            NotifyStateChanged();

            // get key from original
            var key = Original != null ? Original.Id : default;

            var readModel = await DataService
                .Save<TKey, TUpdateModel, TReadModel>(key!, Model!)
                .ConfigureAwait(false);

            SetModel(readModel);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Asynchronously deletes the current model from the data store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous delete operation</returns>
    /// <remarks>
    /// <para>
    /// This method can only delete models that have been previously loaded (i.e., have a valid <see cref="Original"/> reference).
    /// If either <see cref="ModelStateManager{TModel}.Model"/> or <see cref="Original"/> is <c>null</c>, the method returns without performing any operation.
    /// </para>
    /// <para>
    /// During the delete operation:
    /// <list type="number">
    /// <item><see cref="IsBusy"/> is set to <c>true</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called</item>
    /// <item>The delete request is sent to the data service using the identifier from <see cref="Original"/></item>
    /// <item>Upon successful deletion, the state is cleared by calling <see cref="SetModel"/> with <c>null</c></item>
    /// <item><see cref="IsBusy"/> is reset to <c>false</c> and <see cref="ModelStateManager{TModel}.NotifyStateChanged"/> is called again</item>
    /// </list>
    /// </para>
    /// <para>
    /// After a successful delete, all properties are reset to their default values.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the data service delete operation fails</exception>
    /// <example>
    /// <code>
    /// // Load a model for deletion
    /// await stateEdit.Load(123);
    ///
    /// // Confirm deletion with user, then delete
    /// if (confirmDelete)
    /// {
    ///     await stateEdit.Delete();
    /// }
    /// </code>
    /// </example>
    public async Task Delete()
    {
        if (Model is null || Original is null)
            return;

        try
        {
            IsBusy = true;
            NotifyStateChanged();

            // get key from original
            await DataService
                .Delete<TKey, TReadModel>(Original.Id)
                .ConfigureAwait(false);

            SetModel(default);
        }
        finally
        {
            IsBusy = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Sets the read model and converts it to an update model for editing.
    /// Updates the original model reference and edit hash for change tracking.
    /// </summary>
    /// <param name="model">The read model to set. Can be <c>null</c> to clear all state.</param>
    /// <remarks>
    /// <para>
    /// This protected method is used internally to set the model state after load and save operations.
    /// It ensures that both the read and update model references are properly maintained and that
    /// change tracking is correctly initialized.
    /// </para>
    /// <para>
    /// When a non-null model is provided:
    /// <list type="number">
    /// <item>The <see cref="Original"/> property is set to the provided read model</item>
    /// <item>The read model is converted to an update model using <see cref="Mapper"/></item>
    /// <item>The <see cref="ModelStateManager{TModel}.Model"/> property is set to the converted update model</item>
    /// <item>The <see cref="EditHash"/> is calculated and stored for change tracking</item>
    /// </list>
    /// </para>
    /// <para>
    /// When <c>null</c> is provided, all properties are reset to their default values.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the mapper fails to convert from the read model to the update model.
    /// </exception>
    protected void SetModel(TReadModel? model)
    {
        if (model is null)
        {
            Original = null;
            Model = null;
            EditHash = 0;
        }
        else
        {
            Original = model;

            // convert read to update model
            Model = Mapper.Map<TReadModel, TUpdateModel>(model);

            // save hash to track changes
            EditHash = Model.GetHashCode();
        }
    }
}
