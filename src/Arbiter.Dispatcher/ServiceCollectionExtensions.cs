using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Dispatcher;
using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Mapping;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Services;
using Arbiter.CommandQuery.State;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.CommandQuery;

/// <summary>
/// Extension methods for adding command query services to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the remote dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client with service provider.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using both the service provider and HTTP client instance.
    /// </remarks>
    public static IHttpClientBuilder AddRemoteDispatcher(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRemoteDispatcher();
        return services.AddHttpClient<RemoteDispatcher>(configureClient);
    }

    /// <summary>
    /// Adds the remote dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using the HTTP client instance only.
    /// </remarks>
    public static IHttpClientBuilder AddRemoteDispatcher(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRemoteDispatcher();
        return services.AddHttpClient<RemoteDispatcher>(configureClient);
    }

    /// <summary>
    /// Adds the remote dispatcher to the service collection without HTTP client configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers the remote dispatcher without configuring the HTTP client.
    /// The client must register the <see cref="RemoteDispatcher"/> with the correct <see cref="HttpClient"/> separately.
    /// </remarks>
    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // up to client to register RemoteDispatcher with correct HttpClient
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<RemoteDispatcher>());
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }

    /// <summary>
    /// Adds the server dispatcher to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// The server dispatcher uses the mediator pattern to dispatch commands and queries locally.
    /// </remarks>
    public static IServiceCollection AddServerDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IDispatcher, MediatorDispatcher>();
        services.AddOptions<DispatcherOptions>();

        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }
}
