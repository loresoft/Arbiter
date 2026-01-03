using Arbiter.CommandQuery.Definitions;
using Arbiter.Mediation;

using Google.Protobuf;

using Grpc.Core;

using MessagePack;

using Microsoft.Extensions.Logging;

namespace Arbiter.Dispatcher.Server;

/// <summary>
/// gRPC service that dispatches requests to the mediator for processing.
/// </summary>
/// <remarks>
/// This service receives serialized requests via gRPC, deserializes them using MessagePack,
/// applies the current user principal if supported, and dispatches them to the mediator for processing.
/// The response is then serialized and returned to the caller.
/// </remarks>
public class DispatcherService : DispatcherRpc.DispatcherRpcBase
{
    public const string TypeHeader = "x-message-type";

    private readonly ILogger<DispatcherService> _logger;
    private readonly IMediator _mediator;
    private readonly MessagePackSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic logging.</param>
    /// <param name="mediator">The mediator instance for processing requests.</param>
    /// <param name="options">The MessagePack serialization options.</param>
    public DispatcherService(
        ILogger<DispatcherService> logger,
        IMediator mediator,
        MessagePackSerializerOptions options)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options;
    }


    public override async Task<DispatcherResponse> Execute(DispatcherRequest request, ServerCallContext context)
    {
        try
        {
            var requestBytes = request.Payload.ToByteArray();

            _logger.LogInformation("Execute method called with {ByteCount} bytes", requestBytes?.Length ?? 0);

            // Get type from metadata header
            var messageTypeName = context.RequestHeaders.GetValue(TypeHeader);
            _logger.LogInformation("Message type header value: {MessageTypeName}", messageTypeName ?? "NULL");

            if (string.IsNullOrEmpty(messageTypeName))
            {
                _logger.LogWarning("Missing x-message-type header");
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Required header '{TypeHeader}' is missing. Please include the message type name in the request headers."));
            }

            // Resolve request message type
            var requestType = Type.GetType(messageTypeName);
            if (requestType == null)
            {
                _logger.LogWarning("Unknown message type: {MessageTypeName}", messageTypeName);
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Unable to resolve message type '{messageTypeName}'. Ensure the type is available in the current context and the assembly-qualified name is correct."));
            }

            // Validate request payload
            if (requestBytes == null || requestBytes.Length == 0)
            {
                _logger.LogWarning("Empty request payload for message type: {MessageTypeName}", messageTypeName);
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"The request payload is empty for message type '{messageTypeName}'. A valid serialized request is required."));
            }

            // deserialization of request
            var requestMessage = MessagePackSerializer.Deserialize(requestType, requestBytes, _options, context.CancellationToken);
            if (requestMessage == null)
            {
                _logger.LogWarning("Failed to deserialize request of type: {MessageTypeName}", messageTypeName);
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Failed to deserialize request payload to type '{messageTypeName}'. The message format may be invalid or incompatible."));
            }

            // Apply current user principal if supported
            if (requestMessage is IRequestPrincipal requestPrincipal)
            {
                // get current user
                var httpContext = context.GetHttpContext();
                var user = httpContext?.User;

                requestPrincipal.ApplyPrincipal(user);
            }

            // Send to Mediator
            var responseMessage = await _mediator.Send(requestMessage, context.CancellationToken).ConfigureAwait(false);
            if (responseMessage == null)
                return new DispatcherResponse { Payload = ByteString.Empty };

            // Single serialization of response
            var responseBytes = MessagePackSerializer.Serialize(responseMessage, _options, context.CancellationToken);
            return new DispatcherResponse { Payload = ByteString.CopyFrom(responseBytes) };

        }
        catch (Exception ex) when (ex is not RpcException)
        {
            _logger.LogError(ex, "Error processing request");
            throw;
        }
    }
}
