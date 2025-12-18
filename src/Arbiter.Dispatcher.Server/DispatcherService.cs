using Arbiter.CommandQuery.Definitions;
using Arbiter.Mediation;

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
[BindServiceMethod(typeof(DispatcherService), nameof(BindService))]
public class DispatcherService
{
    /// <summary>
    /// Binds the service methods to the gRPC server.
    /// </summary>
    /// <param name="service">The dispatcher service instance to bind.</param>
    /// <returns>A <see cref="ServerServiceDefinition"/> containing the bound service methods.</returns>
    public static ServerServiceDefinition BindService(DispatcherService service)
    {
        return ServerServiceDefinition.CreateBuilder()
            .AddMethod(DispatcherMethod.Execute, service.Execute)
            .Build();
    }

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

    /// <summary>
    /// Executes a request by deserializing it, applying the current user principal if supported,
    /// dispatching it to the mediator, and returning the serialized response.
    /// </summary>
    /// <param name="requestBytes">The serialized request bytes.</param>
    /// <param name="context">The server call context containing request metadata and cancellation token.</param>
    /// <returns>The serialized response bytes.</returns>
    /// <exception cref="RpcException">
    /// Thrown when the x-message-type header is missing, the message type cannot be resolved,
    /// or the request payload cannot be deserialized.
    /// </exception>
    /// <remarks>
    /// The method expects the message type name to be provided in the 'x-message-type' header.
    /// If the request implements <see cref="IRequestPrincipal"/>, the current user principal from
    /// the HTTP context will be applied to the request before processing.
    /// </remarks>
    public async Task<byte[]> Execute(
        byte[] requestBytes,
        ServerCallContext context)
    {
        try
        {
            // Get type from metadata header
            var messageTypeName = context.RequestHeaders.GetValue(DispatcherMethod.TypeHeader);
            if (string.IsNullOrEmpty(messageTypeName))
            {
                _logger.LogWarning("Missing x-message-type header");
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    $"Required header '{DispatcherMethod.TypeHeader}' is missing. Please include the message type name in the request headers."));
            }

            // Resolve request message type
            var requestType = Type.GetType(messageTypeName);
            if (requestType == null)
            {
                _logger.LogWarning("Unknown message type: {MessageTypeName}", messageTypeName);
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    $"Unable to resolve message type '{messageTypeName}'. Ensure the type is available in the current context and the assembly-qualified name is correct."));
            }

            // Single deserialization directly to the actual type
            var request = MessagePackSerializer.Deserialize(requestType, requestBytes, _options, context.CancellationToken);
            if (request == null)
            {
                _logger.LogWarning("Failed to deserialize request of type: {MessageTypeName}", messageTypeName);
                throw new RpcException(new Status(StatusCode.InvalidArgument, 
                    $"Failed to deserialize request payload to type '{messageTypeName}'. The message format may be invalid or incompatible."));
            }

            // Apply current user principal if supported
            if (request is IRequestPrincipal requestPrincipal)
            {
                // get current user
                var httpContext = context.GetHttpContext();
                var user = httpContext?.User;

                requestPrincipal.ApplyPrincipal(user);
            }

            // Send to Mediator
            var response = await _mediator.Send(request, context.CancellationToken).ConfigureAwait(false);

            // Single serialization of response
            return MessagePackSerializer.Serialize(response, _options, context.CancellationToken);
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            _logger.LogError(ex, "Error processing request");
            throw;
        }
    }
}
