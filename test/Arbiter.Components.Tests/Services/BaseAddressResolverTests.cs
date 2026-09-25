using Arbiter.Components.Options;
using Arbiter.Components.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Arbiter.Components.Tests.Services;

public class BaseAddressResolverTests
{
    [Test]
    public async Task ConstructorThrowsWhenConfigurationIsNull()
    {
        var options = CreateOptions(null);
        var action = () => new BaseAddressResolver(null!, options);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ConstructorThrowsWhenEnvironmentOptionsIsNull()
    {
        var configuration = CreateConfiguration([]);
        var action = () => new BaseAddressResolver(configuration, null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetBaseAddressPrefersNavigationManager()
    {
        var configuration = CreateConfiguration([]);
        var options = CreateOptions("https://options.example.com/");
        var navigation = new TestNavigationManager("https://navigation.example.com/");
        var resolver = new BaseAddressResolver(configuration, options, navigation);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsEqualTo("https://navigation.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesEnvironmentOptionsWhenNavigationManagerIsMissing()
    {
        var configuration = CreateConfiguration([]);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(configuration, options);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsEqualTo("https://options.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesEnvironmentOptionsWhenNavigationManagerIsNotInitialized()
    {
        var configuration = CreateConfiguration([]);
        var options = CreateOptions("https://options.example.com/");
        var navigation = new TestNavigationManager(null);
        var resolver = new BaseAddressResolver(configuration, options, navigation);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsEqualTo("https://options.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesCustomConfigurationKey()
    {
        var values = new Dictionary<string, string?> { ["Api:BaseAddress"] = "https://api.example.com/" };
        var configuration = CreateConfiguration(values);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(configuration, options);

        var address = resolver.GetBaseAddress("Api:BaseAddress");

        await Assert.That(address).IsEqualTo("https://api.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesEnvironmentOptionsWhenKeyIsNull()
    {
        var configuration = CreateConfiguration([]);
        var options = CreateOptions("https://options.example.com/");
        var resolver = new BaseAddressResolver(configuration, options);

        var address = resolver.GetBaseAddress(null);

        await Assert.That(address).IsEqualTo("https://options.example.com/");
    }

    [Test]
    public async Task GetBaseAddressReturnsNullWhenNothingIsConfigured()
    {
        var configuration = CreateConfiguration([]);
        var options = CreateOptions(null);
        var resolver = new BaseAddressResolver(configuration, options);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsNull();
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

    /// <summary>
    /// A <see cref="NavigationManager"/> that is only initialized when a base address is supplied, matching the
    /// behavior of a navigation manager used outside of a rendering context.
    /// </summary>
    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string? baseUri)
        {
            if (baseUri != null)
                Initialize(baseUri, baseUri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
        }
    }
}
