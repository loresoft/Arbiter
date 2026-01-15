using System.Net.Http.Headers;

using MessagePack;
using MessagePack.Resolvers;

#if DISPATCHER_CLIENT
namespace Arbiter.Dispatcher.Client;
#else
namespace Arbiter.Dispatcher.Server;
#endif

/// <summary>
/// Defines constants used by the dispatcher message system.
/// </summary>
public static class DispatcherConstants
{
    /// <summary>
    /// The HTTP header name for the message request type.
    /// </summary>
    public const string RequestTypeHeader = "X-Message-Request-Type";

    /// <summary>
    /// The HTTP header name for the message response type.
    /// </summary>
    public const string ResponseTypeHeader = "X-Message-Response-Type";

    /// <summary>
    /// The endpoint path for sending dispatcher messages.
    /// </summary>
    public const string DispatcherEndpoint = "/api/dispatcher/send";

    /// <summary>
    /// The content type for MessagePack serialized data.
    /// </summary>
    public const string MessagePackContentType = "application/x-msgpack";

    /// <summary>
    /// The media type header value for MessagePack content.
    /// </summary>
    public static readonly MediaTypeHeaderValue MessagePackMediaTypeHeader = new(MessagePackContentType);

    /// <summary>
    /// The default serializer options for MessagePack.
    /// </summary>
    public static readonly MessagePackSerializerOptions DefaultSerializerOptions =
        MessagePackSerializerOptions.Standard
            .WithResolver(
                CompositeResolver.Create(
                [
                    // 0) Uses the Roslyn source generator output produced automatically.
                    SourceGeneratedFormatterResolver.Instance,

                    // 1) Attribute-based + built-ins (enums, primitives, etc.)
                    StandardResolver.Instance,

                    // 2) Typeless (for object-typed or unknown static types)
                    TypelessObjectResolver.Instance,

                    // 3) Contractless fallback for POCOs without attributes
                    ContractlessStandardResolver.Instance,
                ])
            )
            .WithCompression(MessagePackCompression.Lz4BlockArray);
}
