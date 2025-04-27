using Microsoft.Extensions.DependencyInjection;

namespace Tracker.Options;

public class AuthenticationOptions
{
    private const char separator = '/';

    public string RoutePrefix { get; set; } = "/authentication";

    public string SignInRoute { get; set; } = "/signin";

    public string SignOutRoute { get; set; } = "/signout";

    public string ReturnDefault { get; set; } = "/";

    public string[] PreventRedirectRoutes { get; set; } = ["/api"];

    public TimeSpan UserCacheTime { get; set; } = TimeSpan.FromMinutes(20);

    public string SignInUrl(string? returnUrl = null)
        => BuildReturn(RoutePrefix, SignInRoute, returnUrl ?? "/");

    public string SignOutUrl(string? returnUrl = null)
        => BuildReturn(RoutePrefix, SignOutRoute, returnUrl ?? "/");

    private static string BuildReturn(string prefix, string route, string returnUrl)
    {
        var prefixSpan = prefix.AsSpan().Trim(separator);
        var routeSpan = route.AsSpan().Trim(separator);
        var returnSpan = Uri.EscapeDataString(returnUrl);

        return $"/{prefixSpan}/{routeSpan}?returnUrl={returnSpan}";
    }


    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddOptions<AuthenticationOptions>();
    }
}
