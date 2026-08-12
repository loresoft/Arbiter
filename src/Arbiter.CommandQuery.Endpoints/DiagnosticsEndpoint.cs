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
    /// <param name="configuration">The application configuration.</param>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A JSON result containing configuration provider values.</returns>
    private async Task<Results<Ok<List<ConfigurationValue>>, ProblemHttpResult>> ConfigurationDebugger(
        [FromServices] IConfiguration configuration,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Configuration debug requested by {UserName}", user?.Identity?.Name ?? "anonymous");

        try
        {
            var items = new List<ConfigurationValue>();
            if (configuration is not IConfigurationRoot configurationRoot)
            {
                _logger.LogWarning("Configuration is not an IConfigurationRoot. Unable to retrieve provider information.");
                return TypedResults.Ok(items);
            }

            void RecurseChildren(IEnumerable<IConfigurationSection> children)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var child in children)
                {
                    var (value, provider) = GetValueAndProvider(configurationRoot, child.Path);
                    if (provider != null)
                    {
                        ConfigurationValue providerValue = new()
                        {
                            Key = child.Path,
                            Value = value,
                            Provider = provider.ToString(),
                        };
                        items.Add(providerValue);
                    }

                    RecurseChildren(child.GetChildren());
                }
            }

            RecurseChildren(configurationRoot.GetChildren());

            return TypedResults.Ok(items.OrderBy(i => i.Key, StringComparer.Ordinal).ToList());

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
    /// <param name="mediator">The mediator used to dispatch the health check command.</param>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A health report result or problem details when an error occurs.</returns>
    private async Task<Results<Ok<HealthReportModel>, ProblemHttpResult>> HealthCheck(
        [FromServices] IMediator mediator,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Health check requested by {UserName}", user?.Identity?.Name ?? "anonymous");

        try
        {
            var command = new HealthCheckCommand(user);
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
    /// Clears cached entries by invalidating all cache tags.
    /// </summary>
    /// <param name="hybridCache">The configured hybrid cache instance.</param>
    /// <param name="user">The current user principal.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the cache was cleared.</returns>
    private async Task<IResult> CacheClear(
        [FromServices] HybridCache? hybridCache = null,
        ClaimsPrincipal? user = default,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cache clear requested by {UserName}", user?.Identity?.Name ?? "anonymous");

        try
        {
            if (hybridCache == null)
            {
                _logger.LogWarning("HybridCache service is not available. Cache clear operation cannot be performed.");
                return TypedResults.NoContent();
            }

            await hybridCache.RemoveByTagAsync("*", cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok("Cache cleared by request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }


    /// <summary>
    /// Resolves a configuration value and the provider that supplies it.
    /// </summary>
    /// <param name="root">The configuration root.</param>
    /// <param name="key">The configuration key.</param>
    /// <returns>The resolved value and provider, if found.</returns>
    private static (string? Value, IConfigurationProvider? Provider) GetValueAndProvider(
        IConfigurationRoot root,
        string key)
    {
        // Iterate through the configuration providers in reverse order to find the last provider that has the key.
        foreach (var provider in root.Providers.Reverse())
        {
            if (provider.TryGet(key, out var value))
                return (value, provider);
        }

        return (null, null);
    }
}
