using Arbiter.CommandQuery.Endpoints;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.MemoryPack.Endpoints;

/// <summary>
/// Provides extension methods for registering MemoryPack-based endpoint routes with the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MemoryPack-based endpoint routes to the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers the necessary services for MemoryPack-based endpoint routing, including the
    /// <see cref="MemoryPackEndpoint"/> implementation of <see cref="IEndpointRoute"/>.
    /// </remarks>
    public static IServiceCollection AddMemoryPackEndpointRoutes(this IServiceCollection services)
    {
        services.AddEndpointRoutes();
        services.AddSingleton<IEndpointRoute, MemoryPackEndpoint>();

        return services;
    }
}
