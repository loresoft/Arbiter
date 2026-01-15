using System.Net;

using Arbiter.Dispatcher.Client;
using Arbiter.Dispatcher.State;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Dispatcher;

/// <summary>
/// Provides extension methods for registering dispatcher services with the dependency injection container.
/// </summary>
public static class DispatcherClientExtensions
{
    /// <summary>
    /// Adds the remote dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client with service provider.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using both the service provider and HTTP client instance.
    /// The HTTP client is configured to use HTTP/2 by default with a fallback policy.
    /// </remarks>
    public static IHttpClientBuilder AddRemoteDispatcher(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRemoteDispatcher();
        return services.AddHttpClient<RemoteDispatcher>((sp, client) =>
        {
            // Set HTTP/2 by default
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            configureClient(sp, client);
        });
    }

    /// <summary>
    /// Adds the remote dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using the HTTP client instance only.
    /// The HTTP client is configured to use HTTP/2 by default with a fallback policy.
    /// </remarks>
    public static IHttpClientBuilder AddRemoteDispatcher(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRemoteDispatcher();
        return services.AddHttpClient<RemoteDispatcher>(client =>
        {
            // Set HTTP/2 by default
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            configureClient(client);
        });
    }

    /// <summary>
    /// Adds the remote dispatcher to the service collection without HTTP client configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>This method registers the remote dispatcher without configuring the HTTP client.
    /// The client must register the <see cref="RemoteDispatcher"/> with the correct <see cref="HttpClient"/> separately.</para>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description>MessagePack serializer options (singleton)</description></item>
    /// <item><description><see cref="IDispatcher"/> implementation (transient)</description></item>
    /// <item><description><see cref="IDispatcherDataService"/> implementation (transient)</description></item>
    /// <item><description>Model state management services (scoped)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // MessagePack Serializer Options Registration
        services.TryAddSingleton(DispatcherConstants.DefaultSerializerOptions);

        // up to client to register RemoteDispatcher with correct HttpClient
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<RemoteDispatcher>());
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>The server dispatcher uses the mediator pattern to dispatch commands and queries locally.</para>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description><see cref="IDispatcher"/> implementation (transient)</description></item>
    /// <item><description><see cref="IDispatcherDataService"/> implementation (transient)</description></item>
    /// <item><description>Model state management services (scoped)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddServerDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IDispatcher, ServerDispatcher>();
        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }
}
