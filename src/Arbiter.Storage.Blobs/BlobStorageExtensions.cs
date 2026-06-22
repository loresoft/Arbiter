using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Storage.Blobs;

/// <summary>
/// Provides extension methods for registering Azure Blob Storage containers.
/// </summary>
public static class BlobStorageExtensions
{
    /// <summary>
    /// Registers a named Azure Blob Storage client, configured containers, and resource initializer,
    /// with an additional configuration delegate that has access to the resolved <see cref="IServiceProvider" />.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the Blob Storage clients.</param>
    /// <param name="nameOrConnectionString">A Blob Storage connection string, service URI, connection string name, or configuration key.</param>
    /// <param name="configureOptions">An optional delegate used to configure containers and runtime options.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddBlobStorage(
        this IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        Action<BlobStorageOptions>? configureOptions = null)
        => AddBlobStorageCore(services, serviceName, nameOrConnectionString, credential: null, configureOptions);

    /// <summary>
    /// Registers a named Azure Blob Storage client using identity-based authentication, configured containers,
    /// and resource initializer, with an additional configuration delegate that has access to the resolved
    /// <see cref="IServiceProvider" />.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to register and resolve the Blob Storage clients.</param>
    /// <param name="serviceUri">The Blob Storage service URI, connection string name, or configuration key that resolves to one.</param>
    /// <param name="credential">The <see cref="TokenCredential" /> used to authenticate, such as <c>DefaultAzureCredential</c>.</param>
    /// <param name="configureOptions">An optional delegate used to configure containers and runtime options.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance for chaining.</returns>
    public static IServiceCollection AddBlobStorage(
        this IServiceCollection services,
        object? serviceName,
        string serviceUri,
        TokenCredential credential,
        Action<BlobStorageOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(credential);

        return AddBlobStorageCore(services, serviceName, serviceUri, credential, configureOptions);
    }


    /// <summary>
    /// Adds an Azure Blob Storage container health check registration for the specified keyed container client.
    /// </summary>
    /// <param name="health">The health check builder.</param>
    /// <param name="containerName">The container name and key used to resolve the blob container client.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder" /> instance for chaining.</returns>
    public static IHealthChecksBuilder AddBlobStorage(
        this IHealthChecksBuilder health,
        string containerName)
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name: $"Storage Container: '{containerName}'",
            factory: sp =>
            {
                return new BlobStorageHealthCheck(
                    sp.GetRequiredKeyedService<BlobContainerClient>(containerName),
                    sp.GetRequiredService<ILogger<BlobStorageHealthCheck>>()
                );
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["Storage"]);

        health.Add(healthCheckRegistration);

        return health;
    }


    /// <summary>
    /// Resolves a connection string from either a direct connection string or a configuration key name.
    /// </summary>
    /// <param name="services">The service provider used to resolve application configuration.</param>
    /// <param name="nameOrConnectionString">A direct connection string, service URI, connection string name, or configuration key.</param>
    /// <returns>The resolved connection string or service URI, or the original value when no configuration value is found.</returns>
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

        var connectionString = configuration.GetConnectionString(nameOrConnectionString);
        if (!string.IsNullOrEmpty(connectionString))
            return connectionString!;

        connectionString = configuration[nameOrConnectionString];
        if (!string.IsNullOrEmpty(connectionString))
            return connectionString!;

        return nameOrConnectionString;
    }


    private static IServiceCollection AddBlobStorageCore(
        IServiceCollection services,
        object? serviceName,
        string nameOrConnectionString,
        TokenCredential? credential,
        Action<BlobStorageOptions>? configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(nameOrConnectionString);

        var optionsName = serviceName?.ToString() ?? Options.DefaultName;

        var containerOptions = new BlobStorageOptions
        {
            ServiceKey = serviceName,
            NameOrConnectionString = nameOrConnectionString,
            Credential = credential,
        };

        configureOptions?.Invoke(containerOptions);

        services
            .AddOptions<BlobStorageOptions>(optionsName)
            .Configure(options =>
            {
                options.ServiceKey = containerOptions.ServiceKey;
                options.NameOrConnectionString = containerOptions.NameOrConnectionString;
                options.Credential = containerOptions.Credential;
                options.NameSuffix = containerOptions.NameSuffix;
                options.Containers = containerOptions.Containers;
            });

        services.AddSingleton(new BlobStorageRegistration(serviceName, optionsName));
        services.TryAddKeyedSingleton(serviceName, (sp, _) => GetOptions(sp, optionsName));

        RegisterContainers(services, containerOptions, optionsName);

        services.AddHostedService<BlobStorageInitializer>();

        return services;
    }

    private static BlobStorageOptions GetOptions(
        IServiceProvider serviceProvider,
        string optionsName)
        => serviceProvider.GetRequiredService<IOptionsMonitor<BlobStorageOptions>>().Get(optionsName);

    private static void RegisterContainers(
        IServiceCollection services,
        BlobStorageOptions containerOptions,
        string optionsName)
    {
        foreach (var container in containerOptions.Containers.Keys)
            services.AddKeyedSingleton(container, (sp, _) => CreateContainerClient(sp, optionsName, container));
    }

    private static BlobContainerClient CreateContainerClient(
        IServiceProvider serviceProvider,
        string optionsName,
        string containerName)
    {
        var options = GetOptions(serviceProvider, optionsName);
        var storageConfiguration = serviceProvider.ResolveConnectionString(options.NameOrConnectionString);
        var resolvedContainerName = options.FormatName(containerName);

        if (IsConnectionString(storageConfiguration))
            return new BlobContainerClient(storageConfiguration, resolvedContainerName);

        var containerUri = BuildContainerUri(storageConfiguration, resolvedContainerName);
        var credential = options.Credential ?? new DefaultAzureCredential();

        if (credential is not null && string.IsNullOrWhiteSpace(containerUri.Query))
            return new BlobContainerClient(containerUri, credential);

        return new BlobContainerClient(containerUri);
    }

    private static Uri BuildContainerUri(
        string storageValue,
        string containerName)
    {
        if (!Uri.TryCreate(storageValue, UriKind.Absolute, out var serviceUri))
            return new Uri($"https://{storageValue}.blob.core.windows.net/{containerName}");

        string path = $"{serviceUri.AbsolutePath.TrimEnd('/')}/{containerName}".TrimStart('/');
        var builder = new UriBuilder(serviceUri) { Path = path };

        return builder.Uri;
    }

    private static bool IsConnectionString(string storageConfiguration)
    {
        return storageConfiguration.Contains("AccountName=", StringComparison.OrdinalIgnoreCase)
            || storageConfiguration.Contains("AccountKey=", StringComparison.OrdinalIgnoreCase)
            || storageConfiguration.Contains("SharedAccessSignature=", StringComparison.OrdinalIgnoreCase)
            || storageConfiguration.Contains("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase);
    }
}
