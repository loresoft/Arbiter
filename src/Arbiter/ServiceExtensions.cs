using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter;

/// <summary>
/// Extension methods for service collection.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds Arbiter services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add Arbiter services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddArbiter(this IServiceCollection services)
    {
        services.TryAddSingleton<IMediator, Mediator>();
        return services;
    }
}
