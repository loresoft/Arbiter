using System.Security.Claims;

using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Endpoints;
using Arbiter.Mediation;

using MemoryPack;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.MemoryPack.Endpoints;

/// <summary>
/// Defines an endpoint for dispatching commands using the Mediator pattern with MemoryPack serialization.
/// </summary>
/// <remarks>
/// This endpoint provides high-performance binary serialization for mediator requests using MemoryPack.
/// The request type is specified via the 'X-Data-Type' header, and the request body contains the
/// MemoryPack-serialized data.
/// </remarks>
public partial class MemoryPackEndpoint : IEndpointRoute
{
    private readonly DispatcherOptions _dispatcherOptions;
    private readonly ILogger<DispatcherEndpoint> _logger;
    private readonly string _sendRoute;


    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryPackEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger for this endpoint</param>
    /// <param name="dispatcherOptions">The configuration options for the dispatcher</param>
    /// <exception cref="ArgumentNullException">When <paramref name="logger"/> or <paramref name="dispatcherOptions"/> are <see langword="null"/></exception>
    public MemoryPackEndpoint(ILogger<DispatcherEndpoint> logger, IOptions<DispatcherOptions> dispatcherOptions)
    {
        _logger = logger;
        _dispatcherOptions = dispatcherOptions.Value;
        _sendRoute = _dispatcherOptions.SendRoute + MemoryPackConstants.RouteSuffix;
    }

    /// <summary>
    /// Adds routes to the specified <see cref="IEndpointRouteBuilder"/> instance.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <remarks>
    /// Configures a POST endpoint for sending mediator commands with MemoryPack serialization.
    /// The endpoint requires the 'X-Data-Type' header to specify the request type.
    /// </remarks>
    public void AddRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(_dispatcherOptions.DispatcherPrefix);

        group
            .MapPost(_sendRoute, Send)
            .WithEntityMetadata("Dispatcher")
            .WithName("Send")
            .WithSummary("Send Mediator command")
            .WithDescription("Send Mediator command")
            .ExcludeFromDescription();
    }

    /// <summary>
    /// Dispatches a MemoryPack-serialized request to the appropriate handler using the Mediator pattern.
    /// </summary>
    /// <param name="httpRequest">The incoming HTTP request containing the MemoryPack-serialized data</param>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>
    /// An <see cref="IResult"/> containing the mediator response serialized with MemoryPack,
    /// or a problem result if an error occurs.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method expects the request type to be specified in the 'X-Data-Type' header.
    /// The request body should contain the MemoryPack-serialized request data.
    /// </para>
    /// <para>
    /// The response is returned as a <see cref="MemoryPackResult{T}"/> which serializes
    /// the result using MemoryPack for optimal performance.
    /// </para>
    /// </remarks>
    protected virtual async Task<IResult> Send(
        HttpRequest httpRequest,
        [FromServices] IMediator mediator,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        string? typeName = null;

        try
        {
            // Extract and validate type name from header
            if (!httpRequest.Headers.TryGetValue(MemoryPackConstants.DataTypeHeader, out var typeHeader) ||
                typeHeader.Count == 0 ||
                string.IsNullOrWhiteSpace(typeName = typeHeader[0]))
            {
                return TypedResults.Problem("Missing or invalid X-Data-Type header", statusCode: 400);
            }

            // Resolve target type with caching
            var requestType = Type.GetType(typeName, throwOnError: false);
            if (requestType == null)
                return TypedResults.Problem($"Could not resolve type: {typeName}", statusCode: 400);

            var requestData = await MemoryPackSerializer
                .DeserializeAsync(requestType, httpRequest.Body, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (requestData == null)
                return TypedResults.Problem("Failed to deserialize request data", statusCode: 400);

            // Send to mediator
            var result = await mediator.Send(requestData, cancellationToken).ConfigureAwait(false);

            return new MemoryPackResult<object?>(result, statusCode: 200);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TypedResults.Problem("Request was canceled", statusCode: 499);
        }
        catch (Exception ex)
        {
            LogDispatchError(_logger, typeName, ex.Message, ex);
            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    [LoggerMessage(1, LogLevel.Error, "Error dispatching request '{RequestType}': {ErrorMessage}")]
    static partial void LogDispatchError(ILogger logger, string? requestType, string errorMessage, Exception exception);
}
