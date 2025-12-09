namespace Arbiter.CommandQuery.State;

/// <summary>
/// Provides state management functionality for a model of type <typeparamref name="TModel"/>.
/// This class manages the model instance and provides change notifications to subscribers.
/// </summary>
/// <typeparam name="TModel">The type of the model being managed by the state. Must have a parameterless constructor.</typeparam>
/// <remarks>
/// The <see cref="ModelStateManager{TModel}"/> class is designed to be used in scenarios where you need to track
/// changes to a model and notify subscribers when the state changes. This is particularly useful in
/// UI scenarios where components need to react to data changes.
/// </remarks>
/// <example>
/// <code>
/// var userState = new StateModel&lt;User&gt;();
/// userState.OnStateChanged += (sender, args) => Console.WriteLine("User state changed");
/// userState.Set(new User { Name = "John Doe" });
/// </code>
/// </example>
public class ModelStateManager<TModel>
    where TModel : new()
{
    /// <summary>
    /// Occurs when the state changes. Subscribers can listen to this event to be notified of state changes.
    /// </summary>
    /// <remarks>
    /// This event is raised whenever the model is modified through the <see cref="Set"/>, <see cref="Clear"/>,
    /// or <see cref="New"/> methods, or when <see cref="NotifyStateChanged"/> is called explicitly.
    /// </remarks>
    public event EventHandler<EventArgs>? OnStateChanged;

    /// <summary>
    /// Gets the current model instance managed by this state.
    /// </summary>
    /// <value>
    /// The current model instance, or <c>null</c> if no model has been set or the model has been cleared.
    /// </value>
    public TModel? Model { get; protected set; }

    /// <summary>
    /// Sets the model to the specified value and notifies subscribers of the change.
    /// </summary>
    /// <param name="model">The model to set. Can be <c>null</c> to clear the current model.</param>
    /// <remarks>
    /// This method will update the <see cref="Model"/> property and automatically trigger the
    /// <see cref="OnStateChanged"/> event to notify all subscribers of the state change.
    /// </remarks>
    public virtual void Set(TModel? model)
    {
        Model = model;
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the current model by setting it to the default value.
    /// </summary>
    /// <remarks>
    /// This method is equivalent to calling <see cref="Set"/> with a default value parameter.
    /// It will trigger the <see cref="OnStateChanged"/> event to notify subscribers of the change.
    /// </remarks>
    public virtual void Clear() => Set(default);

    /// <summary>
    /// Creates a new instance of the model using the parameterless constructor and sets it as the current model.
    /// </summary>
    /// <remarks>
    /// This method creates a new instance of <typeparamref name="TModel"/> using the <c>new()</c> constraint
    /// and sets it as the current model. The <see cref="OnStateChanged"/> event will be triggered to notify
    /// subscribers of the change.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the model type does not have an accessible parameterless constructor.
    /// </exception>
    public virtual void New() => Set(new TModel());

    /// <summary>
    /// Notifies all subscribers that the state has changed by invoking the <see cref="OnStateChanged"/> event.
    /// </summary>
    /// <remarks>
    /// This method can be called manually to trigger state change notifications without actually changing
    /// the model. This is useful when the model's internal state has been modified and subscribers need
    /// to be notified of the change.
    /// </remarks>
    public virtual void NotifyStateChanged() => OnStateChanged?.Invoke(this, EventArgs.Empty);
}
