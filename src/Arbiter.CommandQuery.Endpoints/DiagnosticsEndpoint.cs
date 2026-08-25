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
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Defines diagnostics and operational API endpoints.
/// </summary>
/// <param name="logger">The logger used for diagnostics endpoint operations.</param>
/// <param name="options">The options used to control which routes are exposed and how they are authorized.</param>
/// <remarks>
/// Routes are exposed based on <see cref="DiagnosticsEndpointOptions"/>. The <c>config-debugger</c> and
/// <c>cache-clear</c> routes are disabled unless explicitly enabled.
/// </remarks>
public partial class DiagnosticsEndpoint(ILogger<DiagnosticsEndpoint> logger, IOptions<DiagnosticsEndpointOptions>? options = null) : IEndpointRoute
{
    private readonly ILogger<DiagnosticsEndpoint> _logger = logger;
    private readonly DiagnosticsEndpointOptions _options = options?.Value ?? new DiagnosticsEndpointOptions();

    /// <summary>
    /// Adds the enabled diagnostics and operational routes to the endpoint builder.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public void AddRoutes(IEndpointRouteBuilder builder)
    {
        if (_options.ConfigurationEnabled)
        {
            var route = builder
                .MapGet("config-debugger", ConfigurationDebugger)
                .WithTags("Diagnostics")
                .WithSummary("Show configuration")
                .WithDescription("Show configuration");

            ApplyConventions(route, _options.ConfigurationPolicy, _options.ConfigureConfigurationEndpoint);
        }

        if (_options.HealthCheckEnabled)
        {
            var route = builder
                .MapGet("health-check", HealthCheck)
                .WithTags("Diagnostics")
                .WithSummary("Health Check")
                .WithDescription("Health Check");

            ApplyConventions(route, _options.HealthCheckPolicy, _options.ConfigureHealthCheckEndpoint);
        }

        if (_options.ClaimsCheckEnabled)
        {
            var route = builder
                .MapGet("claims-check", ClaimsCheck)
                .WithTags("Diagnostics")
                .WithSummary("Claims Check")
                .WithDescription("Claims Check");

            ApplyConventions(route, _options.ClaimsCheckPolicy, _options.ConfigureClaimsCheckEndpoint);
        }

        if (_options.CacheClearEnabled)
        {
            var route = builder
                .MapGet("cache-clear", CacheClear)
                .WithTags("Diagnostics")
                .WithSummary("Clear Cache")
                .WithDescription("Clear Cache");

            ApplyConventions(route, _options.CacheClearPolicy, _options.ConfigureCacheClearEndpoint);
        }

        LogDiagnosticsRoutesMapped(
            _logger,
            _options.ConfigurationEnabled,
            _options.HealthCheckEnabled,
            _options.ClaimsCheckEnabled,
            _options.CacheClearEnabled);
    }

    /// <summary>
    /// Applies the shared diagnostics conventions, authorization, and customization actions to a route.
    /// </summary>
    /// <param name="route">The route to apply the conventions to.</param>
    /// <param name="policy">The endpoint specific authorization policy name, if any.</param>
    /// <param name="configure">The endpoint specific customization action, if any.</param>
    private void ApplyConventions(
        RouteHandlerBuilder route,
        string? policy,
        Action<RouteHandlerBuilder>? configure)
    {
        // Exclude diagnostics endpoints from OpenAPI/Swagger documentation by default
        route.ExcludeFromDescription();

        // Apply authorization policy if specified, otherwise use the default policy from options
        var authorizationPolicy = policy ?? _options.AuthorizationPolicy;
        if (!string.IsNullOrEmpty(authorizationPolicy))
            route.RequireAuthorization(authorizationPolicy);
        else
            route.RequireAuthorization();

        // Apply any additional endpoint configuration provided in the options or by the caller
        _options.ConfigureEndpoint?.Invoke(route);
        configure?.Invoke(route);
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
        LogConfigurationDebugRequested(_logger, user?.Identity?.Name ?? "anonymous");

        try
        {
            var command = new ConfigurationQuery(user);
            command.ApplyContext(httpContext);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            LogEndPointException(_logger, ex, ex.Message);

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
        LogHealthCheckRequested(_logger, user?.Identity?.Name ?? "anonymous");

        try
        {
            var command = new HealthCheckCommand(user);
            command.ApplyContext(httpContext);

            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            LogEndPointException(_logger, ex, ex.Message);

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
        LogClaimsCheckRequested(_logger, user?.Identity?.Name ?? "anonymous");

        try
        {
            var claims = user?.Claims
                .Select(c => new ClaimType { Type = c.Type, Value = c.Value })
                .ToList();

            return TypedResults.Ok(claims);
        }
        catch (Exception ex)
        {
            LogEndPointException(_logger, ex, ex.Message);

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
        LogCacheClearRequested(
            _logger,
            user?.Identity?.Name ?? "anonymous",
            tags != null ? string.Join(", ", tags) : "all");

        try
        {
            if (hybridCache == null)
            {
                LogHybridCacheUnavailable(_logger);
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
            LogEndPointException(_logger, ex, ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }


    [LoggerMessage(1, LogLevel.Information, "Diagnostics routes mapped; config-debugger: {ConfigurationEnabled}, health-check: {HealthCheckEnabled}, claims-check: {ClaimsCheckEnabled}, cache-clear: {CacheClearEnabled}")]
    static partial void LogDiagnosticsRoutesMapped(
        ILogger logger,
        bool configurationEnabled,
        bool healthCheckEnabled,
        bool claimsCheckEnabled,
        bool cacheClearEnabled);

    [LoggerMessage(2, LogLevel.Information, "Configuration debug requested by {UserName}")]
    static partial void LogConfigurationDebugRequested(ILogger logger, string userName);

    [LoggerMessage(3, LogLevel.Information, "Health check requested by {UserName}")]
    static partial void LogHealthCheckRequested(ILogger logger, string userName);

    [LoggerMessage(4, LogLevel.Information, "Claims check requested by {UserName}")]
    static partial void LogClaimsCheckRequested(ILogger logger, string userName);

    [LoggerMessage(5, LogLevel.Information, "Cache clear requested by {UserName} for tags: {Tags}")]
    static partial void LogCacheClearRequested(ILogger logger, string userName, string tags);

    [LoggerMessage(6, LogLevel.Warning, "HybridCache service is not available. Cache clear operation cannot be performed.")]
    static partial void LogHybridCacheUnavailable(ILogger logger);

    [LoggerMessage(7, LogLevel.Error, "Error processing request: {ErrorMessage}")]
    static partial void LogEndPointException(ILogger logger, Exception exception, string errorMessage);
}
