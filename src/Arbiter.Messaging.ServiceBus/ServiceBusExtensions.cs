using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides extension methods for registering Azure Service Bus services.
/// </summary>
public static class ServiceBusExtensions
{
    /// <summary>
    /// Registers a named Azure Service Bus client, configured senders, and resource initializer,
    /// with an additional configuration delegate that has access to the resolved <see cref="IServiceProvider" />.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the <see cref="ServiceBusClient" />.</param>
    /// <param name="nameOrConnectionString">A Service Bus connection string, connection string name, or configuration key.</param>
    /// <param name="configureBus">
    /// A delegate used to declare the queues and topics (with their subscriptions) to initialize and register senders for.
    /// Queues and topics must be added here, as their names are required to register senders eagerly.
    /// </param>
    /// <param name="configureOptions">
    /// An optional delegate used to configure runtime options (such as <see cref="ServiceBusOptions.NameSuffix" />)
    /// using values resolved from the <see cref="IServiceProvider" />, for example the current environment name.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddServiceBus(
        this IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        Action<ServiceBusEntityBuilder> configureBus,
        Action<ServiceBusOptionsBuilder>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrConnectionString);
        ArgumentNullException.ThrowIfNull(configureBus);

        // named options instance; senders, clients and the initializer all resolve options by this name
        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        services
            .AddOptions<ServiceBusOptions>(optionsName)
            .Configure(options =>
            {
                options.ServiceKey = serviceName;
                options.NameOrConnectionString = nameOrConnectionString;
                configureBus.Invoke(new ServiceBusEntityBuilder(options));
            })
            .PostConfigure<IServiceProvider>((options, serviceProvider) =>
                configureOptions?.Invoke(new ServiceBusOptionsBuilder(options, serviceProvider)));

        // track the registration so the initializer can enumerate all configured Service Bus instances
        services.AddSingleton(new ServiceBusRegistration(serviceName, optionsName));

        // expose the configured options as a keyed service for direct resolution
        services.TryAddKeyedSingleton(serviceName, (sp, _) => GetOptions(sp, optionsName));

        RegisterClients(services, serviceName, optionsName);
        RegisterSenders(services, configureBus, serviceName, nameOrConnectionString, optionsName);

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


    // resolve fully-configured options (including service-provider configuration) by name
    private static ServiceBusOptions GetOptions(IServiceProvider serviceProvider, string optionsName)
        => serviceProvider.GetRequiredService<IOptionsMonitor<ServiceBusOptions>>().Get(optionsName);

    private static void RegisterClients(IServiceCollection services, object? serviceName, string optionsName)
    {
        // register ServiceBusClient for general use, keyed by service name
        services.TryAddKeyedSingleton(
            serviceKey: serviceName,
            implementationFactory: (sp, _) =>
            {
                var options = GetOptions(sp, optionsName);
                var connectionString = sp.ResolveConnectionString(options.NameOrConnectionString);
                return new ServiceBusClient(connectionString);
            }
        );

        // register ServiceBusAdministrationClient for management operations, keyed by service name
        services.TryAddKeyedSingleton(
            serviceKey: serviceName,
            implementationFactory: (sp, _) =>
            {
                var options = GetOptions(sp, optionsName);
                var connectionString = sp.ResolveConnectionString(options.NameOrConnectionString);
                return new ServiceBusAdministrationClient(connectionString);
            }
        );
    }

    private static void RegisterSenders(
        IServiceCollection services,
        Action<ServiceBusEntityBuilder> configure,
        object? serviceName,
        string nameOrConnectionString,
        string optionsName)
    {
        // enumerate queue/topic names eagerly so senders can be registered keyed by base name;
        // names are structural and must be set in the eager configure delegate
        var nameOptions = new ServiceBusOptions
        {
            ServiceKey = serviceName,
            NameOrConnectionString = nameOrConnectionString,
        };
        configure.Invoke(new ServiceBusEntityBuilder(nameOptions));

        // register senders for queues and topics, keyed by queue/topic name for resolution
        foreach (var queue in nameOptions.Queues.Concat(nameOptions.Topics))
        {
            services.AddKeyedSingleton(queue, (sp, _) =>
            {
                var options = GetOptions(sp, optionsName);

                // if NameSuffix is provided, append to queue/topic name for sender resolution
                var queueName = options.FormatName(queue);
                var serviceBusClient = sp.GetRequiredKeyedService<ServiceBusClient>(options.ServiceKey);
                return serviceBusClient.CreateSender(queueName);
            });
        }
    }
}
