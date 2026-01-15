using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Dispatcher.Server;

/// <summary>
/// Provides extension methods for configuring dispatcher services and endpoints.
/// </summary>
public static class DispatcherServiceExtensions
{
    /// <summary>
    /// Adds the dispatcher service and its dependencies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDispatcherService(this IServiceCollection services)
    {
        // Register the DispatcherService itself
        services.TryAddSingleton<DispatcherEndpoint>();

        // MessagePack Serializer Options Registration
        services.TryAddSingleton(DispatcherConstants.DefaultSerializerOptions);

        return services;
    }

    /// <summary>
    /// Maps the dispatcher service endpoint to the application's request pipeline.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to configure.</param>
    /// <returns>An endpoint convention builder that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapDispatcherService(this IEndpointRouteBuilder endpoints)
    {
        // Resolve the DispatcherEndpoint from the service provider
        var dispatcherEndpoint = endpoints.ServiceProvider.GetRequiredService<DispatcherEndpoint>();

        // Map the route using the DispatcherEndpoint
        return dispatcherEndpoint.AddRoute(endpoints);
    }
}
