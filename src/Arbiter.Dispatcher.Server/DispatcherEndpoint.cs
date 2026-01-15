using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.Mediation;

using MessagePack;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

using ProblemDetails = Arbiter.CommandQuery.Models.ProblemDetails;

namespace Arbiter.Dispatcher.Server;

/// <summary>
/// Provides an HTTP endpoint for dispatching mediator commands and queries.
/// </summary>
public class DispatcherEndpoint
{
    private readonly ILogger<DispatcherEndpoint> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DispatcherEndpoint"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for diagnostic logging.</param>
    public DispatcherEndpoint(ILogger<DispatcherEndpoint> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds the dispatcher endpoint route to the application's endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to add the route to.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public IEndpointConventionBuilder AddRoute(IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost(DispatcherConstants.DispatcherEndpoint, HandleSend)
            .WithTags("Dispatcher")
            .WithName("Send")
            .WithSummary("Send Mediator command")
            .WithDescription("Send Mediator command")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .ExcludeFromDescription();
    }

    /// <summary>
    /// Handles incoming HTTP POST requests for dispatching mediator commands and queries.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="mediator">The mediator instance used to send the deserialized request.</param>
    /// <param name="options">The MessagePack serializer options for deserialization.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IResult"/> containing the mediator response or problem details.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "<Pending>")]
    private async Task<IResult> HandleSend(
        HttpContext context,
        [FromServices] IMediator mediator,
        [FromServices] MessagePackSerializerOptions options,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpRequest = context.Request;

            if (!httpRequest.Headers.TryGetValue(DispatcherConstants.RequestTypeHeader, out var requestTypeHeader))
            {
                _logger.LogWarning("Missing or empty {Header} header", DispatcherConstants.RequestTypeHeader);
                return BadRequestProblem($"Missing {DispatcherConstants.RequestTypeHeader} header");
            }

            var requestTypeName = requestTypeHeader.FirstOrDefault();
            if (string.IsNullOrEmpty(requestTypeName))
            {
                _logger.LogWarning("Missing or empty {Header} header", DispatcherConstants.RequestTypeHeader);
                return BadRequestProblem($"Empty {DispatcherConstants.RequestTypeHeader} header");
            }

            // Resolve the request type
            var requestType = Type.GetType(requestTypeName, throwOnError: false, ignoreCase: true);
            if (requestType is null)
            {
                _logger.LogWarning("Unable to resolve request type: {RequestType}", requestTypeName);
                return BadRequestProblem($"Unknown request type: {requestTypeName}");
            }

            var request = await MessagePackSerializer
                .DeserializeAsync(
                    type: requestType,
                    stream: httpRequest.Body,
                    options: options,
                    cancellationToken: cancellationToken
                )
                .ConfigureAwait(false);

            if (request == null)
            {
                _logger.LogWarning("Failed to deserialize request of type: {RequestType}", requestTypeName);
                return BadRequestProblem($"Failed to deserialize request of type: {requestTypeName}");
            }

            // Apply current user principal if supported
            if (request is IRequestPrincipal requestPrincipal)
                requestPrincipal.ApplyPrincipal(user);

            var response = await mediator
                .Send(request, cancellationToken)
                .ConfigureAwait(false);

            if (response == null)
            {
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return Results.Empty;
            }

            return new MessagePackResult<object>(response);
        }
        catch (Exception ex)
        {
            return ExceptionProblem(ex);
        }
    }

    /// <summary>
    /// Creates a bad request (400) problem details response.
    /// </summary>
    /// <param name="detail">The detailed error message describing the bad request.</param>
    /// <returns>A <see cref="MessagePackResult{T}"/> containing the problem details.</returns>
    private static MessagePackResult<ProblemDetails> BadRequestProblem(string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = detail,
        };

        return new MessagePackResult<ProblemDetails>(problemDetails, statusCode: problemDetails.Status);
    }

    /// <summary>
    /// Creates a problem details response from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert to problem details.</param>
    /// <returns>A <see cref="MessagePackResult{T}"/> containing the problem details derived from the exception.</returns>
    private static MessagePackResult<ProblemDetails> ExceptionProblem(Exception exception)
    {
        var problemDetails = exception.ToProblemDetails();
        return new MessagePackResult<ProblemDetails>(problemDetails, statusCode: problemDetails.Status);
    }
}
