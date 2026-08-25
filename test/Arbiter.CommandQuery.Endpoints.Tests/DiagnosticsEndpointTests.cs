using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints.Tests;

public class DiagnosticsEndpointTests
{
    [Test]
    public void AddRoutesWithDefaultOptionsMapsOnlySafeRoutes()
    {
        // Arrange
        var endpoint = CreateEndpoint(new DiagnosticsEndpointOptions());
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        RoutePatterns(app).Should().BeEquivalentTo("health-check", "claims-check");
    }

    [Test]
    public void AddRoutesWithConfigurationEnabledMapsConfigurationRoute()
    {
        // Arrange
        var endpoint = CreateEndpoint(new DiagnosticsEndpointOptions { ConfigurationEnabled = true });
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        RoutePatterns(app).Should().Contain("config-debugger");
    }

    [Test]
    public void AddRoutesWithCacheClearEnabledMapsCacheClearRoute()
    {
        // Arrange
        var endpoint = CreateEndpoint(new DiagnosticsEndpointOptions { CacheClearEnabled = true });
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        RoutePatterns(app).Should().Contain("cache-clear");
    }

    [Test]
    public void AddRoutesWithAllRoutesDisabledMapsNoRoutes()
    {
        // Arrange
        var options = new DiagnosticsEndpointOptions
        {
            HealthCheckEnabled = false,
            ClaimsCheckEnabled = false,
        };

        var endpoint = CreateEndpoint(options);
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        RoutePatterns(app).Should().BeEmpty();
    }

    [Test]
    public void AddRoutesWithAuthorizationPolicyAppliesPolicyToRoute()
    {
        // Arrange
        var options = new DiagnosticsEndpointOptions { AuthorizationPolicy = "Diagnostics" };

        var endpoint = CreateEndpoint(options);
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        AuthorizationPolicies(app, "health-check").Should().Contain("Diagnostics");
    }

    [Test]
    public void AddRoutesWithEndpointPolicyOverridesAuthorizationPolicy()
    {
        // Arrange
        var options = new DiagnosticsEndpointOptions
        {
            AuthorizationPolicy = "Diagnostics",
            HealthCheckPolicy = "Administrator",
        };

        var endpoint = CreateEndpoint(options);
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        AuthorizationPolicies(app, "health-check").Should().Contain("Administrator");
    }

    [Test]
    public void AddRoutesWithoutAuthorizationPolicyStillRequiresAuthorization()
    {
        // Arrange
        var endpoint = CreateEndpoint(new DiagnosticsEndpointOptions());
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        FindEndpoint(app, "health-check").Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Should().NotBeEmpty();
    }

    [Test]
    public void AddRoutesInvokesConfigureEndpointForEachMappedRoute()
    {
        // Arrange
        var invocations = 0;
        var options = new DiagnosticsEndpointOptions
        {
            ConfigureEndpoint = _ => invocations++,
        };

        var endpoint = CreateEndpoint(options);
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        invocations.Should().Be(2);
    }

    [Test]
    public void AddRoutesInvokesEndpointSpecificConfigureAction()
    {
        // Arrange
        var invocations = 0;
        var options = new DiagnosticsEndpointOptions
        {
            ConfigureHealthCheckEndpoint = _ => invocations++,
        };

        var endpoint = CreateEndpoint(options);
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        invocations.Should().Be(1);
    }

    [Test]
    public void AddRoutesDoesNotInvokeConfigureActionForDisabledRoute()
    {
        // Arrange
        var invocations = 0;
        var options = new DiagnosticsEndpointOptions
        {
            ConfigureCacheClearEndpoint = _ => invocations++,
        };

        var endpoint = CreateEndpoint(options);
        var app = CreateApplication();

        // Act
        endpoint.AddRoutes(app);

        // Assert
        invocations.Should().Be(0);
    }

    private static DiagnosticsEndpoint CreateEndpoint(DiagnosticsEndpointOptions options)
        => new(NullLogger<DiagnosticsEndpoint>.Instance, Options.Create(options));

    private static WebApplication CreateApplication()
        => WebApplication.CreateSlimBuilder().Build();

    private static IEnumerable<Endpoint> Endpoints(IEndpointRouteBuilder builder)
        => builder.DataSources.SelectMany(dataSource => dataSource.Endpoints);

    private static IEnumerable<string> RoutePatterns(IEndpointRouteBuilder builder)
        => Endpoints(builder).OfType<RouteEndpoint>().Select(route => route.RoutePattern.RawText!);

    private static Endpoint FindEndpoint(IEndpointRouteBuilder builder, string pattern)
        => Endpoints(builder)
            .OfType<RouteEndpoint>()
            .Single(route => route.RoutePattern.RawText == pattern);

    private static IEnumerable<string?> AuthorizationPolicies(IEndpointRouteBuilder builder, string pattern)
        => FindEndpoint(builder, pattern).Metadata
            .GetOrderedMetadata<IAuthorizeData>()
            .Select(data => data.Policy);
}
