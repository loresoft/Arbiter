using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Messaging.WebPubSub.Tests;

public class TestApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        // dotnet user-secrets set "AzureWebPubSub" "<Connection String>" --id "a6c8f6a5-2d49-4b1b-a7df-0243c7ed11b7"
        builder.Configuration.AddUserSecrets("a6c8f6a5-2d49-4b1b-a7df-0243c7ed11b7");

        builder.Services.AddWebPubSub(
            serviceName: "TestWebPubSub",
            nameOrConnectionString: "AzureWebPubSub",
            configureHubs: hubs => hubs
                .AddHub("testHub", "unit-test")
                .AddHub("cacheExpireTest", "cache-expire-group"),
            configureOptions: options =>
                options.WithHubSuffix(options.Services.GetRequiredService<IHostEnvironment>().EnvironmentName)
        );

        base.ConfigureApplication(builder);
    }
}
