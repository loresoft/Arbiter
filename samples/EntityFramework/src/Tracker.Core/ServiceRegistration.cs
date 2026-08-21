using Arbiter.CommandQuery;
using Arbiter.Messaging.WebPubSub;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tracker;

internal static class ServiceRegistration
{
    [RegisterServices]
    public static void Register(IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;
        var environment = builder.Environment;

        services.AddHybridCache();
        services.AddEntityHybridCacheQueryBehavior();

        // distributed cache expiration; only enabled when a Web PubSub connection is configured.
        var webPubSubConnection = configuration.GetConnectionString("WebPubSub");
        if (string.IsNullOrWhiteSpace(webPubSubConnection))
            return;

        services
            .AddWebPubSub(
                serviceName: "Tracker",
                nameOrConnectionString: webPubSubConnection,
                configureHubs: hubs => hubs.AddHub("cache", "expire"),
                configureOptions: options => options.WithGroupSuffix(environment.EnvironmentName));

        services
            .AddWebPubSubCacheExpire(
                serviceName: "Tracker",
                hubName: "cache",
                groupName: "expire");

    }
}
