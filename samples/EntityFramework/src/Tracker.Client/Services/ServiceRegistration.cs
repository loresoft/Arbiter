using System.Text.Json;

using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Dispatcher;

using Blazored.Modal;

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
            .AddBlazoredModal();

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
                .AddHttpClient<RemoteDispatcher>((sp, client) =>
                {
                    var hostEnvironment = sp.GetRequiredService<IOptions<EnvironmentOptions>>();
                    client.BaseAddress = new Uri(hostEnvironment.Value.BaseAddress);
                })
                .AddHttpMessageHandler<ProgressBarHandler>();

            services.AddRemoteDispatcher();
        }

        if (tags.Contains("Server"))
            services.AddServerDispatcher();
    }
}
