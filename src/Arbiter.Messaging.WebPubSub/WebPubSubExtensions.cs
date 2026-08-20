using Azure.Core;
using Azure.Identity;
using Azure.Messaging.WebPubSub;

using System.Collections.Frozen;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides extension methods for registering Azure Web PubSub services.
/// </summary>
public static class WebPubSubExtensions
{
    /// <summary>
    /// Registers named Azure Web PubSub service clients for the declared hubs.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the Web PubSub clients and options.</param>
    /// <param name="nameOrConnectionString">A Web PubSub connection string, connection string name, or configuration key.</param>
    /// <param name="configureHubs">
    /// A delegate used to declare the hubs, and the groups associated with them, to register service clients for.
    /// Hubs must be added here, as their names are required to register service clients eagerly.
    /// </param>
    /// <param name="configureOptions">
    /// An optional delegate used to configure runtime options (such as <see cref="WebPubSubOptions.HubSuffix" />
    /// or <see cref="WebPubSubOptions.GroupSuffix" />) using values resolved from the
    /// <see cref="IServiceProvider" />, for example the current environment name.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSub(
        this IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        Action<WebPubSubBuilder> configureHubs,
        Action<WebPubSubOptionsBuilder>? configureOptions = null)
        => AddWebPubSubCore(
            services,
            serviceName,
            nameOrConnectionString,
            credential: null,
            configureHubs,
            configureOptions);

    /// <summary>
    /// Registers named Azure Web PubSub service clients for the declared hubs using identity-based authentication.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the Web PubSub clients and options.</param>
    /// <param name="endpoint">
    /// The Web PubSub service endpoint (for example, <c>https://my-service.webpubsub.azure.com</c>),
    /// a configuration key, or a connection string name that resolves to one.
    /// </param>
    /// <param name="credential">The <see cref="TokenCredential" /> used to authenticate, such as <c>DefaultAzureCredential</c>.</param>
    /// <param name="configureHubs">
    /// A delegate used to declare the hubs, and the groups associated with them, to register service clients for.
    /// </param>
    /// <param name="configureOptions">
    /// An optional delegate used to configure runtime options using values resolved from the <see cref="IServiceProvider" />.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSub(
        this IServiceCollection services,
        object? serviceName,
        string endpoint,
        TokenCredential credential,
        Action<WebPubSubBuilder> configureHubs,
        Action<WebPubSubOptionsBuilder>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(credential);

        return AddWebPubSubCore(
            services,
            serviceName,
            endpoint,
            credential,
            configureHubs,
            configureOptions);
    }


    /// <summary>
    /// Registers a publisher for an Azure Web PubSub hub using the groups declared for the hub by
    /// <see cref="AddWebPubSub(IServiceCollection, object, string, Action{WebPubSubBuilder}, Action{WebPubSubOptionsBuilder})" />.
    /// </summary>
    /// <typeparam name="TPublisher">The publisher type deriving from <see cref="WebPubSubPublisherBase" />.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered Web PubSub clients and options.</param>
    /// <param name="hubName">The hub name to publish messages to.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSubPublisher<TPublisher>(
        this IServiceCollection services,
        object? serviceName,
        string hubName)
        where TPublisher : WebPubSubPublisherBase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        services.TryAddSingleton(sp =>
        {
            var context = CreateHubContext(sp, optionsName, hubName);
            return ActivatorUtilities.CreateInstance<TPublisher>(sp, context);
        });

        return services;
    }


    /// <summary>
    /// Registers a listener processor for an Azure Web PubSub hub using the groups declared for the hub by
    /// <see cref="AddWebPubSub(IServiceCollection, object, string, Action{WebPubSubBuilder}, Action{WebPubSubOptionsBuilder})" />.
    /// </summary>
    /// <typeparam name="TProcessor">The processor type deriving from <see cref="WebPubSubProcessorBase" />.</typeparam>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered Web PubSub clients and options.</param>
    /// <param name="hubName">The hub name to listen to messages on.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSubProcessor<TProcessor>(
        this IServiceCollection services,
        object? serviceName,
        string hubName)
        where TProcessor : WebPubSubProcessorBase
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        // register the processor as a singleton and expose the same instance as a hosted service
        services.TryAddSingleton(sp =>
        {
            var context = CreateHubContext(sp, optionsName, hubName);
            return ActivatorUtilities.CreateInstance<TProcessor>(sp, context);
        });

        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<TProcessor>());

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


    private static IServiceCollection AddWebPubSubCore(
        IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        TokenCredential? credential,
        Action<WebPubSubBuilder> configureHubs,
        Action<WebPubSubOptionsBuilder>? configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrConnectionString);
        ArgumentNullException.ThrowIfNull(configureHubs);

        // named options instance; service clients and processors all resolve options by this name
        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        // invoke the configure delegate once to capture the declared hubs; the resulting
        // names are reused for both options configuration and eager client registration
        var entityOptions = new WebPubSubOptions
        {
            ServiceKey = serviceName,
            NameOrConnectionString = nameOrConnectionString,
            Credential = credential,
        };
        configureHubs.Invoke(new WebPubSubBuilder(entityOptions));

        services
            .AddOptions<WebPubSubOptions>(optionsName)
            .Configure(options =>
            {
                options.ServiceKey = serviceName;
                options.NameOrConnectionString = nameOrConnectionString;
                options.Credential = credential;
                options.Hubs = entityOptions.Hubs;
                options.Groups = entityOptions.Groups;
            })
            .PostConfigure<IServiceProvider>((options, serviceProvider) =>
                configureOptions?.Invoke(new WebPubSubOptionsBuilder(options, serviceProvider)));

        // track the registration so consumers can enumerate all configured Web PubSub instances
        services.AddSingleton(new WebPubSubRegistration(serviceName, optionsName));

        // expose the configured options as a keyed service for direct resolution
        services.TryAddKeyedSingleton(serviceName, (sp, _) => GetOptions(sp, optionsName));

        RegisterClients(services, entityOptions, optionsName);

        return services;
    }


    private static WebPubSubOptions GetOptions(
        IServiceProvider serviceProvider,
        string optionsName)
        => serviceProvider.GetRequiredService<IOptionsMonitor<WebPubSubOptions>>().Get(optionsName);


    private static void RegisterClients(
        IServiceCollection services,
        WebPubSubOptions entityOptions,
        string optionsName)
    {
        // register a service client per hub, keyed by the declared hub name;
        // hubs were captured from the single configure invocation in AddWebPubSub
        foreach (var hub in entityOptions.Hubs)
        {
            services.TryAddKeyedSingleton(hub, (sp, _) => CreateServiceClient(sp, optionsName, hub));
        }
    }

    private static WebPubSubServiceClient CreateServiceClient(
        IServiceProvider services,
        string optionsName,
        string hubName)
    {
        var options = GetOptions(services, optionsName);

        // if suffix is provided, append to the hub name for client resolution
        var name = options.FormatHub(hubName);
        var configuration = services.ResolveConnectionString(options.NameOrConnectionString);

        if (IsConnectionString(configuration))
            return new WebPubSubServiceClient(configuration, name);

        var endpoint = BuildEndpointUri(configuration);
        var credential = options.Credential ?? new DefaultAzureCredential();

        return new WebPubSubServiceClient(endpoint, name, credential);
    }

    internal static WebPubSubHubContext CreateHubContext(
        IServiceProvider services,
        string optionsName,
        string hubName)
    {
        var options = GetOptions(services, optionsName);
        var serviceClient = services.GetRequiredKeyedService<WebPubSubServiceClient>(hubName);

        options.Groups.TryGetValue(hubName, out var groups);

        // if GroupSuffix is provided, append to the group names for listener resolution
        var groupNames = (groups ?? []).ToFrozenDictionary(
            group => group,
            options.FormatGroup,
            StringComparer.OrdinalIgnoreCase);

        return new WebPubSubHubContext(serviceClient, options, options.FormatHub(hubName), groupNames);
    }


    private static Uri BuildEndpointUri(string configuration)
    {
        if (Uri.TryCreate(configuration, UriKind.Absolute, out var endpointUri))
            return endpointUri;

        configuration = configuration.Trim().TrimEnd('/');

        var host = configuration.Contains('.', StringComparison.Ordinal)
            ? configuration
            : $"{configuration}.webpubsub.azure.com";

        return host.ToUri();
    }

    private static bool IsConnectionString(string configuration)
    {
        return configuration.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase)
            || configuration.Contains("AccessKey=", StringComparison.OrdinalIgnoreCase);
    }
}
