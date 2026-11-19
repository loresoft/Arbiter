using Arbiter.CommandQuery.EntityFramework;
using Arbiter.Health;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Tracker.Data;

namespace Tracker.Health;

public static class HealthRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddHealthServices();

        var health = services.AddHealthChecks();

        //default liveness check to ensure app is responsive
        health.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        health.AddUrlCheck(
            name: "Service API",
            urlFactory: static sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return configuration.GetValue<string>("ServiceBaseAddress")
                    ?? throw new InvalidOperationException("ServiceBaseAddress configuration value is missing.");
            },
            httpMethod: HttpMethod.Head
        );

        // add EF database health check
        health.AddDataContext<TrackerContext>();
    }
}
