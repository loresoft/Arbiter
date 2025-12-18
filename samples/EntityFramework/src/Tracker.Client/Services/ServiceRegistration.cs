using System.Text.Json;

using Arbiter.Dispatcher;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.Options;

using Tracker.Extensions;
using Tracker.Options;

namespace Tracker.Client.Services;

public static class ServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services, ISet<string> tags)
    {
        // component libraries
        services
            .AddBlazorControls();

        // json options
        services
            .AddSingleton(sp =>
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                options.AddDomainOptions();

                return options;
            });

        if (tags.Contains("WebAssembly"))
        {
            services
                .AddRemoteDispatcher(static sp =>
                {
                    var hostEnvironment = sp.GetRequiredService<IOptions<EnvironmentOptions>>();
                    return hostEnvironment.Value.BaseAddress;
                });
        }

        if (tags.Contains("Server"))
            services.AddServerDispatcher();
    }
}
