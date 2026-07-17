using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Models;
using Arbiter.Health.Handlers;
using Arbiter.Health.HealthChecks;
using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Health;

/// <summary>
/// Provides extension methods for registering health services and URL-based health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds Arbiter health services and health check request handling.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddHealthServices(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHealthChecks();
        services.TryAddSingleton<IRequestHandler<HealthCheckCommand, HealthReportModel>, HealthCheckHandler>();

        return services;
    }


    /// <summary>
    /// Adds a URL health check using a static URL value.
    /// </summary>
    /// <param name="health">The health checks builder to configure.</param>
    /// <param name="name">The registration name for the health check.</param>
    /// <param name="url">The URL to probe.</param>
    /// <param name="httpMethod">The HTTP method used for the URL probe. Defaults to <see cref="HttpMethod.Head"/>.</param>
    /// <param name="failureStatus">The health status to report when the check fails. Defaults to <see cref="HealthStatus.Unhealthy"/>.</param>
    /// <param name="tags">The tags associated with the health check. Defaults to a single <c>URL</c> tag.</param>
    /// <param name="timeout">An optional timeout for the health check execution.</param>
    /// <returns>The configured <see cref="IHealthChecksBuilder"/> instance.</returns>
    public static IHealthChecksBuilder AddUrlCheck(
        this IHealthChecksBuilder health,
        string name,
        string url,
        HttpMethod? httpMethod = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(health);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(url);

        // defaults
        httpMethod ??= HttpMethod.Head;
        failureStatus ??= HealthStatus.Unhealthy;
        tags ??= ["URL"];

        var healthCheckRegistration = new HealthCheckRegistration(
            name: name,
            factory: sp =>
            {
                return new UrlHealthCheck(
                    logger: sp.GetRequiredService<ILogger<UrlHealthCheck>>(),
                    httpClientFactory: sp.GetRequiredService<IHttpClientFactory>(),
                    url: url,
                    httpMethod: httpMethod
                );
            },
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout
        );

        health.Add(healthCheckRegistration);

        return health;
    }

    /// <summary>
    /// Adds a URL health check using a URL factory resolved from the service provider.
    /// </summary>
    /// <param name="health">The health checks builder to configure.</param>
    /// <param name="name">The registration name for the health check.</param>
    /// <param name="urlFactory">A factory that resolves the URL to probe.</param>
    /// <param name="httpMethod">The HTTP method used for the URL probe. Defaults to <see cref="HttpMethod.Head"/>.</param>
    /// <param name="failureStatus">The health status to report when the check fails. Defaults to <see cref="HealthStatus.Unhealthy"/>.</param>
    /// <param name="tags">The tags associated with the health check. Defaults to a single <c>URL</c> tag.</param>
    /// <param name="timeout">An optional timeout for the health check execution.</param>
    /// <returns>The configured <see cref="IHealthChecksBuilder"/> instance.</returns>
    public static IHealthChecksBuilder AddUrlCheck(
        this IHealthChecksBuilder health,
        string name,
        Func<IServiceProvider, string> urlFactory,
        HttpMethod? httpMethod = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(health);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(urlFactory);

        // defaults
        httpMethod ??= HttpMethod.Head;
        failureStatus ??= HealthStatus.Unhealthy;
        tags ??= ["URL"];

        var healthCheckRegistration = new HealthCheckRegistration(
            name: name,
            factory: sp =>
            {
                var url = urlFactory(sp);

                return new UrlHealthCheck(
                    logger: sp.GetRequiredService<ILogger<UrlHealthCheck>>(),
                    httpClientFactory: sp.GetRequiredService<IHttpClientFactory>(),
                    url: url,
                    httpMethod: httpMethod
                );
            },
            failureStatus: failureStatus,
            tags: tags,
            timeout: timeout
        );

        health.Add(healthCheckRegistration);

        return health;
    }

}
