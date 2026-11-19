using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Endpoints.Tests;

public class BaseAddressResolverTests
{
    [Test]
    public void ConstructorThrowsWhenEnvironmentOptionsIsNull()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var configuration = CreateConfiguration([]);

        // Act
        var action = () => new BaseAddressResolver(accessor, configuration, null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetBaseAddressPrefersHttpContext()
    {
        // Arrange
        var accessor = CreateAccessor("https", "request.example.com", "/app");
        var configuration = CreateConfiguration([]);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(accessor, configuration, options);

        // Act
        var address = resolver.GetBaseAddress();

        // Assert
        address.Should().Be("https://request.example.com/app");
    }

    [Test]
    public void GetBaseAddressUsesEnvironmentOptionsWhenHttpContextIsMissing()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var values = new Dictionary<string, string?> { ["BaseAddress"] = "https://configuration.example.com/" };
        var configuration = CreateConfiguration(values);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(accessor, configuration, options);

        // Act
        var address = resolver.GetBaseAddress();

        // Assert
        address.Should().Be("https://options.example.com/");
    }

    [Test]
    public void GetBaseAddressUsesEnvironmentOptionsWhenKeyIsNull()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var configuration = CreateConfiguration([]);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(accessor, configuration, options);

        // Act
        var address = resolver.GetBaseAddress(null);

        // Assert
        address.Should().Be("https://options.example.com/");
    }

    [Test]
    public void GetBaseAddressUsesCustomConfigurationKey()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var values = new Dictionary<string, string?> { ["Api:BaseAddress"] = "https://api.example.com/" };
        var configuration = CreateConfiguration(values);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(accessor, configuration, options);

        // Act
        var address = resolver.GetBaseAddress("Api:BaseAddress");

        // Assert
        address.Should().Be("https://api.example.com/");
    }

    [Test]
    public void GetBaseAddressReturnsNullWhenNothingIsConfigured()
    {
        // Arrange
        var accessor = new HttpContextAccessor();
        var configuration = CreateConfiguration([]);
        var options = CreateOptions(null);
        var resolver = new BaseAddressResolver(accessor, configuration, options);

        // Act
        var address = resolver.GetBaseAddress();

        // Assert
        address.Should().BeNull();
    }

    [Test]
    public void AddEndpointRoutesResolvesBaseAddressFromEnvironmentOptions()
    {
        // Arrange
        var values = new Dictionary<string, string?> { ["BaseAddress"] = "https://configuration.example.com/" };
        var configuration = CreateConfiguration(values);

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddEndpointRoutes();
        services.PostConfigure<EnvironmentOptions>(options => options.BaseAddress = "https://code.example.com/");

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        // Act
        var resolver = scope.ServiceProvider.GetRequiredService<IBaseAddressResolver>();
        var address = resolver.GetBaseAddress();

        // Assert
        resolver.Should().BeOfType<BaseAddressResolver>();
        address.Should().Be("https://code.example.com/");
    }

    private static HttpContextAccessor CreateAccessor(string scheme, string host, string pathBase)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = scheme;
        httpContext.Request.Host = new HostString(host);
        httpContext.Request.PathBase = new PathString(pathBase);

        return new HttpContextAccessor { HttpContext = httpContext };
    }

    private static IOptions<EnvironmentOptions> CreateOptions(string? baseAddress)
    {
        var environmentOptions = new EnvironmentOptions { BaseAddress = baseAddress };
        return Microsoft.Extensions.Options.Options.Create(environmentOptions);
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
