using Arbiter.CommandQuery;

using Microsoft.Extensions.DependencyInjection;

namespace Tracker.Services;

internal static class ServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddMediator();
        services.AddHybridCache();
    }
}
