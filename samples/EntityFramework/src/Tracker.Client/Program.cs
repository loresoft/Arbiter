using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Tracker.Options;

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

        builder.Services.Configure<EnvironmentOptions>(options =>
        {
            options.BaseAddress = builder.HostEnvironment.BaseAddress;
            options.EnvironmentName = builder.HostEnvironment.Environment;
        });
        await builder.Build().RunAsync();
    }
}
