using Arbiter.CommandQuery.Definitions;
using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;
using Arbiter.Dispatcher.State;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides a base class for pages that display a single read model loaded by its identifier
/// using a <see cref="ModelStateLoader{TKey, TModel}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the identifier key used to uniquely identify the model</typeparam>
/// <typeparam name="TReadModel">The type of the read model displayed by the page</typeparam>
/// <remarks>
/// The model is loaded when <see cref="Id"/> is set or changed. When the model
/// cannot be found, the user is notified and the page either navigates to the location returned by
/// <see cref="GetRedirectLocation(RedirectReason, TKey)"/> or renders the not
/// found page. Derived pages load supporting data by overriding <see cref="OnLoadedAsync(CancellationToken)"/>.
/// </remarks>
public abstract class ViewPageBase<TKey, TReadModel> : ModelComponentBase<TReadModel>
    where TKey : notnull
    where TReadModel : class, IHaveIdentifier<TKey>, new()
{
    /// <summary>
    /// Gets or sets the identifier of the model this page displays.
    /// </summary>
    [Parameter, EditorRequired]
    public required TKey Id { get; set; }

    /// <summary>
    /// Gets or sets the state loader used to load and cache the model displayed by this page.
    /// </summary>
    [Inject]
    protected ModelStateLoader<TKey, TReadModel> Store { get; set; } = default!;


    /// <summary>
    /// Gets the dispatcher used to send custom requests.
    /// </summary>
    protected IDispatcher Dispatcher => Store.Dispatcher;

    /// <summary>
    /// Gets the data service used to load models from the data store.
    /// </summary>
    protected IDispatcherDataService DataService => Store.DataService;

    /// <summary>
    /// Gets the currently loaded model, or <see langword="null"/> when it has not been loaded.
    /// </summary>
    protected TReadModel? Model => Store.Model;

    /// <summary>
    /// Gets a value indicating whether the <see cref="Store"/> is currently loading data.
    /// </summary>
    protected bool IsBusy => Store.IsBusy;

    /// <inheritdoc />
    /// <remarks>
    /// The default relies on <typeparamref name="TReadModel"/> overriding <see cref="object.ToString"/>;
    /// override this property to select a specific property instead.
    /// </remarks>
    protected override string? ModelDisplay => Store.Model?.ToString();


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


    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Store.OnStateChanged += HandleModelChange;
    }

    /// <inheritdoc />
    protected override Task OnParametersSetAsync() => LoadModel(force: false);


    /// <summary>
    /// Reloads the model and any additional data, bypassing the state loader cache.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected virtual Task HandleRefresh() => LoadModel(force: true);


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


    /// <inheritdoc />
    protected override void DisposeManagedResources()
    {
        Store.OnStateChanged -= HandleModelChange;

        base.DisposeManagedResources();
    }


    private async Task LoadModel(bool force)
    {
        var cancellationToken = CancellationToken;

        try
        {
            // the state loader skips the request when the same identifier is already loaded
            await Store.Load(Id, force, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            if (Store.Model == null)
            {
                RedirectNotFound();
                return;
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
}
