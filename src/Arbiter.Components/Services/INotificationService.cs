namespace Arbiter.Components.Services;

/// <summary>
/// Displays short lived notification messages to the user.
/// </summary>
/// <remarks>
/// Implementations are expected to be non blocking; the methods queue a message for display and return
/// immediately rather than waiting for the user to acknowledge it.
/// </remarks>
public interface INotificationService
{
    /// <summary>
    /// Displays an informational message.
    /// </summary>
    /// <param name="message">The message to display</param>
    void ShowInformation(string message);

    /// <summary>
    /// Displays a message indicating that an operation completed successfully.
    /// </summary>
    /// <param name="message">The message to display</param>
    void ShowSuccess(string message);

    /// <summary>
    /// Displays a warning for the specified exception.
    /// </summary>
    /// <param name="exception">The exception that caused the warning</param>
    /// <param name="message">
    /// An optional message to display instead of the message of <paramref name="exception"/>
    /// </param>
    /// <remarks>
    /// Implementations are expected to log <paramref name="exception"/> so the details remain available
    /// after the notification is dismissed.
    /// </remarks>
    void ShowWarning(Exception exception, string? message = null);

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    /// <param name="message">The message to display</param>
    void ShowWarning(string message);

    /// <summary>
    /// Displays an error for the specified exception.
    /// </summary>
    /// <param name="exception">The exception that caused the error</param>
    /// <param name="message">
    /// An optional message to display instead of the message of <paramref name="exception"/>
    /// </param>
    /// <remarks>
    /// Implementations are expected to log <paramref name="exception"/> so the details remain available
    /// after the notification is dismissed.
    /// </remarks>
    void ShowError(Exception exception, string? message = null);

    /// <summary>
    /// Displays an error message.
    /// </summary>
    /// <param name="message">The message to display</param>
    void ShowError(string message);

    /// <summary>
    /// Removes all notifications that are currently displayed.
    /// </summary>
    void Clear();
}
