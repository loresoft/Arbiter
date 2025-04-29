using System.Net;

namespace Arbiter.CommandQuery;

/// <summary>
/// Represents an exception that occurs within the domain layer of the application.
/// </summary>
/// <remarks>
/// This exception is typically used to represent errors that occur in the domain logic of the application.
/// It includes an HTTP status code and optional error details.
/// </remarks>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a default message and status code 500 (Internal Server Error).
    /// </summary>
    public DomainException()
        : this(HttpStatusCode.InternalServerError, "Unknown server error")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified message and status code 500 (Internal Server Error).
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(string? message)
        : this(HttpStatusCode.InternalServerError, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified message, inner exception, and status code 500 (Internal Server Error).
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainException(string? message, Exception? innerException)
        : this(HttpStatusCode.InternalServerError, message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified HTTP status code and message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code for the error.</param>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(HttpStatusCode statusCode, string? message)
        : base(message)
    {
        StatusCode = (int)statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified HTTP status code, message, and inner exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code for the error.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainException(HttpStatusCode statusCode, string? message, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = (int)statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified status code and message.
    /// </summary>
    /// <param name="statusCode">The status code for the error.</param>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(int statusCode, string? message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with a specified status code, message, and inner exception.
    /// </summary>
    /// <param name="statusCode">The status code for the error.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DomainException(int statusCode, string? message, Exception? innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the status code associated with the exception.
    /// </summary>
    /// <value>
    /// The HTTP or custom status code representing the error.
    /// </value>
    public int StatusCode { get; }

    /// <summary>
    /// Gets or sets the errors associated with the domain exception.
    /// </summary>
    /// <value>
    /// A dictionary where the key is the property name, and the value is an array of error messages.
    /// </value>
    public IDictionary<string, string[]>? Errors { get; set; }
}
