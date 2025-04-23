using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Data;

public static class DataServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddDbContextPool<TrackerContext>(
            optionsAction: (provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("Tracker");

                options.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure());
            });
    }
}
