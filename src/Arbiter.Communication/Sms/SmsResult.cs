namespace Arbiter.Communication.Sms;

/// <summary>
/// Represents the result of an SMS operation, including its success status, an optional message, and an optional exception.
/// </summary>
public readonly record struct SmsResult
{
    /// <summary>
    /// Gets a value indicating whether the SMS was sent successfully.
    /// </summary>
    public bool Successful { get; init; }

    /// <summary>
    /// Gets an optional message describing the result of the SMS operation.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the number of segments the SMS message was split into, if applicable.
    /// </summary>
    public int? Segments { get; init; }

    /// <summary>
    /// Gets the exception that occurred during the SMS operation, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful <see cref="SmsResult"/> with an optional message.
    /// </summary>
    /// <param name="message">An optional message describing the success.</param>
    /// <param name="segments">The number of segments the SMS message was split into, if applicable.</param>
    /// <returns>An <see cref="SmsResult"/> indicating success.</returns>
    public static SmsResult Success(string? message = null, int? segments = null)
        => new() { Successful = true, Message = message, Segments = segments };

    /// <summary>
    /// Creates a failed <see cref="SmsResult"/> with an optional message and exception.
    /// </summary>
    /// <param name="message">An optional message describing the failure.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    /// <returns>An <see cref="SmsResult"/> indicating failure.</returns>
    public static SmsResult Fail(string? message = null, Exception? exception = null)
        => new() { Successful = false, Message = message, Exception = exception };
}
