using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Services;

/// <summary>
/// An <see cref="INotificationService"/> that displays notifications using an <see cref="IToaster"/> and
/// writes warnings and errors to the log.
/// </summary>
/// <remarks>
/// The full detail of an exception is always logged. Whether it is also shown to the user is controlled by
/// <see cref="NotificationServiceOptions.ShowExceptionDetails"/>, which is disabled by default so internal
/// detail is not disclosed in the user interface.
/// </remarks>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IToaster _toaster;
    private readonly NotificationServiceOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="logger">The logger warnings and errors are written to</param>
    /// <param name="toaster">The toaster used to display the notifications</param>
    /// <param name="options">
    /// The options controlling how notifications are displayed, or <see langword="null"/> to use the defaults
    /// </param>
    /// <exception cref="ArgumentNullException">When <paramref name="logger"/> or <paramref name="toaster"/> is <see langword="null"/></exception>
    public NotificationService(
        ILogger<NotificationService> logger,
        IToaster toaster,
        IOptions<NotificationServiceOptions>? options = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(toaster);

        _logger = logger;
        _toaster = toaster;
        _options = options?.Value ?? new NotificationServiceOptions();
    }

    /// <inheritdoc />
    public void ShowInformation(string message)
    {
        _toaster.ShowInformation(message);
    }

    /// <inheritdoc />
    public void ShowSuccess(string message)
    {
        _toaster.ShowSuccess(message);
    }

    /// <inheritdoc />
    public void ShowWarning(string message)
    {
        _logger.LogWarning("Warning: {Message}", message);
        _toaster.ShowWarning(message, config => config.Timeout = _options.WarningTimeout);
    }

    /// <inheritdoc />
    public void ShowWarning(Exception exception, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogWarning(exception, "Warning: {Message}", message ?? exception.Message);

        var formatted = FormatMessage(exception, message, _options.WarningMessage);
        _toaster.ShowWarning(formatted, config => config.Timeout = _options.WarningTimeout);
    }

    /// <inheritdoc />
    public void ShowError(string message)
    {
        _logger.LogError("Error: {Message}", message);
        _toaster.ShowError(message, config => config.Timeout = _options.ErrorTimeout);
    }

    /// <inheritdoc />
    public void ShowError(Exception exception, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogError(exception, "Error: {Message}", message ?? exception.Message);

        var formatted = FormatMessage(exception, message, _options.ErrorMessage);
        _toaster.ShowError(formatted, config => config.Timeout = _options.ErrorTimeout);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _toaster.Clear();
    }

    /// <summary>
    /// Builds the text displayed to the user for the specified exception.
    /// </summary>
    /// <param name="exception">The exception being reported</param>
    /// <param name="message">The message supplied by the caller, or <see langword="null"/> when there is none</param>
    /// <param name="fallbackMessage">The message to display when there is nothing else to show</param>
    /// <returns>The text to display</returns>
    private string FormatMessage(Exception exception, string? message, string fallbackMessage)
    {
        var showDetails = _options.ShowExceptionDetails;

        if (string.IsNullOrWhiteSpace(message))
            return showDetails ? exception.Message : fallbackMessage;

        return showDetails ? $"{message}: {exception.Message}" : message;
    }
}
