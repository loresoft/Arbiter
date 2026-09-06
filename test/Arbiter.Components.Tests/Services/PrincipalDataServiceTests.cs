using System.Security.Claims;

using Arbiter.Components.Services;

using Microsoft.AspNetCore.Components.Authorization;

namespace Arbiter.Components.Tests.Services;

public class PrincipalDataServiceTests
{
    [Test]
    public async Task ConstructorThrowsWhenNoAuthenticationSourceIsSupplied()
    {
        var dispatcher = new FakeDispatcher();

        var action = () => new PrincipalDataService(dispatcher);

        await Assert.That(action).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task GetUserPrefersTheAuthenticationState()
    {
        var dispatcher = new FakeDispatcher();
        var cascadedUser = CreatePrincipal("cascaded");
        var providerUser = CreatePrincipal("provider");
        var provider = new FakeAuthenticationStateProvider(providerUser);
        var state = Task.FromResult(new AuthenticationState(cascadedUser));
        var dataService = new PrincipalDataService(dispatcher, provider, state);

        var user = await dataService.GetUser();

        await Assert.That(user!.Identity!.Name).IsEqualTo("cascaded");
    }

    [Test]
    public async Task GetUserUsesTheProviderWhenNoStateIsRegistered()
    {
        var dispatcher = new FakeDispatcher();
        var providerUser = CreatePrincipal("provider");
        var provider = new FakeAuthenticationStateProvider(providerUser);
        var dataService = new PrincipalDataService(dispatcher, provider);

        var user = await dataService.GetUser();

        await Assert.That(user!.Identity!.Name).IsEqualTo("provider");
    }

    [Test]
    public async Task GetUserReturnsNullWhenTheProviderRefusesTheRequest()
    {
        var dispatcher = new FakeDispatcher();
        var provider = new FakeAuthenticationStateProvider(null, throwWhenRequested: true);
        var dataService = new PrincipalDataService(dispatcher, provider);

        var user = await dataService.GetUser();

        await Assert.That(user).IsNull();
    }

    [Test]
    public async Task GetUserReturnsAnAnonymousPrincipalWhenNobodyIsSignedIn()
    {
        var dispatcher = new FakeDispatcher();
        var provider = new FakeAuthenticationStateProvider(null);
        var dataService = new PrincipalDataService(dispatcher, provider);

        var user = await dataService.GetUser();

        await Assert.That(user!.Identity!.IsAuthenticated).IsFalse();
    }

    private static ClaimsPrincipal CreatePrincipal(string name)
    {
        var claims = new[] { new Claim(ClaimTypes.Name, name) };
        var identity = new ClaimsIdentity(claims, "Test");

        return new ClaimsPrincipal(identity);
    }
}
