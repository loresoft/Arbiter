using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Extension methods for configuring Web PubSub health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds an Azure Web PubSub health check registration for a keyed hub service client.
    /// </summary>
    /// <param name="health">The health check builder.</param>
    /// <param name="hubName">The hub name and key used to resolve the Web PubSub service client.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder" /> instance for chaining.</returns>
    public static IHealthChecksBuilder AddWebPubSub(
        this IHealthChecksBuilder health,
        string hubName)
    {
        ArgumentNullException.ThrowIfNull(health);
        ArgumentException.ThrowIfNullOrEmpty(hubName);

        var healthCheckRegistration = new HealthCheckRegistration(
            name: $"Web PubSub Hub: '{hubName}'",
            factory: sp =>
            {
                return new WebPubSubHealthCheck(
                    sp.GetRequiredService<ILogger<WebPubSubHealthCheck>>(),
                    sp.GetRequiredKeyedService<WebPubSubServiceClient>(hubName)
                );
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["WebPubSub"]
        );

        health.Add(healthCheckRegistration);
        return health;
    }
}
