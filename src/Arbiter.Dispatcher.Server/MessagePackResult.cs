using System.Reflection;

using Arbiter.CommandQuery.Extensions;

using MessagePack;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Dispatcher.Server;

/// <summary>
/// An <see cref="IResult"/> that serializes a value to MessagePack format and writes it to the HTTP response.
/// </summary>
/// <typeparam name="TValue">The type of the value to serialize.</typeparam>
public class MessagePackResult<TValue> :
    IResult,
    IValueHttpResult,
    IValueHttpResult<TValue>,
    IStatusCodeHttpResult,
    IContentTypeHttpResult,
    IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackResult{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to serialize. If <c>null</c>, a 204 No Content response is returned.</param>
    /// <param name="statusCode">The HTTP status code. If not specified, defaults to 200 OK for non-null values and 204 No Content for null values.</param>
    /// <param name="contentType">The content type. If not specified, defaults to the MessagePack content type.</param>
    public MessagePackResult(TValue? value, int? statusCode = null, string? contentType = null)
    {
        Value = value;
        StatusCode = statusCode;
        ContentType = contentType;
    }

    /// <summary>
    /// Gets the value to be serialized to the response.
    /// </summary>
    /// <value>
    /// The value to serialize, or <c>null</c> if no content should be returned.
    /// </value>
    public TValue? Value { get; }

    /// <inheritdoc/>
    object? IValueHttpResult.Value => Value;

    /// <summary>
    /// Gets the content type for the response.
    /// </summary>
    /// <value>
    /// The content type, or <c>null</c> to use the default MessagePack content type.
    /// </value>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the HTTP status code for the response.
    /// </summary>
    /// <value>
    /// The status code, or <c>null</c> to use the default (200 OK for non-null values, 204 No Content for null values).
    /// </value>
    public int? StatusCode { get; }


    /// <summary>
    /// Executes the result operation, serializing the value to MessagePack format and writing it to the HTTP response.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType = ContentType
            ?? DispatcherConstants.MessagePackContentType;

        if (Value is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        }

        if (StatusCode is { } statusCode)
            httpContext.Response.StatusCode = statusCode;

        var valueType = Value.GetType();
        var responseType = valueType.GetPortableName();

        var options = httpContext.RequestServices.GetService<MessagePackSerializerOptions>()
            ?? DispatcherConstants.DefaultSerializerOptions;

        if (responseType is not null)
            httpContext.Response.Headers[DispatcherConstants.ResponseTypeHeader] = responseType;

        return MessagePackSerializer.SerializeAsync(valueType, httpContext.Response.Body, Value, options, httpContext.RequestAborted);
    }

    /// <summary>
    /// Populates metadata for the endpoint, documenting the possible response types and status codes.
    /// </summary>
    /// <param name="method">The method info for the endpoint handler.</param>
    /// <param name="builder">The endpoint builder to populate metadata for.</param>
    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(builder);

        // Document 200 OK with MessagePack content
        builder.Metadata.Add(new ProducesResponseTypeMetadata(
            statusCode: StatusCodes.Status200OK,
            type: typeof(TValue),
            contentTypes: [DispatcherConstants.MessagePackContentType]));

        // Document 204 No Content for null values
        builder.Metadata.Add(new ProducesResponseTypeMetadata(
            statusCode: StatusCodes.Status204NoContent,
            type: typeof(void),
            contentTypes: []));
    }
}

