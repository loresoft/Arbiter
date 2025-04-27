using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Arbiter.CommandQuery.Endpoints;

namespace Tracker.Web.Endpoints;

[RegisterTransient<IFeatureEndpoint>(Duplicate = DuplicateStrategy.Append)]
public class AuthenticationEndpoint : IFeatureEndpoint
{
    private readonly Options.AuthenticationOptions _options;

    public AuthenticationEndpoint(IOptions<Options.AuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup(_options.RoutePrefix);

        group
            .MapGet(_options.SignInRoute, SignIn)
            .WithTags("Authentication")
            .WithSummary("Initiate authentication challenge")
            .WithDescription("Initiate authentication challenge")
            .AllowAnonymous();

        group
            .MapGet(_options.SignOutRoute, SignOut)
            .WithTags("Authentication")
            .WithSummary("Initiate sign-out operation")
            .WithDescription("Initiate sign-out operation")
            .AllowAnonymous();
    }

    private ChallengeHttpResult SignIn([FromQuery] string? returnUrl)
    {
        var properties = GetAuthProperties(returnUrl);
        return TypedResults.Challenge(properties, [OpenIdConnectDefaults.AuthenticationScheme]);
    }

    private SignOutHttpResult SignOut([FromQuery] string? returnUrl)
    {
        var properties = GetAuthProperties(returnUrl);
        return TypedResults.SignOut(properties, [OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme]);
    }

    private AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        const string pathBase = "/";

        if (string.IsNullOrEmpty(returnUrl))
            returnUrl = pathBase;
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        else if (returnUrl[0] != '/')
            returnUrl = $"{pathBase}{returnUrl}";

        return new AuthenticationProperties { RedirectUri = returnUrl, };
    }
}
