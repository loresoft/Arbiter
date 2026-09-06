using Arbiter.Components.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Arbiter.Components.Tests.Services;

public class BaseAddressResolverTests
{
    [Test]
    public async Task ConstructorThrowsWhenConfigurationIsNull()
    {
        var action = () => new BaseAddressResolver(null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetBaseAddressPrefersNavigationManager()
    {
        var values = new Dictionary<string, string?> { [BaseAddressResolver.BaseAddressKey] = "https://config.example.com/" };
        var configuration = CreateConfiguration(values);
        var navigation = new TestNavigationManager("https://navigation.example.com/");
        var resolver = new BaseAddressResolver(configuration, navigation);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsEqualTo("https://navigation.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesConfigurationWhenNavigationManagerIsMissing()
    {
        var values = new Dictionary<string, string?> { [BaseAddressResolver.BaseAddressKey] = "https://config.example.com/" };
        var configuration = CreateConfiguration(values);
        var resolver = new BaseAddressResolver(configuration);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsEqualTo("https://config.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesConfigurationWhenNavigationManagerIsNotInitialized()
    {
        var values = new Dictionary<string, string?> { [BaseAddressResolver.BaseAddressKey] = "https://config.example.com/" };
        var configuration = CreateConfiguration(values);
        var navigation = new TestNavigationManager(null);
        var resolver = new BaseAddressResolver(configuration, navigation);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsEqualTo("https://config.example.com/");
    }

    [Test]
    public async Task GetBaseAddressUsesCustomConfigurationKey()
    {
        var values = new Dictionary<string, string?> { ["Api:BaseAddress"] = "https://api.example.com/" };
        var configuration = CreateConfiguration(values);
        var resolver = new BaseAddressResolver(configuration);

        var address = resolver.GetBaseAddress("Api:BaseAddress");

        await Assert.That(address).IsEqualTo("https://api.example.com/");
    }

    [Test]
    public async Task GetBaseAddressFallsBackToDefaultKeyWhenKeyIsNull()
    {
        var values = new Dictionary<string, string?> { [BaseAddressResolver.BaseAddressKey] = "https://config.example.com/" };
        var configuration = CreateConfiguration(values);
        var resolver = new BaseAddressResolver(configuration);

        var address = resolver.GetBaseAddress(null);

        await Assert.That(address).IsEqualTo("https://config.example.com/");
    }

    [Test]
    public async Task GetBaseAddressReturnsNullWhenNothingIsConfigured()
    {
        var values = new Dictionary<string, string?>();
        var configuration = CreateConfiguration(values);
        var resolver = new BaseAddressResolver(configuration);

        var address = resolver.GetBaseAddress();

        await Assert.That(address).IsNull();
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
