using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Arbiter.Queue;

/// <summary>
/// Provides extension methods for registering background queue services.
/// </summary>
public static class BackgroundQueueExtensions
{
    /// <summary>
    /// Adds in-process background queue services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddBackgroundQueue(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediator();

        services.TryAddSingleton<BackgroundQueue>();
        services.TryAddSingleton<IBackgroundQueue>(sp => sp.GetRequiredService<BackgroundQueue>());
        services.AddSingleton<IHostedService, BackgroundQueueWorker>();

        return services;
    }
}
