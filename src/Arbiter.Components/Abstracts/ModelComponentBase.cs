using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using Arbiter.CommandQuery.Extensions;
using Arbiter.Components.Services;
using Arbiter.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides the shared behavior for pages that work with a model type.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model loaded from the data store</typeparam>
/// <remarks>
/// This class supplies the display labels, page title, cancellation token and disposal used by
/// <see cref="ViewPageBase{TKey, TReadModel}"/>, <see cref="EditPageBase{TKey, TReadModel, TUpdateModel}"/>
/// and <see cref="ListPageBase{TKey, TReadModel, TListModel}"/>. It does not own a state store; derived pages
/// declare the store they use and subscribe to its change notifications.
/// </remarks>
public abstract partial class ModelComponentBase<TReadModel> : PrincipalComponentBase, IDisposable
    where TReadModel : class
{
    [GeneratedRegex(@"^(?<name>\w+)(Read|List|Create|Update|Export)Model$", RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ModelTypeRegex();
    private static readonly string _modelTypeName = ModelTypeRegex().Replace(typeof(TReadModel).Name, "${name}");
    private static readonly string _modelTypeLabel = _modelTypeName.ToTitle();

    private CancellationTokenSource? _cancellationSource;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the service used to display notifications to the user.
    /// </summary>
    [Inject]
    protected INotificationService Notification { get; set; } = default!;

    /// <summary>
    /// Gets or sets the navigation manager used to redirect away from the current model.
    /// </summary>
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;


    /// <summary>
    /// Gets a value indicating whether this component has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    /// Gets a token that is canceled when this component is disposed.
    /// </summary>
    protected CancellationToken CancellationToken
    {
        get
        {
            if (_disposed)
                return new CancellationToken(canceled: true);

            var tokenSource = _cancellationSource ??= new CancellationTokenSource();
            return tokenSource.Token;
        }
    }


    /// <summary>
    /// Gets the human readable name of the model type, for example <c>Purchase Order</c> for a
    /// <c>PurchaseOrderReadModel</c>.
    /// </summary>
    /// <value>
    /// The type name of <typeparamref name="TReadModel"/> with the <c>Read</c>, <c>List</c>, <c>Create</c>,
    /// <c>Update</c> or <c>Export</c> model suffix removed and split into words, for example
    /// <c>PurchaseOrderReadModel</c> becomes <c>Purchase Order</c>. A type name without one of those suffixes is
    /// used as is.
    /// </value>
    /// <remarks>
    /// <para>
    /// This value names the kind of record the page works with rather than a specific record, so it stays the same
    /// for every instance of the page. It is used for the page title built by <see cref="PageTitle(string?)"/> and
    /// for the notification messages raised when a record is saved, deleted or not found.
    /// </para>
    /// <para>
    /// The value is computed once per closed generic type and is safe to read from any thread. Override when the
    /// type name does not read well to a user, for example to return <c>Invoice</c> for an
    /// <c>ArInvoiceHeaderReadModel</c>. Use <see cref="ModelName"/> instead when the value is not being shown to a
    /// user.
    /// </para>
    /// </remarks>
    protected virtual string ModelLabel => _modelTypeLabel;

    /// <summary>
    /// Gets the name of the model type without spaces, for example <c>PurchaseOrder</c> for a
    /// <c>PurchaseOrderReadModel</c>.
    /// </summary>
    /// <value>
    /// The type name of <typeparamref name="TReadModel"/> with the <c>Read</c>, <c>List</c>, <c>Create</c>,
    /// <c>Update</c> or <c>Export</c> model suffix removed.
    /// </value>
    /// <remarks>
    /// This is the identifier form of <see cref="ModelLabel"/> and is intended for values that must be stable and
    /// are never shown to a user, such as an authorization policy subject, a permission name, a telemetry property
    /// or a cache key. Overriding <see cref="ModelLabel"/> does not change this value, so a display change cannot
    /// silently break an authorization check.
    /// </remarks>
    protected virtual string ModelName => _modelTypeName;

    /// <summary>
    /// Gets the human readable name of the record currently loaded, for example <c>PO-10432</c>.
    /// </summary>
    /// <value>
    /// The display name of the current record, or <see langword="null"/> when no record is loaded or the page does not
    /// display a single record. The default implementation always returns <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Where <see cref="ModelLabel"/> names the kind of record, this names the record itself. It is appended to the
    /// page title by <see cref="PageTitle(string?)"/> and included in the notification messages raised when a
    /// record is saved or deleted, so it should be short and should identify the record to a user, such as a name,
    /// title or reference number rather than a surrogate key.
    /// </para>
    /// <para>
    /// This property is read after the model has been loaded and again after every save, so an override must
    /// tolerate a model that has not been loaded yet and should read from the loaded model rather than from a
    /// captured field.
    /// </para>
    /// </remarks>
    protected virtual string? ModelDisplay => null;

    /// <summary>
    /// Gets a value indicating whether the current model has unsaved changes.
    /// </summary>
    /// <value>The default implementation always returns <see langword="false"/></value>
    /// <remarks>
    /// When <see langword="true"/>, <see cref="PageTitle(string?)"/> appends an asterisk to indicate unsaved changes.
    /// </remarks>
    protected virtual bool IsDirty => false;


    /// <summary>
    /// Builds a page title from the <see cref="ModelLabel"/>, an optional suffix and the
    /// <see cref="ModelDisplay"/>, appending an asterisk when <see cref="IsDirty"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="suffix">An optional suffix appended after the model label</param>
    /// <returns>The formatted page title</returns>
    protected virtual string PageTitle(string? suffix = null)
    {
        var displayName = ModelDisplay;
        var isDirty = IsDirty;

        return StringBuilder.Pool.Use(builder =>
        {
            builder.Append(ModelLabel);
            if (suffix.HasValue())
            {
                builder.Append(' ');
                builder.Append(suffix);
            }

            if (displayName.HasValue())
            {
                builder.Append(" - ");
                builder.Append(displayName);
            }

            if (isDirty)
                builder.Append(" *");

            return builder.ToString();
        });
    }


    /// <summary>
    /// Handles state changes raised by the state store by re-rendering the component.
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The event data</param>
    /// <remarks>
    /// A failure while rendering is logged but not shown to the user, because a state change is not an action
    /// the user initiated.
    /// </remarks>
    protected virtual void HandleModelChange(object? sender, EventArgs e)
        => Observe(notifyError: false);

    /// <summary>
    /// Re-renders the component unless it has already been disposed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    protected Task StateChangedAsync()
        => _disposed ? Task.CompletedTask : InvokeAsync(StateHasChanged);

    /// <summary>
    /// Runs an asynchronous operation from a synchronous event handler, re-rendering the component when it
    /// completes and logging any failure.
    /// </summary>
    /// <param name="operation">The operation to run using the component <see cref="CancellationToken"/>, or <see langword="null"/> to only re-render</param>
    /// <param name="notifyError">When <see langword="true"/>, a failure is also reported to the user through <see cref="Notification"/></param>
    /// <param name="operationName">The name of the operation used when logging a failure; defaults to the name of the calling member</param>
    /// <remarks>
    /// Synchronous events raised by components cannot be awaited, so the operation is observed in the background
    /// rather than returned to the caller. Nothing is run once the component has been disposed, and cancellation
    /// caused by disposal is ignored.
    /// <para>
    /// Use this method instead of an <c>async void</c> handler so a failed operation is reported rather than
    /// being thrown on the renderer.
    /// </para>
    /// </remarks>
    protected void Observe(
        Func<CancellationToken, Task>? operation = null,
        bool notifyError = true,
        [CallerMemberName] string? operationName = null)
    {
        if (_disposed)
            return;

        _ = ObserveOperationAsync(operation, notifyError, operationName);
    }


    /// <summary>
    /// Releases the resources used by this component.
    /// </summary>
    /// <remarks>
    /// Derived classes that hold disposable resources or event subscriptions should override this method,
    /// release their own resources and then call <c>base.DisposeManagedResources()</c>.
    /// </remarks>
    protected virtual void DisposeManagedResources()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        // mark disposed first so a new cancellation source is not created while releasing resources
        _disposed = true;

        DisposeManagedResources();

        if (_cancellationSource != null)
        {
            _cancellationSource.Cancel();
            _cancellationSource.Dispose();
            _cancellationSource = null;
        }

        GC.SuppressFinalize(this);
    }


    private async Task ObserveOperationAsync(
        Func<CancellationToken, Task>? operation,
        bool notifyError,
        string? operationName)
    {
        try
        {
            if (operation != null)
                await operation(CancellationToken);

            await StateChangedAsync();
        }
        catch (OperationCanceledException)
        {
            // component was disposed or navigated away; nothing to report
        }
        catch (ObjectDisposedException)
        {
            // component was disposed while the operation was running
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error running {OperationName} for {ModelLabel}: {ErrorMessage}", operationName, ModelLabel, ex.Message);

            if (notifyError)
                Notification.ShowError(ex);
        }
    }
}
