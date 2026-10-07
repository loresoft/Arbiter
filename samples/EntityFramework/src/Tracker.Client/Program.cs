using Arbiter.CommandQuery;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Tracker.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .AddAuthenticationStateDeserialization();

        builder.Services
            .AddTrackerShared()
            .AddTrackerClient("WebAssembly");

        builder.Services.AddEnvironmentOptions(options =>
        {
            options.BaseAddress = builder.HostEnvironment.BaseAddress;
            options.EnvironmentName = builder.HostEnvironment.Environment;
        });

        await builder.Build().RunAsync();
    }
}
