using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Defines an endpoint for dispatching commands using the Mediator pattern.
/// </summary>
public partial class DispatcherEndpoint : IEndpointRoute
{
    private readonly DispatcherOptions _dispatcherOptions;
    private readonly ILogger<DispatcherEndpoint> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger for this feature endpoint</param>
    /// <param name="dispatcherOptions">The configuration options for the dispatcher</param>
    /// <exception cref="ArgumentNullException">When <paramref name="logger"/> or <paramref name="dispatcherOptions"/> are <see langword="null"/></exception>
    public DispatcherEndpoint(ILogger<DispatcherEndpoint> logger, IOptions<DispatcherOptions> dispatcherOptions)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dispatcherOptions);

        _dispatcherOptions = dispatcherOptions.Value;
        _logger = logger;
    }


    /// <inheritdoc/>
    public void AddRoutes(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(_dispatcherOptions.DispatcherPrefix);

        group
            .MapPost(_dispatcherOptions.SendRoute, Send)
            .WithEntityMetadata("Dispatcher")
            .WithName("Send")
            .WithSummary("Send Mediator command")
            .WithDescription("Send Mediator command")
            .ExcludeFromDescription();
    }

    /// <summary>
    /// Dispatches a request to the appropriate handler using the Mediator pattern.
    /// </summary>
    /// <param name="dispatchRequest">The incoming dispatcher request</param>
    /// <param name="mediator">The <see cref="IMediator"/> to send request to.</param>
    /// <param name="user">The current security claims principal</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>Awaitable task returning the mediator response</returns>
    protected virtual async Task<IResult> Send(
        [FromBody] DispatchRequest dispatchRequest,
        [FromServices] IMediator mediator,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        var request = dispatchRequest.Request;

        if (request is IRequestPrincipal requestPrincipal)
            requestPrincipal.ApplyPrincipal(user);

        try
        {
            var result = await mediator.Send(request, cancellationToken).ConfigureAwait(false);
            return TypedResults.Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return TypedResults.Problem("Request was canceled", statusCode: 499);
        }
        catch (Exception ex)
        {
            LogDispatchError(_logger, request?.GetType()?.FullName, ex.Message, ex);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }


    [LoggerMessage(1, LogLevel.Error, "Error dispatching request '{RequestType}': {ErrorMessage}")]
    static partial void LogDispatchError(ILogger logger, string? requestType, string errorMessage, Exception exception);
}
