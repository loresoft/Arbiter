using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tracker.Options;

public class EnvironmentOptions
{
    public const string ConfigurationName = "ServerEnvironment";

    public string EnvironmentName { get; set; } = "Development";

    public string BaseAddress { get; set; } = "https://localhost:7020/";

    public TimeSpan UserCacheTime { get; set; } = TimeSpan.FromMinutes(5);

    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services
            .AddOptions<EnvironmentOptions>()
            .Configure<IConfiguration>((settings, configuration) => configuration
                .GetSection(ConfigurationName)
                .Bind(settings)
            );
    }
}
