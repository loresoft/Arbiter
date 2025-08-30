using MemoryPack;

using Microsoft.AspNetCore.Http;

namespace Arbiter.MemoryPack.Endpoints;

/// <summary>
/// Represents an HTTP result that serializes a value using MemoryPack and writes it to the response.
/// </summary>
/// <typeparam name="TValue">The type of the value to serialize.</typeparam>
public sealed class MemoryPackResult<TValue> : IResult, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>, IContentTypeHttpResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryPackResult{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to serialize and write to the response.</param>
    /// <param name="statusCode">The HTTP status code to set on the response. If null, the status code will not be modified.</param>
    /// <param name="contentType">The content type to set on the response. If null or empty, defaults to "application/x-memorypack".</param>
    public MemoryPackResult(TValue? value, int? statusCode = null, string? contentType = null)
    {
        Value = value;
        ContentType = contentType;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the value to be serialized and written to the response.
    /// </summary>
    /// <value>The value to serialize, or null if no value is provided.</value>
    public TValue? Value { get; }

    /// <summary>
    /// Gets the value to be serialized and written to the response as an object.
    /// </summary>
    /// <value>The value to serialize, or null if no value is provided.</value>
    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the content type to be set on the response.
    /// </summary>
    /// <value>The content type string, or null to use the default "application/x-memorypack".</value>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the HTTP status code to be set on the response.
    /// </summary>
    /// <value>The HTTP status code, or null if the status code should not be modified.</value>
    public int? StatusCode { get; }

    /// <summary>
    /// Executes the result asynchronously by serializing the value using MemoryPack and writing it to the HTTP response.
    /// </summary>
    /// <param name="httpContext">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous execution of the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
    /// <remarks>
    /// This method sets the content type, status code (if specified), and includes type information
    /// in the "X-Data-Type" header for proper deserialization. The value is serialized directly
    /// to the response body writer to avoid intermediate buffering.
    /// </remarks>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var response = httpContext.Response;

        response.ContentType = !string.IsNullOrEmpty(ContentType)
            ? ContentType
            : MemoryPackConstants.MemoryPackMediaType;

        if (StatusCode.HasValue)
            response.StatusCode = StatusCode.Value;

        if (Value is null)
            return;

        var valueType = Value.GetType();

        // include type information for deserialization
        response.Headers.Append(MemoryPackConstants.DataTypeHeader, valueType.AssemblyQualifiedName);

        // write to PipeWriter directly to avoid intermediate buffer
        MemoryPackSerializer.Serialize(valueType, response.BodyWriter, Value);

        await response.BodyWriter.FlushAsync(httpContext.RequestAborted).ConfigureAwait(false);
    }
}
