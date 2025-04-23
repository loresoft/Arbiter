using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.CommandQuery.Endpoints;

/// <summary>
/// Extensions for adding Arbiter endpoints to an ASP.NET Core application.
/// </summary>
public static class FeatureEndpointExtensions
{
    /// <summary>
    /// Adds feature endpoints to the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddFeatureEndpoints(this IServiceCollection services)
    {
        services.Add(ServiceDescriptor.Transient<IFeatureEndpoint, DispatcherEndpoint>());

        return services;
    }

    /// <summary>
    /// Maps feature endpoints to the specified route builder. Uses service provider to get list of <see cref="IFeatureEndpoint"/> instances.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="prefix">Optional route prefix.  Default is "/api"</param>
    /// <param name="serviceKey">Optional service key to get list of <see cref="IFeatureEndpoint"/> instances from the service provider</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapFeatureFeatureEndpoints(
        this IEndpointRouteBuilder endpoints,
        string prefix = "/api",
        string? serviceKey = null)
    {
        var featureGroup = endpoints.MapGroup(prefix);

        var features = string.IsNullOrEmpty(serviceKey)
            ? endpoints.ServiceProvider.GetServices<IFeatureEndpoint>()
            : endpoints.ServiceProvider.GetKeyedServices<IFeatureEndpoint>(serviceKey);

        foreach (var feature in features)
            feature.AddRoutes(featureGroup);

        return featureGroup;
    }
}
