using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Defines diagnostics and operational API endpoints.
/// </summary>
/// <param name="logger">The logger used for diagnostics endpoint operations.</param>
public class DiagnosticsEndpoint(ILogger<DiagnosticsEndpoint> logger) : IEndpointRoute
{
    private readonly ILogger<DiagnosticsEndpoint> _logger = logger;

    /// <summary>
    /// Adds diagnostics and operational routes to the endpoint builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public void AddRoutes(IEndpointRouteBuilder builder)
    {
        builder
            .MapGet("config-debugger", ConfigurationDebugger)
            .WithTags("Diagnostics")
            .WithSummary("Show configuration")
            .WithDescription("Show configuration")
            .ExcludeFromDescription()
            .RequireAuthorization();

        builder
            .MapGet("health-check", HealthCheck)
            .WithTags("Diagnostics")
            .WithSummary("Health Check")
            .WithDescription("Health Check")
            .ExcludeFromDescription()
            .RequireAuthorization();

        builder
            .MapGet("claims-check", ClaimsCheck)
            .WithTags("Diagnostics")
            .WithSummary("Claims Check")
            .WithDescription("Claims Check")
            .ExcludeFromDescription()
            .RequireAuthorization();

        builder
            .MapGet("cache-clear", CacheClear)
            .WithTags("Diagnostics")
            .WithSummary("Clear Cache")
            .WithDescription("Clear Cache")
            .ExcludeFromDescription()
            .RequireAuthorization();
    }


    /// <summary>
    /// Returns configuration entries as key, value, and provider records.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the request metadata.</param>
    /// <param name="mediator">The mediator used to dispatch the configuration query.</param>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A configuration result or problem details when an error occurs.</returns>
    private async Task<Results<Ok<IReadOnlyList<ConfigurationValue>>, ProblemHttpResult>> ConfigurationDebugger(
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Configuration debug requested by {UserName}", user?.Identity?.Name ?? "anonymous");

        try
        {
            var command = new ConfigurationQuery(user);
            command.ApplyContext(httpContext);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Executes the health check command and returns the current health report.
    /// </summary>
    /// <param name="httpContext">The HTTP context containing the connection and header information.</param>
    /// <param name="mediator">The mediator used to dispatch the health check command.</param>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A health report result or problem details when an error occurs.</returns>
    private async Task<Results<Ok<HealthReportModel>, ProblemHttpResult>> HealthCheck(
        HttpContext httpContext,
        [FromServices] IMediator mediator,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Health check requested by {UserName}", user?.Identity?.Name ?? "anonymous");

        try
        {
            var command = new HealthCheckCommand(user);
            command.ApplyContext(httpContext);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Returns the current user's claims.
    /// </summary>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of claim type and value pairs.</returns>
    private async Task<Results<Ok<List<ClaimType>>, ProblemHttpResult>> ClaimsCheck(
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Claims check requested by {UserName}", user?.Identity?.Name ?? "anonymous");

        try
        {
            var claims = user?.Claims
                .Select(c => new ClaimType { Type = c.Type, Value = c.Value })
                .ToList();

            return TypedResults.Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }

    /// <summary>
    /// Clears cached entries by invalidating the specified cache tags, or all tags when none are specified.
    /// </summary>
    /// <param name="hybridCache">The configured hybrid cache instance.</param>
    /// <param name="tags">The cache tags to invalidate. When empty, all cache entries are invalidated.</param>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the cache was cleared.</returns>
    private async Task<IResult> CacheClear(
        [FromServices] HybridCache? hybridCache = null,
        [FromQuery(Name = "tag")] string[]? tags = null,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cache clear requested by {UserName} for tags: {Tags}",
            user?.Identity?.Name ?? "anonymous",
            tags != null ? string.Join(", ", tags) : "all");

        try
        {
            if (hybridCache == null)
            {
                _logger.LogWarning("HybridCache service is not available. Cache clear operation cannot be performed.");
                return TypedResults.NoContent();
            }

            // no tags specified, clear everything using the wildcard tag
            var cacheTags = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            if (cacheTags is null || cacheTags.Length == 0)
                cacheTags = ["*"];

            await hybridCache.RemoveByTagAsync(cacheTags, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok("Cache cleared by request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }
}
