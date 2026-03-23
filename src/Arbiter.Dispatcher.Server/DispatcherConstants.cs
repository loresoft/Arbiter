using System.Net.Http.Headers;

using Arbiter.CommandQuery;

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
    /// The media type header value for MessagePack content.
    /// </summary>
    public static readonly MediaTypeHeaderValue MessagePackMediaTypeHeader = new(MessagePackDefaults.MessagePackContentType);
}
