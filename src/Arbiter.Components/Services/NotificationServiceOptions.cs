namespace Arbiter.Components.Services;

/// <summary>
/// Options controlling how <see cref="NotificationService"/> displays notifications.
/// </summary>
public class NotificationServiceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the message of an exception is shown to the user.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to append the exception message to the notification; otherwise <see langword="false"/> to show
    /// <see cref="ErrorMessage"/> or <see cref="WarningMessage"/> instead. The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// An exception message can disclose internal detail such as a connection string, a file path or a database
    /// name, so it is hidden from the user by default. The exception is always written to the log with its full
    /// detail regardless of this setting. Enable this in a development environment to see the failure without
    /// having to read the log.
    /// </remarks>
    public bool ShowExceptionDetails { get; set; }

    /// <summary>
    /// Gets or sets the message shown for an exception when <see cref="ShowExceptionDetails"/> is <see langword="false"/>
    /// and no message is supplied by the caller.
    /// </summary>
    public string ErrorMessage { get; set; } = "An unexpected error occurred. Please try again.";

    /// <summary>
    /// Gets or sets the message shown for an exception raised as a warning when
    /// <see cref="ShowExceptionDetails"/> is <see langword="false"/> and no message is supplied by the caller.
    /// </summary>
    public string WarningMessage { get; set; } = "The operation did not complete as expected.";

    /// <summary>
    /// Gets or sets the number of seconds a warning notification is displayed for.
    /// </summary>
    /// <value>The default is 15 seconds.</value>
    public int WarningTimeout { get; set; } = 15;

    /// <summary>
    /// Gets or sets the number of seconds an error notification is displayed for.
    /// </summary>
    /// <value>The default is 30 seconds.</value>
    public int ErrorTimeout { get; set; } = 30;
}
