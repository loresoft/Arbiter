using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Components.Tests.Extensions;

public class NavigationManagerExtensionsTests
{
    [Test]
    public async Task NavigateLocalOnlyThrowsWhenTheNavigationManagerIsNull()
    {
        NavigationManager navigation = null!;

        var action = () => navigation.NavigateLocalOnly("/invoices");

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NavigateLocalOnlyThrowsWhenTheUrlIsNull()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();

        var action = () => navigation.NavigateLocalOnly(null!);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task NavigateLocalOnlyNavigatesToARelativeUrl()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();

        navigation.NavigateLocalOnly("invoices/12");

        await Assert.That(navigation.Uri).EndsWith("/invoices/12");
    }

    [Test]
    public async Task NavigateLocalOnlyNavigatesToAnAbsoluteLocalUrl()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var target = $"{navigation.BaseUri}invoices/12";

        navigation.NavigateLocalOnly(target);

        await Assert.That(navigation.Uri).IsEqualTo(target);
    }

    [Test]
    public async Task NavigateLocalOnlyFallsBackToTheBaseUriForAnExternalUrl()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var baseUri = navigation.BaseUri;

        navigation.NavigateLocalOnly("https://external.example.com/invoices");

        await Assert.That(navigation.Uri).IsEqualTo(baseUri);
    }

    [Test]
    public async Task NavigateLocalOnlyFallsBackToTheBaseUriForAMalformedUrl()
    {
        using var context = new BunitContext();
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var baseUri = navigation.BaseUri;

        navigation.NavigateLocalOnly("http://");

        await Assert.That(navigation.Uri).IsEqualTo(baseUri);
    }
}
