using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides extension methods for registering Azure Service Bus services.
/// </summary>
public static class ServiceBusExtensions
{
    /// <summary>
    /// Registers a named Azure Service Bus client, configured senders, and resource initializer.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the <see cref="ServiceBusClient" />.</param>
    /// <param name="nameOrConnectionString">A Service Bus connection string, connection string name, or configuration key.</param>
    /// <param name="configure">A delegate used to configure queues, topics, subscriptions, and options.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddServiceBus(
        this IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        Action<ServiceBusOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrConnectionString);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ServiceBusOptions
        {
            ServiceKey = serviceName,
            NameOrConnectionString = nameOrConnectionString,
        };

        configure.Invoke(options);

        // register for enumeration and as keyed for resolution
        services.AddSingleton(options);
        services.TryAddKeyedSingleton(serviceName, options);

        services.TryAddKeyedSingleton(
            serviceKey: serviceName,
            implementationFactory: (sp, _) =>
            {
                var connectionString = sp.ResolveConnectionString(options.NameOrConnectionString);
                return new ServiceBusClient(connectionString);
            }
        );

        // register senders for queues and topics
        foreach (var queue in options.Queues.Concat(options.Topics))
        {
            services.AddKeyedSingleton(queue, (sp, _) =>
            {
                // if NameSuffix is provided, append to queue/topic name for sender resolution
                var queueName = options.FormatName(queue);
                var serviceBusClient = sp.GetRequiredKeyedService<ServiceBusClient>(options.ServiceKey);
                return serviceBusClient.CreateSender(queueName);
            });
        }

        services.AddHostedService<ServiceBusInitializer>();

        return services;
    }

    /// <summary>
    /// Resolves a connection string from either a direct connection string or a configuration key name.
    /// </summary>
    /// <param name="services">The service provider used to resolve application configuration.</param>
    /// <param name="nameOrConnectionString">A direct connection string, connection string name, or configuration key.</param>
    /// <returns>The resolved connection string, or the original value when no configuration value is found.</returns>
    public static string ResolveConnectionString(
        this IServiceProvider services,
        string nameOrConnectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrConnectionString);

        var isConnectionString = nameOrConnectionString.IndexOfAny([';', '=', ':', '/']) > 0;
        if (isConnectionString)
            return nameOrConnectionString;

        var configuration = services.GetRequiredService<IConfiguration>();

        // first try connection strings section
        var connectionString = configuration.GetConnectionString(nameOrConnectionString);
        if (!string.IsNullOrEmpty(connectionString))
            return connectionString!;

        // next try root collection
        connectionString = configuration[nameOrConnectionString];
        if (!string.IsNullOrEmpty(connectionString))
            return connectionString!;

        return nameOrConnectionString;
    }

}
