using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;

using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.Mediation;

using MessagePack;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ProblemDetails = Arbiter.CommandQuery.Models.ProblemDetails;

namespace Arbiter.Dispatcher.Server;

/// <summary>
/// Provides an HTTP endpoint for dispatching mediator commands and queries.
/// </summary>
public partial class DispatcherEndpoint
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
    /// The endpoint supports both JSON (application/json) and MessagePack (application/x-msgpack) content types.
    /// Defaults to JSON when no Content-Type header is specified.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to add the route to.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public IEndpointConventionBuilder AddRoute(IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapPost(DispatcherConstants.DispatcherEndpoint, Send)
            .WithTags("Dispatcher")
            .WithName("Send")
            .WithSummary("Send Mediator command")
            .WithDescription("Send Mediator command with JSON or MessagePack serialization")
            .Accepts<object>("application/json", "application/x-msgpack")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError)
            .ExcludeFromDescription();
    }

    /// <summary>
    /// Handles incoming HTTP POST requests for dispatching mediator commands and queries.
    /// Supports both JSON and MessagePack serialization based on the Content-Type header.
    /// Defaults to JSON when no Content-Type header is specified.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="mediator">The mediator instance used to send the deserialized request.</param>
    /// <param name="user">The claims principal representing the current user. Applied to requests that implement <see cref="IRequestPrincipal"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IResult"/> containing the mediator response or problem details.</returns>
    private async Task<IResult> Send(
        HttpContext context,
        [FromServices] IMediator mediator,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        var httpRequest = context.Request;

        // Determine the content type early (default to JSON)
        var contentType = httpRequest.ContentType ?? MediaTypeNames.Application.Json;
        var isJson = !contentType.StartsWith(MessagePackDefaults.MessagePackContentType, StringComparison.OrdinalIgnoreCase);

        try
        {
            // Resolve the request type
            var requestType = ReadTypeHeader(httpRequest, out var message);
            if (requestType is null)
                return BadRequest(message, isJson);

            var request = (isJson)
                ? await ReadJsonBody(context, requestType, cancellationToken).ConfigureAwait(false)
                : await ReadMessagePack(context, requestType, cancellationToken).ConfigureAwait(false);

            if (request == null)
            {
                LogFailedToDeserializeRequest(_logger, requestType.Name);
                return BadRequest($"Failed to deserialize request of type: {requestType.Name}", isJson);
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

            // Determine response type if available, used for serialization
            Type? responseType = null;
            if (response is IResponseType responseInstance)
                responseType = responseInstance.ResponseType;

            return isJson
                ? TypedResults.Json<object>(response)
                : new MessagePackResult(response, valueType: responseType);
        }
        catch (Exception ex)
        {
            LogError(_logger, ex, nameof(Send), ex.Message);
            return ExceptionResult(ex, isJson);
        }
    }


    /// <summary>
    /// Reads and validates the request type header from the HTTP request.
    /// </summary>
    /// <param name="httpRequest">The HTTP request to read the header from.</param>
    /// <param name="message">An error message if the header is missing, empty, or references an unknown type; otherwise, <c>null</c>.</param>
    /// <returns>The resolved <see cref="Type"/> if successful; otherwise, <c>null</c>.</returns>
    private Type? ReadTypeHeader(HttpRequest httpRequest, out string? message)
    {
        message = null;

        // read the request type header
        if (!httpRequest.Headers.TryGetValue(DispatcherConstants.RequestTypeHeader, out var requestTypeHeader))
        {
            LogMissingHeader(_logger, DispatcherConstants.RequestTypeHeader);
            message = $"Missing {DispatcherConstants.RequestTypeHeader} header";
            return null;
        }

        // use first value from header
        var requestTypeName = requestTypeHeader.FirstOrDefault();
        if (string.IsNullOrEmpty(requestTypeName))
        {
            LogEmptyHeader(_logger, DispatcherConstants.RequestTypeHeader);
            message = $"Empty {DispatcherConstants.RequestTypeHeader} header";
            return null;
        }

        // Resolve the request type
        var requestType = Type.GetType(requestTypeName, throwOnError: false, ignoreCase: true);
        if (requestType is null)
        {
            LogUnableToResolveRequestType(_logger, requestTypeName);
            message = $"Unable to resolve request type: {requestTypeName}";
            return null;
        }

        return requestType;
    }


    /// <summary>
    /// Deserializes the HTTP request body as JSON.
    /// </summary>
    /// <param name="context">The HTTP context containing the request.</param>
    /// <param name="requestType">The type to deserialize the JSON into.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the deserialized object, or <c>null</c> if deserialization fails.</returns>
    private static ValueTask<object?> ReadJsonBody(HttpContext context, Type requestType, CancellationToken cancellationToken)
    {
        var jsonOptions = context.RequestServices.GetService<JsonSerializerOptions>();

        return JsonSerializer.DeserializeAsync(context.Request.Body, requestType, jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deserializes the HTTP request body as MessagePack.
    /// </summary>
    /// <param name="context">The HTTP context containing the request.</param>
    /// <param name="requestType">The type to deserialize the MessagePack data into.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the deserialized object, or <c>null</c> if deserialization fails.</returns>
    private static ValueTask<object?> ReadMessagePack(HttpContext context, Type requestType, CancellationToken cancellationToken)
    {
        var messagePackOptions = context.RequestServices.GetService<MessagePackSerializerOptions>()
            ?? MessagePackDefaults.DefaultSerializerOptions;

        return MessagePackSerializer.DeserializeAsync(requestType, context.Request.Body, messagePackOptions, cancellationToken);
    }


    /// <summary>
    /// Creates a bad request (400) problem details response.
    /// </summary>
    /// <param name="detail">The detailed error message describing the bad request.</param>
    /// <param name="isJson">If <c>true</c>, returns JSON format; otherwise, returns MessagePack format.</param>
    /// <returns>An <see cref="IResult"/> containing the problem details with status code 400.</returns>
    private static IResult BadRequest(string? detail, bool isJson = false)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = detail,
        };

        return isJson
            ? TypedResults.Json<ProblemDetails>(problemDetails, contentType: MediaTypeNames.Application.ProblemJson, statusCode: problemDetails.Status)
            : new MessagePackResult(problemDetails, statusCode: problemDetails.Status);
    }

    /// <summary>
    /// Creates a problem details response from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert to problem details.</param>
    /// <param name="isJson">If <c>true</c>, returns JSON format; otherwise, returns MessagePack format.</param>
    /// <returns>An <see cref="IResult"/> containing the problem details derived from the exception with the appropriate status code.</returns>
    private static IResult ExceptionResult(Exception exception, bool isJson = false)
    {
        var problemDetails = exception.ToProblemDetails();

        return isJson
            ? TypedResults.Json<ProblemDetails>(problemDetails, contentType: MediaTypeNames.Application.ProblemJson, statusCode: problemDetails.Status)
            : new MessagePackResult(problemDetails, statusCode: problemDetails.Status);
    }

    [LoggerMessage(LogLevel.Warning, "Failed to deserialize request of type: {requestType}")]
    private static partial void LogFailedToDeserializeRequest(ILogger logger, string requestType);

    [LoggerMessage(LogLevel.Warning, "Missing {header} header")]
    private static partial void LogMissingHeader(ILogger logger, string header);

    [LoggerMessage(LogLevel.Warning, "Empty {header} header")]
    private static partial void LogEmptyHeader(ILogger logger, string header);

    [LoggerMessage(LogLevel.Warning, "Unable to resolve request type: {requestType}")]
    private static partial void LogUnableToResolveRequestType(ILogger logger, string requestType);

    [LoggerMessage(LogLevel.Error, "Error {methodName}: {errorMessage}")]
    private static partial void LogError(ILogger logger, Exception exception, string methodName, string errorMessage);
}
