using System.Net.Http.Headers;
using System.Net.Mime;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Functions;

/// <summary>
/// Provides extension methods for registering Azure Functions clients and related health checks.
/// </summary>
public static class FunctionExtensions
{
    /// <summary>
    /// Adds the default <see cref="FunctionClient"/> with optional options configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An optional action used to configure <see cref="FunctionClientOptions"/>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddFunctionClient(
        this IServiceCollection services,
        Action<FunctionClientOptions>? configureOptions = null)
        => AddFunctionClient<FunctionClient>(services, configureOptions);

    /// <summary>
    /// Adds a typed <see cref="FunctionClient"/> with optional options configuration.
    /// </summary>
    /// <typeparam name="TClient">The typed function client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">An optional action used to configure <see cref="FunctionClientOptions"/>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddFunctionClient<TClient>(
        this IServiceCollection services,
        Action<FunctionClientOptions>? configureOptions = null)
        where TClient : FunctionClient
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsName = typeof(TClient).FullName ?? typeof(TClient).Name;
        var optionsBuilder = services.AddOptions<FunctionClientOptions>(optionsName);

        if (configureOptions is not null)
            optionsBuilder.Configure(configureOptions);

        services
            .AddHttpClient<TClient>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var options = serviceProvider.GetRequiredService<IOptionsMonitor<FunctionClientOptions>>().Get(optionsName);

                var functionBaseAddress = options.BaseAddress;
                if (string.IsNullOrWhiteSpace(functionBaseAddress) && !string.IsNullOrWhiteSpace(options.BaseAddressConfigurationKey))
                    functionBaseAddress = configuration.GetValue<string>(options.BaseAddressConfigurationKey);

                var functionMasterKey = options.MasterKey;
                if (string.IsNullOrWhiteSpace(functionMasterKey) && !string.IsNullOrWhiteSpace(options.MasterKeyConfigurationKey))
                    functionMasterKey = configuration.GetValue<string>(options.MasterKeyConfigurationKey);

                if (string.IsNullOrWhiteSpace(functionBaseAddress) || string.IsNullOrWhiteSpace(functionMasterKey))
                {
                    throw new InvalidOperationException(
                        $"Function client requires base address and master key. " +
                        $"Configure '{nameof(FunctionClientOptions.BaseAddress)}'/'{nameof(FunctionClientOptions.MasterKey)}' " +
                        $"directly or provide valid configuration keys '{options.BaseAddressConfigurationKey}'/'{options.MasterKeyConfigurationKey}'.");
                }

                if (Uri.TryCreate(functionBaseAddress, UriKind.Absolute, out var absoluteUri))
                    client.BaseAddress = absoluteUri;
                else
                    client.BaseAddress = new Uri(Uri.UriSchemeHttps + Uri.SchemeDelimiter + functionBaseAddress, UriKind.Absolute);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Add("x-functions-key", functionMasterKey);
            });

        return services;
    }


    /// <summary>
    /// Adds a health check for the default <see cref="FunctionClient"/>.
    /// </summary>
    /// <param name="health">The health check builder.</param>
    /// <param name="name">The health check name. When <see langword="null"/>, <see cref="FunctionClient"/> name is used.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder" /> instance for chaining.</returns>
    public static IHealthChecksBuilder AddFunctionClient(
        this IHealthChecksBuilder health,
        string? name = null)
        => AddFunctionClient<FunctionClient>(health, name);

    /// <summary>
    /// Adds a health check for a registered typed function client.
    /// </summary>
    /// <typeparam name="TClient">The typed function client implementation.</typeparam>
    /// <param name="health">The health check builder.</param>
    /// <param name="name">The health check name. When <see langword="null"/>, <typeparamref name="TClient"/> name is used.</param>
    /// <returns>The same <see cref="IHealthChecksBuilder" /> instance for chaining.</returns>
    public static IHealthChecksBuilder AddFunctionClient<TClient>(
        this IHealthChecksBuilder health,
        string? name = null)
        where TClient : FunctionClient
    {
        var healthCheckRegistration = new HealthCheckRegistration(
            name: name ?? typeof(TClient).Name,
            factory: sp =>
            {
                return new FunctionHealthCheck(
                    sp.GetRequiredService<ILogger<FunctionHealthCheck>>(),
                    sp.GetRequiredService<TClient>()
                );
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["Function"]);

        health.Add(healthCheckRegistration);

        return health;
    }
}
