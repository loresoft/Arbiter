using Arbiter.CommandQuery.Definitions;
using Arbiter.Components.Abstracts;
using Arbiter.Components.Services;
using Arbiter.Dispatcher;

using LoreSoft.Blazor.Controls;

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
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>Registered services include:</para>
    /// <list type="bullet">
    /// <item><description><see cref="INotificationService"/> implemented by <see cref="NotificationService"/> (scoped)</description></item>
    /// <item><description><see cref="IBaseAddressResolver"/> implemented by <see cref="BaseAddressResolver"/> (scoped)</description></item>
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
    /// </remarks>
    public static IServiceCollection AddArbiterComponents(
        this IServiceCollection services,
        Action<NotificationServiceOptions>? configureNotifications = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configureNotifications != null)
            services.Configure(configureNotifications);

        // register the IToaster used by NotificationService
        services.AddBlazorControls();

        services.TryAddScoped<INotificationService, NotificationService>();
        services.TryAddScoped<IBaseAddressResolver, BaseAddressResolver>();

        // the dispatcher registers a default data service with TryAdd, so replace it to stay order independent
        services.Replace(ServiceDescriptor.Transient<IDispatcherDataService, PrincipalDataService>());

        return services;
    }
}
