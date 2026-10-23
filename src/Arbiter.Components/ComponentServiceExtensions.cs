using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Options;
using Arbiter.Components.Abstracts;
using Arbiter.Components.Services;
using Arbiter.Dispatcher;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Components;

/// <summary>
/// Provides extension methods for registering the services used by the Arbiter component base classes with the dependency injection container.
/// </summary>
public static class ComponentServiceExtensions
{
    /// <summary>
    /// Adds the services required by <see cref="ModelComponentBase{TReadModel}"/> and the page base classes derived
    /// from it to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureNotifications">
    /// An optional delegate used to configure the <see cref="NotificationServiceOptions"/> used by
    /// <see cref="NotificationService"/>.
    /// </param>
    /// <param name="configureEnvironment">
    /// An optional delegate used to configure the <see cref="EnvironmentOptions"/>. The delegate runs after the
    /// options are bound from <see cref="IConfiguration"/>, so any values it sets override configured values.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description><see cref="INotificationService"/> implemented by <see cref="NotificationService"/> (scoped)</description></item>
    /// <item><description><see cref="IBaseAddressResolver"/> implemented by <see cref="BaseAddressResolver"/> (scoped)</description></item>
    /// <item><description><see cref="IPermissionResolver"/> implemented by <see cref="DefaultPermissionResolver"/> (scoped); replace it with <see cref="AddPermissionResolver{TResolver}"/></description></item>
    /// <item><description><see cref="IDispatcherDataService"/> implemented by <see cref="PrincipalDataService"/> (transient)</description></item>
    /// </list>
    /// <para>
    /// This method does not register a dispatcher; call one of the <c>AddMessagePackDispatcher</c>,
    /// <c>AddJsonDispatcher</c> or <c>AddServerDispatcher</c> methods as well. The order of the two calls does not
    /// matter: the <see cref="IDispatcherDataService"/> registered here supersedes the default registered by the
    /// dispatcher so that the current user is resolved from the Blazor authentication state. Register a custom
    /// implementation after this method to override it.
    /// </para>
    /// <para>
    /// <see cref="NotificationService"/> displays notifications through an <c>IToaster</c>, which is registered by
    /// calling <c>AddBlazorControls</c>.
    /// </para>
    /// <para>
    /// <see cref="EnvironmentOptions"/> is first bound from <see cref="IConfiguration"/> and then updated by
    /// <paramref name="configureEnvironment"/>, when provided. The options mirror the host environment values for
    /// scenarios where <c>IHostEnvironment</c> is not available, such as Blazor WebAssembly.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddArbiterComponents(
        this IServiceCollection services,
        Action<NotificationServiceOptions>? configureNotifications = null,
        Action<EnvironmentOptions>? configureEnvironment = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configureNotifications != null)
            services.Configure(configureNotifications);

        // binding is registered once and the delegate runs as post-configure, so it overrides configuration in any call order
        services.AddEnvironmentOptions(configureEnvironment);

        // register the IToaster used by NotificationService
        services.AddBlazorControls();

        services.TryAddScoped<INotificationService, NotificationService>();
        services.TryAddScoped<IBaseAddressResolver, BaseAddressResolver>();
        services.TryAddScoped<IPermissionResolver, DefaultPermissionResolver>();

        // the dispatcher registers a default data service with TryAdd, so replace it to stay order independent
        ServiceDescriptor descriptor = ServiceDescriptor.Transient<IDispatcherDataService, PrincipalDataService>();
        services.Replace(descriptor);

        return services;
    }

    /// <summary>
    /// Registers the <see cref="IPermissionResolver"/> used by <see cref="EntitySelect{TModel, TValue}"/> and
    /// <see cref="EntityTypeahead{TItem, TValue}"/> to disable themselves when the current user lacks permission.
    /// </summary>
    /// <typeparam name="TResolver">The <see cref="IPermissionResolver"/> implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="lifetime">The service lifetime of the resolver. Defaults to <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// Any previously registered <see cref="IPermissionResolver"/>, including the <see cref="DefaultPermissionResolver"/>
    /// added by <see cref="AddArbiterComponents"/>, is replaced. The order of the two calls does not matter.
    /// </remarks>
    public static IServiceCollection AddPermissionResolver<TResolver>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TResolver : class, IPermissionResolver
    {
        ArgumentNullException.ThrowIfNull(services);

        ServiceDescriptor descriptor = ServiceDescriptor.Describe(typeof(IPermissionResolver), typeof(TResolver), lifetime);
        services.Replace(descriptor);

        return services;
    }
}
