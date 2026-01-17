using System.Reflection;

using Arbiter.CommandQuery;
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
/// <remarks>
/// <para>
/// This class provides a way to return MessagePack-serialized responses from ASP.NET Core minimal API endpoints.
/// MessagePack is a binary serialization format that is typically faster and more compact than JSON.
/// </para>
/// <para>
/// The class automatically handles null values by returning a 204 No Content response, and supports
/// explicit type specification for serialization scenarios where the runtime type differs from the desired
/// serialization type. This is particularly useful with collection initializers or when serializing
/// derived types as their base type or interface.
/// </para>
/// <para>
/// When used in minimal API endpoints, this class implements <see cref="IEndpointMetadataProvider"/>
/// to automatically populate OpenAPI metadata for proper API documentation.
/// </para>
/// </remarks>
public class MessagePackResult :
    IResult,
    IValueHttpResult,
    IStatusCodeHttpResult,
    IContentTypeHttpResult,
    IEndpointMetadataProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessagePackResult"/> class.
    /// </summary>
    /// <param name="value">The value to serialize. If <c>null</c>, a 204 No Content response is returned.</param>
    /// <param name="statusCode">The HTTP status code. If not specified, defaults to 200 OK for non-null values and 204 No Content for null values.</param>
    /// <param name="contentType">The content type. If not specified, defaults to the MessagePack content type.</param>
    /// <param name="valueType">The type to use for MessagePack serialization. If not specified, defaults to the runtime type of the value.</param>
    /// <remarks>
    /// The <paramref name="valueType"/> parameter is used to explicitly specify the type for serialization.
    /// This is particularly useful when the runtime type differs from the desired serialization type.
    /// A common scenario is with collection initializers, where the concrete type (e.g., <c>List&lt;T&gt;</c>)
    /// may need to be serialized as a different type (e.g., <c>IEnumerable&lt;T&gt;</c> or <c>T[]</c>).
    /// </remarks>
    public MessagePackResult(object? value, int? statusCode = null, string? contentType = null, Type? valueType = null)
    {
        Value = value;
        StatusCode = statusCode;
        ContentType = contentType;
        ValueType = valueType;
    }

    /// <summary>
    /// Gets the value to be serialized to the response.
    /// </summary>
    /// <value>
    /// The value to serialize, or <c>null</c> if no content should be returned.
    /// </value>
    public object? Value { get; }

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
    /// Gets the type to use for MessagePack serialization.
    /// </summary>
    /// <value>
    /// The type to use for serialization, or <c>null</c> to use the runtime type of <see cref="Value"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows explicit specification of the serialization type, which is useful when
    /// the value should be serialized as a base type or interface rather than its concrete type.
    /// If not specified, the actual runtime type of the value is used.
    /// </para>
    /// <para>
    /// The type is used both for MessagePack serialization and to generate a portable type name
    /// that is added to the response headers via <see cref="DispatcherConstants.ResponseTypeHeader"/>.
    /// This portable name helps clients properly deserialize the response by providing explicit type information.
    /// </para>
    /// </remarks>
    public Type? ValueType { get; }

    /// <summary>
    /// Executes the result operation, serializing the value to MessagePack format and writing it to the HTTP response.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType = ContentType
            ?? MessagePackDefaults.MessagePackContentType;

        if (Value is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        if (StatusCode is { } statusCode)
            httpContext.Response.StatusCode = statusCode;

        var valueType = ValueType ?? Value.GetType();
        var responseType = valueType.GetPortableName();

        var options = httpContext.RequestServices.GetService<MessagePackSerializerOptions>()
            ?? MessagePackDefaults.DefaultSerializerOptions;

        if (responseType is not null)
            httpContext.Response.Headers[DispatcherConstants.ResponseTypeHeader] = responseType;

        MessagePackSerializer.Serialize(valueType, httpContext.Response.BodyWriter, Value, options, httpContext.RequestAborted);

        await httpContext.Response.BodyWriter
            .FlushAsync(httpContext.RequestAborted)
            .ConfigureAwait(false);
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
            contentTypes: [MessagePackDefaults.MessagePackContentType]));

        // Document 204 No Content for null values
        builder.Metadata.Add(new ProducesResponseTypeMetadata(
            statusCode: StatusCodes.Status204NoContent,
            type: typeof(void),
            contentTypes: []));
    }
}

