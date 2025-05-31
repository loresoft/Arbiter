namespace Arbiter.Communication.Email;

/// <summary>
/// Represents the result of an email delivery operation, including success status, message, and any exception details.
/// </summary>
public readonly record struct EmailResult
{
    /// <summary>
    /// Gets a value indicating whether the email was sent successfully.
    /// </summary>
    public bool Successful { get; init; }

    /// <summary>
    /// Gets an optional message describing the result of the email operation.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the exception that occurred during the email operation, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful <see cref="EmailResult"/> with an optional message.
    /// </summary>
    /// <param name="message">An optional message describing the success.</param>
    /// <returns>An <see cref="EmailResult"/> indicating success.</returns>
    public static EmailResult Success(string? message = null)
        => new() { Successful = true, Message = message };

    /// <summary>
    /// Creates a failed <see cref="EmailResult"/> with an optional message and exception.
    /// </summary>
    /// <param name="message">An optional message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    /// <returns>An <see cref="EmailResult"/> indicating failure.</returns>
    public static EmailResult Fail(string? message = null, Exception? exception = null)
        => new() { Successful = false, Message = message, Exception = exception };
}
