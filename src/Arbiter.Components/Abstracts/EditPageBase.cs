using Arbiter.CommandQuery.Definitions;
using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;
using Arbiter.Dispatcher.State;

using LoreSoft.Blazor.Controls;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides a base class for pages that create or edit a single model loaded by its identifier
/// using a <see cref="ModelStateEditor{TKey, TReadModel, TUpdateModel}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify the model</typeparam>
/// <typeparam name="TReadModel">
/// The type of the read model loaded from the data store. Must expose a parameterless constructor and its
/// identifier through <see cref="IHaveIdentifier{TKey}"/>.
/// </typeparam>
/// <typeparam name="TUpdateModel">
/// The type of the update model bound to the edit form. Must expose a parameterless constructor and a
/// value based <see cref="object.GetHashCode"/>.
/// </typeparam>
/// <remarks>
/// <para>
/// A default <see cref="Id"/> value starts a create operation; any other value
/// loads the existing model. The page maintains an <see cref="EditContext"/> for the current update model, warns
/// before navigating away with unsaved changes, and exposes save, cancel and delete operations. Derived pages
/// load supporting data by overriding <see cref="OnLoadedAsync(CancellationToken)"/>.
/// </para>
/// <para>
/// Two model types are used. <typeparamref name="TReadModel"/> is what the data store returns and is kept as
/// <c>Store.Original</c>; it is the read only snapshot of the record as it exists on the server, including
/// server owned values such as the identifier, row version and audit fields.
/// <typeparamref name="TUpdateModel"/> is what the user edits and is exposed as <see cref="Model"/>; it
/// contains only the fields the form is allowed to change.
/// </para>
/// <para>
/// Whenever a model is loaded or created, the store maps the read model to a new update model and takes a
/// snapshot of its hash code. Editing <see cref="Model"/> therefore never affects <c>Store.Original</c>, which
/// is why cancelling can restore the original values and why <see cref="ModelDisplay"/> reads from the
/// original rather than from the edited copy. Saving sends the update model together with the identifier taken
/// from the original, then replaces both models with the read model returned by the server, so values assigned
/// during save such as a generated key or a new row version are picked up automatically.
/// </para>
/// <para>
/// Unsaved change tracking compares the current hash code of <see cref="Model"/> against the snapshot, so
/// <typeparamref name="TUpdateModel"/> must implement <see cref="object.GetHashCode"/> by value. Declaring it
/// as a record is the simplest way to get this. Note that the comparison is shallow: changes made inside nested
/// objects or collections are not detected unless those types also compare by value.
/// </para>
/// <para>
/// Because the form binds to the update model, validation attributes belong on
/// <typeparamref name="TUpdateModel"/>. A mapping must be registered in both directions, as the store maps read
/// to update when loading and update to read when a model is assigned directly.
/// </para>
/// </remarks>
public abstract class EditPageBase<TKey, TReadModel, TUpdateModel> : ModelComponentBase<TReadModel>
    where TKey : notnull
    where TReadModel : class, IHaveIdentifier<TKey>, new()
    where TUpdateModel : class, new()
{
    private IDisposable? _locationChangingHandler;
    private bool _lastDirtyState;

    /// <summary>
    /// Gets or sets the identifier of the model this page edits.
    /// </summary>
    [Parameter, EditorRequired]
    public required TKey Id { get; set; }

    /// <summary>
    /// Gets or sets the state editor used to load, track and persist the model edited by this page.
    /// </summary>
    [Inject]
    protected ModelStateEditor<TKey, TReadModel, TUpdateModel> Store { get; set; } = default!;

    /// <summary>
    /// Gets or sets the service used to display modal dialogs.
    /// </summary>
    [Inject]
    protected ModalService Modal { get; set; } = default!;


    /// <summary>
    /// Gets a value indicating whether this page is creating a new model rather than editing an existing one.
    /// </summary>
    protected bool IsCreate => EqualityComparer<TKey>.Default.Equals(Id, default);

    /// <summary>
    /// Gets the dispatcher used to send custom requests.
    /// </summary>
    protected IDispatcher Dispatcher => Store.Dispatcher;

    /// <summary>
    /// Gets the data service used to load and save models.
    /// </summary>
    protected IDispatcherDataService DataService => Store.DataService;

    /// <summary>
    /// Gets the update model currently being edited, or <see langword="null"/> when it has not been loaded.
    /// </summary>
    /// <remarks>
    /// This is the editable copy the form binds to. It is a distinct instance from <c>Store.Original</c>, so
    /// changes made here are local until <see cref="HandleSave"/> succeeds and are discarded by
    /// <see cref="HandleCancel"/>. The instance is replaced whenever the model is loaded, created, saved or
    /// cancelled, so it should not be captured in a field.
    /// </remarks>
    protected TUpdateModel? Model => Store.Model;

    /// <summary>
    /// Gets the read model as it was last returned by the data store, or <see langword="null"/> when it has not been
    /// loaded.
    /// </summary>
    /// <remarks>
    /// This is the read only snapshot the editable <see cref="Model"/> was mapped from, including server owned
    /// values such as the identifier, row version and audit fields. It is not affected by edits made to
    /// <see cref="Model"/> and is replaced whenever the model is loaded, created, saved or cancelled, so it
    /// should not be captured in a field.
    /// </remarks>
    protected TReadModel? Original => Store.Original;


    /// <summary>
    /// Gets the edit context bound to the current <see cref="Model"/>.
    /// </summary>
    /// <remarks>
    /// A new context is created whenever the underlying model instance changes and is reused across parameter
    /// changes so validation state is not lost. Validation is driven by the attributes declared on
    /// <typeparamref name="TUpdateModel"/>.
    /// </remarks>
    protected EditContext? EditContext { get; set; }

    /// <inheritdoc />
    protected override bool IsDirty => Store.IsDirty;

    /// <inheritdoc />
    /// <remarks>
    /// The value is read from the original read model rather than from the edited copy. The default relies on
    /// <typeparamref name="TReadModel"/> overriding <see cref="object.ToString"/>; override this property to
    /// select a specific property instead.
    /// </remarks>
    protected override string? ModelDisplay => Store.Original?.ToString();

    /// <summary>
    /// Gets a value indicating whether a missing model should be created with the requested
    /// <see cref="Id"/> instead of navigating to
    /// <see cref="GetRedirectLocation(RedirectReason, TKey)"/> with
    /// <see cref="RedirectReason.NotFound"/>.
    /// </summary>
    protected virtual bool AllowUpsert => false;


    /// <summary>
    /// Gets the location to navigate to when leaving the current model for the specified reason.
    /// </summary>
    /// <param name="reason">The reason the page is navigating away from the current model</param>
    /// <param name="id">
    /// The identifier the location applies to, or the default value when the reason does not relate to a
    /// specific model
    /// </param>
    /// <returns>
    /// The location to navigate to, or <see langword="null"/> to use the behavior described by the calling page
    /// </returns>
    protected virtual string? GetRedirectLocation(RedirectReason reason, TKey? id) => null;

    /// <summary>
    /// Notifies the user that the requested model was not found and leaves the page.
    /// </summary>
    /// <remarks>
    /// Navigates to the location returned by <see cref="GetRedirectLocation(RedirectReason, TKey)"/> for
    /// <see cref="RedirectReason.NotFound"/>, or renders the not found page when no location is returned.
    /// </remarks>
    protected void RedirectNotFound(bool showNotification = true)
    {
        if (showNotification)
            Notification.ShowWarning($"{ModelLabel} was not found.");

        var url = GetRedirectLocation(RedirectReason.NotFound, Id);

        if (url != null)
            Navigation.NavigateTo(url);
        else
            Navigation.NotFound();
    }


    /// <summary>
    /// Gets the verb describing the current operation.
    /// </summary>
    /// <returns><c>Create</c> when creating a new model; otherwise <c>Edit</c></returns>
    protected string EditLabel() => IsCreate ? "Create" : "Edit";

    /// <summary>
    /// Gets the title used when editing the model.
    /// </summary>
    /// <returns>The edit title</returns>
    protected string EditTitle() => $"{ModelLabel} {EditLabel()}";


    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Store.OnStateChanged += HandleModelChange;
        _locationChangingHandler = Navigation.RegisterLocationChangingHandler(HandleLocationChange);
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        await LoadModel(force: false);

        if (Store.Model == null)
            return; // model was not loaded; either redirected or the load failed

        // only rebuild the edit context when the model instance changes,
        // otherwise validation state is reset on every parameter change
        if (ReferenceEquals(EditContext?.Model, Store.Model))
            return;

        // unsubscribe from the previous edit context to avoid memory leaks
        if (EditContext != null)
            EditContext.OnFieldChanged -= HandleFormChange;

        EditContext = new EditContext(Store.Model);
        EditContext.OnFieldChanged += HandleFormChange;
    }

    /// <summary>
    /// Reloads the model and any additional data, discarding cached state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual Task HandleRefresh() => LoadModel(force: true);

    /// <summary>
    /// Validates and saves the current model, navigating to the location returned by
    /// <see cref="GetRedirectLocation(RedirectReason, TKey)"/> when the
    /// identifier changed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual async Task HandleSave()
    {
        try
        {
            var cancellationToken = CancellationToken;

            // guard direct calls that bypass the EditForm submit validation;
            // a missing context means the model has not been loaded, so there is nothing valid to save
            if (EditContext?.Validate() != true)
                return;

            if (!await OnSavingAsync(cancellationToken))
                return;

            var originalId = Store.Original == null ? default : Store.Original.Id;

            await Store.Save(cancellationToken);

            // the saved model is the new baseline for change tracking
            ResetDirtyState();

            Notification.ShowSuccess($"{ModelLabel} '{ModelDisplay}' saved successfully");

            await OnSavedAsync(cancellationToken);

            var updatedId = Store.Original == null ? default : Store.Original.Id;

            // stay on the same page if the identifier hasn't changed
            if (!EqualityComparer<TKey>.Default.Equals(originalId, updatedId))
            {
                var url = GetRedirectLocation(RedirectReason.Created, updatedId) ?? "/";
                Navigation.NavigateTo(url);

                // the page is being torn down; no need to render the current state
                return;
            }
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving {ModelLabel} '{ModelId}': {ErrorMessage}", ModelLabel, Id, ex.Message);
            Notification.ShowError(ex);
        }

        await StateChangedAsync();
    }

    /// <summary>
    /// Discards any unsaved changes and navigates away from the page.
    /// </summary>
    /// <param name="redirect">
    /// An optional location to navigate to instead of the location returned by
    /// <see cref="GetRedirectLocation(RedirectReason, TKey)"/>
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual async Task HandleCancel(string? redirect = null)
    {
        try
        {
            var cancellationToken = CancellationToken;

            if (Store.IsDirty)
            {
                var confirmed = await Modal.Confirm("Are you sure you want to cancel? All unsaved changes will be lost.", "Confirm Cancel", ModalVariant.Warning);
                if (!confirmed)
                    return;
            }

            // always reset the store so edits don't linger in the shared state for the next page
            await Store.Cancel(cancellationToken);
            ResetDirtyState();

            var url = redirect ?? GetRedirectLocation(RedirectReason.Canceled, default);

            if (url != null)
                Navigation.NavigateTo(url);
            else
                await StateChangedAsync();
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error canceling {ModelLabel} '{ModelId}': {ErrorMessage}", ModelLabel, Id, ex.Message);
            Notification.ShowError(ex);

            await StateChangedAsync();
        }
    }

    /// <summary>
    /// Deletes the current model after confirmation and navigates to the location returned by
    /// <see cref="GetRedirectLocation(RedirectReason, TKey)"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual async Task HandleDelete()
    {
        try
        {
            if (IsCreate || Store.Model == null)
                return;

            var cancellationToken = CancellationToken;

            var name = $"{ModelLabel} '{ModelDisplay}'";
            if (!await Modal.ConfirmDelete(name))
                return;

            await Store.Delete(cancellationToken);

            Notification.ShowSuccess($"{name} deleted successfully");

            var url = GetRedirectLocation(RedirectReason.Deleted, default) ?? "/";
            Navigation.NavigateTo(url);
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting {ModelLabel} '{ModelId}': {ErrorMessage}", ModelLabel, Id, ex.Message);
            Notification.ShowError(ex);

            await StateChangedAsync();
        }
    }


    /// <summary>
    /// Handles field changes raised by the <see cref="EditContext"/> so the dirty state is re-evaluated.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="args">The event data</param>
    /// <remarks>
    /// The store is only notified when the dirty state actually changes, so binding to <c>oninput</c> does not
    /// force a render on every keystroke.
    /// </remarks>
    protected void HandleFormChange(object? sender, FieldChangedEventArgs args)
    {
        var isDirty = Store.IsDirty;
        if (isDirty == _lastDirtyState)
            return;

        _lastDirtyState = isDirty;
        Store.NotifyStateChanged();
    }

    /// <summary>
    /// Synchronizes the tracked dirty state with the store after an operation that changes the model.
    /// </summary>
    /// <remarks>
    /// <see cref="HandleFormChange"/> only notifies the store when the dirty state changes, so the tracked value
    /// must be updated whenever the store is loaded, saved or reset outside of a field change.
    /// </remarks>
    private void ResetDirtyState() => _lastDirtyState = Store.IsDirty;


    /// <summary>
    /// Called after the model has been loaded so the page can load any additional data it requires.
    /// The default implementation does nothing.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual Task OnLoadedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after a new model has been created so the page can apply default values.
    /// The default implementation does nothing.
    /// </summary>
    /// <param name="model">The newly created model to apply defaults to</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// <para>
    /// Changes made here are not treated as user edits; the model is still reported as clean by
    /// <c>Store.IsDirty</c> afterwards, so the page does not open with an unsaved changes indicator
    /// or prompt when navigating away.
    /// </para>
    /// <para>
    /// Use this for defaults that depend on context, such as the current user, the current date or a value
    /// taken from a route or query string parameter. Constant defaults belong on the model type as property
    /// initializers instead.
    /// </para>
    /// <para>
    /// This is called for both the create path and the upsert path described by <see cref="AllowUpsert"/>.
    /// </para>
    /// </remarks>
    protected virtual Task OnCreatedAsync(TUpdateModel model, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before the model is saved so the page can normalize the model or veto the save.
    /// The default implementation allows the save to continue.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns><see langword="true"/> to continue saving; otherwise <see langword="false"/> to abort the save</returns>
    /// <remarks>
    /// Override to apply changes that must be made to <see cref="Model"/> before it is sent, or to run
    /// checks that cannot be expressed as validation attributes. Returning <see langword="false"/> leaves the page
    /// unchanged and shows no notification, so an override that aborts should tell the user why.
    /// </remarks>
    protected virtual Task<bool> OnSavingAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Called after the model has been saved successfully. The default implementation does nothing.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// This is called whether or not the page is about to redirect. Override to
    /// refresh data derived from the model, for example by calling
    /// <see cref="OnLoadedAsync(CancellationToken)"/>, or to invalidate related caches.
    /// </remarks>
    protected virtual Task OnSavedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    /// <inheritdoc />
    protected override void DisposeManagedResources()
    {
        Store.OnStateChanged -= HandleModelChange;

        EditContext?.OnFieldChanged -= HandleFormChange;
        EditContext = null;

        _locationChangingHandler?.Dispose();
        _locationChangingHandler = null;

        base.DisposeManagedResources();
    }


    private async Task CreateModel(TKey? id, CancellationToken cancellationToken)
    {
        // an identifier is supplied for the upsert path so the new record is created with the requested key
        if (id is null || EqualityComparer<TKey>.Default.Equals(id, default))
            Store.New();
        else
            Store.New(id);

        if (Store.Model == null)
            return;

        await OnCreatedAsync(Store.Model, cancellationToken);

        // defaults applied by OnCreatedAsync are not user edits, so the model starts clean
        Store.Accept();
        ResetDirtyState();
    }

    private async Task LoadModel(bool force)
    {
        var cancellationToken = CancellationToken;

        try
        {
            if (IsCreate)
            {
                await CreateModel(id: default, cancellationToken);
            }
            else
            {
                await Store.Load(Id, force, cancellationToken);
                ResetDirtyState();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (Store.Model == null)
            {
                if (!AllowUpsert)
                {
                    RedirectNotFound();
                    return;
                }

                await CreateModel(Id, cancellationToken);

                if (Store.Original == null)
                    throw new InvalidOperationException($"The {ModelLabel} could not be created for upsert of identifier '{Id}'.");
            }

            await OnLoadedAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
            return;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading {ModelLabel} '{ModelId}': {ErrorMessage}", ModelLabel, Id, ex.Message);
            Notification.ShowError(ex);
        }

        await StateChangedAsync();
    }

    private async ValueTask HandleLocationChange(LocationChangingContext context)
    {
        if (Store.Model == null || !Store.IsDirty)
            return;

        // if the user tries to navigate away with unsaved changes, show a confirmation dialog
        var confirmed = await Modal.Confirm("Are you sure you want to leave this page? All unsaved changes will be lost.", "Unsaved Changes", ModalVariant.Warning);

        if (!confirmed)
        {
            context.PreventNavigation();
            return;
        }

        // reset the store so changes don't linger in memory
        await Store.Cancel(context.CancellationToken);
        ResetDirtyState();
    }
}
