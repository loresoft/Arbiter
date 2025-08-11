using Arbiter.CommandQuery.Definitions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Extensions for adding Arbiter endpoints to an ASP.NET Core application.
/// </summary>
public static class EndpointRouteExtensions
{
    /// <summary>
    /// Adds route endpoints to the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddEndpointRoutes(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<IBaseAddressResolver, BaseAddressResolver>();

        // allow duplicates
        services.AddSingleton<IEndpointRoute, DispatcherEndpoint>();

        return services;
    }

    /// <summary>
    /// Maps route endpoints to the specified route builder. Uses service provider to get list of <see cref="IEndpointRoute"/> instances.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="prefix">Optional route prefix.  Default is "/api"</param>
    /// <param name="serviceKey">Optional service key to get list of <see cref="IEndpointRoute"/> instances from the service provider</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapEndpointRoutes(
        this IEndpointRouteBuilder endpoints,
        string prefix = "/api",
        string? serviceKey = null)
    {
        var routeGroup = endpoints.MapGroup(prefix);

        var endpointRoutes = string.IsNullOrEmpty(serviceKey)
            ? endpoints.ServiceProvider.GetServices<IEndpointRoute>()
            : endpoints.ServiceProvider.GetKeyedServices<IEndpointRoute>(serviceKey);

        foreach (var feature in endpointRoutes)
            feature.AddRoutes(routeGroup);

        return routeGroup;
    }
}
