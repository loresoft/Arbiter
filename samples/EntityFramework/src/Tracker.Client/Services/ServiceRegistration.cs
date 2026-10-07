using System.Text.Json;

using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Options;
using Arbiter.Components;
using Arbiter.Dispatcher;

using LoreSoft.Blazor.Controls;

using Microsoft.Extensions.Options;

using Tracker.Extensions;

namespace Tracker.Client.Services;

public static class ServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services, ISet<string> tags)
    {
        // component libraries
        services.AddBlazorControls();

        // page component services
        services.AddArbiterComponents();

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
                    client.BaseAddress = hostEnvironment.Value.BaseAddress.ToUri();
                })
                .AddHttpMessageHandler<ProgressBarHandler>();

        }

        if (tags.Contains("Server"))
            services.AddServerDispatcher();
    }
}
