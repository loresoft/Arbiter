using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Tracker.Data;

namespace Tracker.Health;

public static class HealthRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        var health = services.AddHealthChecks();

        //default liveness check to ensure app is responsive
        health.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        // add EF database health check
        health.AddDbContextCheck<TrackerContext>();
    }
}
