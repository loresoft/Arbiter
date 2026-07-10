using Arbiter.Mediation;
using Arbiter.Queue;

using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using MessagePack;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        => AddServiceBusCore(
            services,
            serviceName,
            nameOrConnectionString,
            credential: null,
            configureBus,
            configureOptions);

    /// <summary>
    /// Registers a named Azure Service Bus client using identity-based authentication, configured senders,
    /// and resource initializer, with an additional configuration delegate that has access to the resolved
    /// <see cref="IServiceProvider" />.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the <see cref="ServiceBusClient" />.</param>
    /// <param name="fullyQualifiedNamespace">
    /// The fully qualified Service Bus namespace (for example, <c>my-namespace.servicebus.windows.net</c>),
    /// a configuration key, or a connection string name that resolves to one.
    /// </param>
    /// <param name="credential">The <see cref="TokenCredential" /> used to authenticate, such as <c>DefaultAzureCredential</c>.</param>
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
        string fullyQualifiedNamespace,
        TokenCredential credential,
        Action<ServiceBusEntityBuilder> configureBus,
        Action<ServiceBusOptionsBuilder>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(credential);

        return AddServiceBusCore(
            services,
            serviceName,
            fullyQualifiedNamespace,
            credential,
            configureBus,
            configureOptions);
    }


    /// <summary>
    /// Registers a message processor for an Azure Service Bus queue.
    /// </summary>
    /// <typeparam name="TProcessor">The processor type deriving from <see cref="ServiceBusProcessorBase" />.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered <see cref="ServiceBusClient" /> and options.</param>
    /// <param name="queueName">The queue name to process messages from.</param>
    /// <param name="configureProcessor">
    /// An optional delegate used to configure the <see cref="ServiceBusProcessorOptions" /> (such as
    /// <see cref="ServiceBusProcessorOptions.MaxConcurrentCalls" /> or <see cref="ServiceBusProcessorOptions.AutoCompleteMessages" />).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusProcessor<TProcessor>(
        this IServiceCollection services,
        object? serviceName,
        string queueName,
        Action<ServiceBusProcessorOptions>? configureProcessor = null)
        where TProcessor : ServiceBusProcessorBase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        // register the processor keyed by queue name, mirroring the sender keying scheme
        services.TryAddKeyedSingleton(
            serviceKey: queueName,
            implementationFactory: (sp, _) => CreateProcessor(sp, optionsName, queueName, configureProcessor));

        RegisterProcessor<TProcessor>(services, queueName);

        return services;
    }

    /// <summary>
    /// Registers a message processor for an Azure Service Bus topic subscription.
    /// </summary>
    /// <typeparam name="TProcessor">The processor type deriving from <see cref="ServiceBusProcessorBase" />.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered <see cref="ServiceBusClient" /> and options.</param>
    /// <param name="topicName">The topic name to process messages from.</param>
    /// <param name="subscriptionName">The subscription name to process messages from.</param>
    /// <param name="configureProcessor">
    /// An optional delegate used to configure the <see cref="ServiceBusProcessorOptions" /> (such as
    /// <see cref="ServiceBusProcessorOptions.MaxConcurrentCalls" /> or <see cref="ServiceBusProcessorOptions.AutoCompleteMessages" />).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusProcessor<TProcessor>(
        this IServiceCollection services,
        object? serviceName,
        string topicName,
        string subscriptionName,
        Action<ServiceBusProcessorOptions>? configureProcessor = null)
        where TProcessor : ServiceBusProcessorBase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        // composite key matches the "topic/subscription" structure of the processed entity
        var processorKey = $"{topicName}/{subscriptionName}";

        // register the processor keyed by the topic/subscription composite key
        services.TryAddKeyedSingleton(processorKey, (sp, _) =>
        {
            var options = GetOptions(sp, optionsName);

            // if NameSuffix is provided, append to topic name for processor resolution
            var entityName = options.FormatName(topicName);
            var serviceBusClient = sp.GetRequiredKeyedService<ServiceBusClient>(options.ServiceKey);

            var processorOptions = new ServiceBusProcessorOptions();
            configureProcessor?.Invoke(processorOptions);

            return serviceBusClient.CreateProcessor(entityName, subscriptionName, processorOptions);
        });

        RegisterProcessor<TProcessor>(services, processorKey);

        return services;
    }


    /// <summary>
    /// Registers a Service Bus backed background queue for mediator requests.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered <see cref="ServiceBusClient" /> and options.</param>
    /// <param name="queueName">The queue name used to store background requests.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusBackgroundQueue(
        this IServiceCollection services,
        object? serviceName,
        string queueName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        services.TryAddKeyedSingleton(
            serviceKey: queueName,
            implementationFactory: (sp, _) => CreateSender(sp, optionsName, queueName));

        services.TryAddSingleton<IBackgroundQueue>(sp =>
        {
            var sender = sp.GetRequiredKeyedService<ServiceBusSender>(queueName);
            var options = sp.GetService<MessagePackSerializerOptions>();

            return new ServiceBusBackgroundQueue(sender, options);
        });

        services.TryAddSingleton<ServiceBusBackgroundService>();

        return services;
    }

    /// <summary>
    /// Registers a Service Bus processor for background mediator requests stored in a queue.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered <see cref="ServiceBusClient" /> and options.</param>
    /// <param name="queueName">The queue name to process background requests from.</param>
    /// <param name="configureProcessor">An optional delegate used to configure the Service Bus processor.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusBackgroundProcessor(
        this IServiceCollection services,
        object? serviceName,
        string queueName,
        Action<ServiceBusProcessorOptions>? configureProcessor = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        services.AddMediator();
        services.TryAddSingleton<ServiceBusBackgroundService>();

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        // register the processor keyed by queue name, mirroring the sender keying scheme
        services.TryAddKeyedSingleton(
            serviceKey: queueName,
            implementationFactory: (sp, _) => CreateProcessor(sp, optionsName, queueName, configureProcessor));

        RegisterProcessor<ServiceBusBackgroundProcessor>(services, queueName);

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


    private static IServiceCollection AddServiceBusCore(
        IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        TokenCredential? credential,
        Action<ServiceBusEntityBuilder> configureBus,
        Action<ServiceBusOptionsBuilder>? configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrConnectionString);
        ArgumentNullException.ThrowIfNull(configureBus);

        // named options instance; senders, clients and the initializer all resolve options by this name
        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        // invoke the configure delegate once to capture the declared entities; the resulting
        // names are reused for both options configuration and eager sender registration
        var entityOptions = new ServiceBusOptions
        {
            ServiceKey = serviceName,
            NameOrConnectionString = nameOrConnectionString,
            Credential = credential,
        };
        configureBus.Invoke(new ServiceBusEntityBuilder(entityOptions));

        services
            .AddOptions<ServiceBusOptions>(optionsName)
            .Configure(options =>
            {
                options.ServiceKey = serviceName;
                options.NameOrConnectionString = nameOrConnectionString;
                options.Credential = credential;
                options.Queues = entityOptions.Queues;
                options.Topics = entityOptions.Topics;
                options.Subscriptions = entityOptions.Subscriptions;
            })
            .PostConfigure<IServiceProvider>((options, serviceProvider) =>
                configureOptions?.Invoke(new ServiceBusOptionsBuilder(options, serviceProvider)));

        // track the registration so the initializer can enumerate all configured Service Bus instances
        services.AddSingleton(new ServiceBusRegistration(serviceName, optionsName));

        // expose the configured options as a keyed service for direct resolution
        services.TryAddKeyedSingleton(serviceName, (sp, _) => GetOptions(sp, optionsName));

        RegisterClients(services, serviceName, optionsName);
        RegisterSenders(services, entityOptions, optionsName);

        services.AddHostedService<ServiceBusInitializer>();

        return services;
    }


    private static ServiceBusOptions GetOptions(
        IServiceProvider serviceProvider,
        string optionsName)
        => serviceProvider.GetRequiredService<IOptionsMonitor<ServiceBusOptions>>().Get(optionsName);


    private static void RegisterClients(
        IServiceCollection services,
        object? serviceName,
        string optionsName)
    {
        // register ServiceBusClient for general use, keyed by service name
        services.TryAddKeyedSingleton(
            serviceKey: serviceName,
            implementationFactory: (sp, _) =>
            {
                var options = GetOptions(sp, optionsName);
                var serviceBusConfiguration = sp.ResolveConnectionString(options.NameOrConnectionString);

                if (IsConnectionString(serviceBusConfiguration))
                    return new ServiceBusClient(serviceBusConfiguration);

                var fullyQualifiedNamespace = BuildFullyQualifiedNamespace(serviceBusConfiguration);
                var credential = options.Credential ?? new DefaultAzureCredential();

                return new ServiceBusClient(fullyQualifiedNamespace, credential);
            }
        );

        // register ServiceBusAdministrationClient for management operations, keyed by service name
        services.TryAddKeyedSingleton(
            serviceKey: serviceName,
            implementationFactory: (sp, _) =>
            {
                var options = GetOptions(sp, optionsName);
                var serviceBusConfiguration = sp.ResolveConnectionString(options.NameOrConnectionString);

                if (IsConnectionString(serviceBusConfiguration))
                    return new ServiceBusAdministrationClient(serviceBusConfiguration);

                var fullyQualifiedNamespace = BuildFullyQualifiedNamespace(serviceBusConfiguration);
                var credential = options.Credential ?? new DefaultAzureCredential();

                return new ServiceBusAdministrationClient(fullyQualifiedNamespace, credential);
            }
        );
    }

    private static void RegisterSenders(
        IServiceCollection services,
        ServiceBusOptions entityOptions,
        string optionsName)
    {
        // register senders for queues and topics, keyed by queue/topic name for resolution;
        // names were captured from the single configure invocation in AddServiceBus
        foreach (var queue in entityOptions.Queues.Concat(entityOptions.Topics))
        {
            services.TryAddKeyedSingleton(queue, (sp, _) => CreateSender(sp, optionsName, queue));
        }
    }

    private static void RegisterProcessor<TProcessor>(
        IServiceCollection services,
        object processorKey)
        where TProcessor : ServiceBusProcessorBase
    {
        // register the processor as a singleton, resolving the keyed ServiceBusProcessor for this entity;
        // the same instance is exposed as a hosted service so only one instance runs
        services.TryAddSingleton(sp =>
        {
            var processor = sp.GetRequiredKeyedService<ServiceBusProcessor>(processorKey);
            return ActivatorUtilities.CreateInstance<TProcessor>(sp, processor);
        });

        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<TProcessor>());
    }


    private static string BuildFullyQualifiedNamespace(string serviceBusConfiguration)
    {
        if (Uri.TryCreate(serviceBusConfiguration, UriKind.Absolute, out var serviceBusUri))
            return serviceBusUri.Host;

        serviceBusConfiguration = serviceBusConfiguration.Trim().TrimEnd('/');

        return serviceBusConfiguration.Contains('.', StringComparison.Ordinal)
            ? serviceBusConfiguration
            : $"{serviceBusConfiguration}.servicebus.windows.net";
    }

    private static bool IsConnectionString(string serviceBusConfiguration)
    {
        return serviceBusConfiguration.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase)
            || serviceBusConfiguration.Contains("SharedAccessKeyName=", StringComparison.OrdinalIgnoreCase)
            || serviceBusConfiguration.Contains("SharedAccessKey=", StringComparison.OrdinalIgnoreCase)
            || serviceBusConfiguration.Contains("UseDevelopmentEmulator=true", StringComparison.OrdinalIgnoreCase);
    }


    private static ServiceBusSender CreateSender(
        IServiceProvider services,
        string optionsName,
        string queueName)
    {
        var options = GetOptions(services, optionsName);

        // if NameSuffix is provided, append to queue/topic name for sender resolution
        var name = options.FormatName(queueName);
        var serviceBusClient = services.GetRequiredKeyedService<ServiceBusClient>(options.ServiceKey);

        return serviceBusClient.CreateSender(name);
    }

    private static ServiceBusProcessor CreateProcessor(
        IServiceProvider services,
        string optionsName,
        string queueName,
        Action<ServiceBusProcessorOptions>? configureProcessor = null)
    {
        var options = GetOptions(services, optionsName);

        // if NameSuffix is provided, append to queue/topic name for processor resolution
        var name = options.FormatName(queueName);

        var serviceBusClient = services.GetRequiredKeyedService<ServiceBusClient>(options.ServiceKey);

        var processorOptions = new ServiceBusProcessorOptions();
        configureProcessor?.Invoke(processorOptions);

        return serviceBusClient.CreateProcessor(name, processorOptions);
    }
}
