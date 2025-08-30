using Arbiter.CommandQuery.Dispatcher;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.MemoryPack.Dispatcher;

/// <summary>
/// Extension methods for registering MemoryPack-based dispatchers with the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MemoryPack-based dispatcher to the service collection with custom <see cref="HttpClient"/> configuration using service provider.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the <see cref="HttpClient"/> for the <see cref="MemoryPackDispatcher"/> using both service provider and HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <remarks>
    /// This method registers the necessary services for MemoryPack-based dispatching, including the
    /// <see cref="MemoryPackDispatcher"/> implementation of <see cref="IDispatcher"/>.
    /// This overload allows configuration of the HTTP client using both the service provider and HTTP client instance,
    /// which is useful for scenarios requiring dependency injection within the configuration action.
    /// </remarks>
    public static IHttpClientBuilder AddMemoryPackDispatcher(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureClient);

        services.AddMemoryPackDispatcher();
        return services.AddHttpClient<MemoryPackDispatcher>(configureClient);
    }

    /// <summary>
    /// Adds MemoryPack-based dispatcher to the service collection with custom <see cref="HttpClient"/> configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the <see cref="HttpClient"/> for the <see cref="MemoryPackDispatcher"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <remarks>
    /// This method registers the necessary services for MemoryPack-based dispatching, including the
    /// <see cref="MemoryPackDispatcher"/> implementation of <see cref="IDispatcher"/>.
    /// This overload allows configuration of the HTTP client using the HTTP client instance only,
    /// which is suitable for basic configuration scenarios.
    /// </remarks>
    public static IHttpClientBuilder AddMemoryPackDispatcher(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureClient);

        services.AddMemoryPackDispatcher();
        return services.AddHttpClient<MemoryPackDispatcher>(configureClient);
    }

    /// <summary>
    /// Adds MemoryPack-based dispatcher to the service collection without HTTP client configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers the necessary services for MemoryPack-based dispatching, including the
    /// <see cref="MemoryPackDispatcher"/> implementation of <see cref="IDispatcher"/> and related services.
    /// The HTTP client must be configured separately by the client application.
    /// Use this method when you need to register the MemoryPack dispatcher services but want to configure
    /// the HTTP client using a different approach or when the HTTP client is already configured elsewhere.
    /// </remarks>
    public static IServiceCollection AddMemoryPackDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register dispatcher service factory
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<MemoryPackDispatcher>());
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        return services;
    }
}
