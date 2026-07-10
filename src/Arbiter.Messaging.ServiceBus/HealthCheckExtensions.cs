using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Extension methods for configuring Service Bus messaging.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds an Azure Service Bus queue health check registration for a keyed Service Bus administration client and sender.
    /// </summary>
    /// <param name="health">The health check builder.</param>
    /// <param name="serviceName">The key used to resolve the Service Bus administration client.</param>
    /// <param name="queueName">The queue name and key used to resolve the Service Bus sender.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder" /> instance for chaining.</returns>
    public static IHealthChecksBuilder AddServiceBus(
        this IHealthChecksBuilder health,
        string serviceName,
        string queueName)
    {
        ArgumentNullException.ThrowIfNull(health);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        ArgumentException.ThrowIfNullOrEmpty(queueName);

        var healthCheckRegistration = new HealthCheckRegistration(
            name: $"Service Bus Queue: '{queueName}'",
            factory: sp =>
            {
                return new ServiceBusQueueHealthCheck(
                    sp.GetRequiredService<ILogger<ServiceBusQueueHealthCheck>>(),
                    sp.GetRequiredKeyedService<ServiceBusAdministrationClient>(serviceName),
                    sp.GetRequiredKeyedService<ServiceBusSender>(queueName)
                );
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ServiceBus"]
        );

        health.Add(healthCheckRegistration);
        return health;

    }

    /// <summary>
    /// Adds an Azure Service Bus subscription health check registration for a keyed Service Bus administration client and topic sender.
    /// </summary>
    /// <param name="health">The health check builder.</param>
    /// <param name="serviceName">The key used to resolve the Service Bus administration client.</param>
    /// <param name="topicName">The topic name and key used to resolve the Service Bus sender.</param>
    /// <param name="subscriptionName">The subscription name to monitor.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder" /> instance for chaining.</returns>
    public static IHealthChecksBuilder AddServiceBus(
        this IHealthChecksBuilder health,
        string serviceName,
        string topicName,
        string subscriptionName)
    {
        ArgumentNullException.ThrowIfNull(health);
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        ArgumentException.ThrowIfNullOrEmpty(topicName);
        ArgumentException.ThrowIfNullOrEmpty(subscriptionName);

        var healthCheckRegistration = new HealthCheckRegistration(
            name: $"Service Bus Subscription: '{subscriptionName}' on Topic: '{topicName}'",
            factory: sp =>
            {
                return new ServiceBusSubscriptionHealthCheck(
                    sp.GetRequiredService<ILogger<ServiceBusSubscriptionHealthCheck>>(),
                    sp.GetRequiredKeyedService<ServiceBusAdministrationClient>(serviceName),
                    sp.GetRequiredKeyedService<ServiceBusSender>(topicName),
                    subscriptionName
                );
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ServiceBus"]
        );

        health.Add(healthCheckRegistration);
        return health;
    }

}
