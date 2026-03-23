using System.Text.Json;

using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;

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
                .AddMessagePackDispatcher((sp, client) =>
                {
                    var hostEnvironment = sp.GetRequiredService<IOptions<EnvironmentOptions>>();
                    client.BaseAddress = new Uri(hostEnvironment.Value.BaseAddress);
                })
                .AddHttpMessageHandler<ProgressBarHandler>();

        }

        if (tags.Contains("Server"))
            services.AddServerDispatcher();
    }
}
