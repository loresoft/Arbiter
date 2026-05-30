using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TestHost.Abstracts;

namespace Arbiter.Messaging.ServiceBus.Tests;

public class TestApplication : TestHostApplication
{
    protected override void ConfigureApplication(HostApplicationBuilder builder)
    {
        // dotnet user-secrets set "AzureWebJobsServiceBus" "<Connection String>" --id "a6c8f6a5-2d49-4b1b-a7df-0243c7ed11b7"
        builder.Configuration.AddUserSecrets("a6c8f6a5-2d49-4b1b-a7df-0243c7ed11b7");

        builder.Services.AddServiceBus(
            serviceName: "TestServiceBus",
            nameOrConnectionString: "AzureWebJobsServiceBus",
            configureBus: entities => entities
                .AddQueue("test-queue")
                .AddTopic("test-topic", "unit-test"),
            configureOptions: options =>
                options.WithNameSuffix(options.Services.GetRequiredService<IHostEnvironment>().EnvironmentName)
        );

        base.ConfigureApplication(builder);
    }
}
