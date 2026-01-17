using System.Net;

using Arbiter.CommandQuery;
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
    /// Adds the remote MessagePack dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client with service provider.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using both the service provider and HTTP client instance.
    /// The HTTP client is configured to use HTTP/2 by default with a fallback policy.
    /// </remarks>
    public static IHttpClientBuilder AddMessagePackDispatcher(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMessagePackDispatcher();
        return services.AddHttpClient<MessagePackDispatcher>((sp, client) =>
        {
            // Set HTTP/2 by default
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            configureClient(sp, client);
        });
    }

    /// <summary>
    /// Adds the remote MessagePack dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using the HTTP client instance only.
    /// The HTTP client is configured to use HTTP/2 by default with a fallback policy.
    /// </remarks>
    public static IHttpClientBuilder AddMessagePackDispatcher(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMessagePackDispatcher();
        return services.AddHttpClient<MessagePackDispatcher>(client =>
        {
            // Set HTTP/2 by default
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            configureClient(client);
        });
    }

    /// <summary>
    /// Adds the remote MessagePack dispatcher to the service collection without HTTP client configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>This method registers the remote dispatcher without configuring the HTTP client.
    /// The client must register the <see cref="MessagePackDispatcher"/> with the correct <see cref="HttpClient"/> separately.</para>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description>MessagePack serializer options (singleton)</description></item>
    /// <item><description><see cref="IDispatcher"/> implementation (transient)</description></item>
    /// <item><description><see cref="IDispatcherDataService"/> implementation (transient)</description></item>
    /// <item><description>Model state management services (scoped)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddMessagePackDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // MessagePack Serializer Options Registration
        services.AddMessagePackOptions();

        // up to client to register MessagePackDispatcher with correct HttpClient
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<MessagePackDispatcher>());

        services.AddDispatcherServices();

        return services;
    }


    /// <summary>
    /// Adds the remote JSON dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client with service provider.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using both the service provider and HTTP client instance.
    /// The HTTP client is configured to use HTTP/2 by default with a fallback policy.
    /// </remarks>
    public static IHttpClientBuilder AddJsonDispatcher(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddJsonDispatcher();
        return services.AddHttpClient<JsonDispatcher>((sp, client) =>
        {
            // Set HTTP/2 by default
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            configureClient(sp, client);
        });
    }

    /// <summary>
    /// Adds the remote JSON dispatcher to the service collection with configuration for the HTTP client.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> for further configuration of the HTTP client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// This overload allows configuration of the HTTP client using the HTTP client instance only.
    /// The HTTP client is configured to use HTTP/2 by default with a fallback policy.
    /// </remarks>
    public static IHttpClientBuilder AddJsonDispatcher(this IServiceCollection services, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddJsonDispatcher();
        return services.AddHttpClient<JsonDispatcher>(client =>
        {
            // Set HTTP/2 by default
            client.DefaultRequestVersion = HttpVersion.Version20;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            configureClient(client);
        });
    }

    /// <summary>
    /// Adds the remote JSON dispatcher to the service collection without HTTP client configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <remarks>
    /// <para>This method registers the remote JSON dispatcher without configuring the HTTP client.
    /// The client must register the <see cref="JsonDispatcher"/> with the correct <see cref="HttpClient"/> separately.</para>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description><see cref="IDispatcher"/> implementation (transient)</description></item>
    /// <item><description><see cref="IDispatcherDataService"/> implementation (transient)</description></item>
    /// <item><description>Model state management services (scoped)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddJsonDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // up to client to register JsonDispatcher with correct HttpClient
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<JsonDispatcher>());

        services.AddDispatcherServices();

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

        services.AddDispatcherServices();

        return services;
    }


    /// <summary>
    /// Adds common dispatcher services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// <para>This method registers common services used by all dispatcher implementations.</para>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description><see cref="IDispatcherDataService"/> implementation (transient)</description></item>
    /// <item><description><see cref="ModelStateManager{TModel}"/> for model state management (scoped)</description></item>
    /// <item><description><see cref="ModelStateLoader{TModel, TKey}"/> for loading model state (scoped)</description></item>
    /// <item><description><see cref="ModelStateEditor{TModel, TKey, TUpdate}"/> for editing model state (scoped)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddDispatcherServices(this IServiceCollection services)
    {
        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.TryAdd(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }
}
