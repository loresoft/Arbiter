using System.Security.Claims;

using Arbiter.CommandQuery.Services;

using Bunit;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.Components.Tests.Abstracts;

public class PrincipalBaseTests
{
    [Test]
    public async Task GetUserReturnsNullWhenTheAuthenticationStateIsMissing()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        var user = await page.PublicGetUser();

        await Assert.That(user).IsNull();
    }

    [Test]
    public async Task GetUserReturnsTheCascadedUser()
    {
        using var context = CreateContext();
        var principal = CreatePrincipal();
        var page = RenderWithState(context, principal);

        var user = await page.PublicGetUser();

        await Assert.That(user!.Identity!.Name).IsEqualTo("tester");
    }

    [Test]
    public async Task GetUserIdReturnsTheDefaultWhenTheAuthenticationStateIsMissing()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        var userId = await page.PublicGetUserId();

        await Assert.That(userId).IsEqualTo(0);
    }

    [Test]
    public async Task GetUserIdReadsTheUserIdClaim()
    {
        using var context = CreateContext();
        var principal = CreatePrincipal();
        var page = RenderWithState(context, principal);

        var userId = await page.PublicGetUserId();

        await Assert.That(userId).IsEqualTo(1234);
    }

    [Test]
    public async Task HasRoleThrowsWhenTheRoleNameIsEmpty()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        var action = () => page.PublicHasRole(" ");

        await Assert.That(action).Throws<ArgumentException>();
    }

    [Test]
    public async Task HasRoleReturnsFalseWhenTheAuthenticationStateIsMissing()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        var hasRole = await page.PublicHasRole("Administrator");

        await Assert.That(hasRole).IsFalse();
    }

    [Test]
    public async Task HasRoleReturnsTrueWhenTheUserIsInTheRole()
    {
        using var context = CreateContext();
        var principal = CreatePrincipal();
        var page = RenderWithState(context, principal);

        var hasRole = await page.PublicHasRole("Administrator");

        await Assert.That(hasRole).IsTrue();
    }

    [Test]
    public async Task HasRoleReturnsFalseWhenTheUserIsNotInTheRole()
    {
        using var context = CreateContext();
        var principal = CreatePrincipal();
        var page = RenderWithState(context, principal);

        var hasRole = await page.PublicHasRole("Auditor");

        await Assert.That(hasRole).IsFalse();
    }

    [Test]
    public async Task HasPolicyThrowsWhenThePolicyNameIsEmpty()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        var action = () => page.PublicHasPolicy(" ");

        await Assert.That(action).Throws<ArgumentException>();
    }

    [Test]
    public async Task HasPolicyReturnsFalseWhenTheAuthenticationStateIsMissing()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        var hasPolicy = await page.PublicHasPolicy(AdministratorPolicy);

        await Assert.That(hasPolicy).IsFalse();
    }

    [Test]
    public async Task HasPolicyReturnsTrueWhenTheUserSatisfiesThePolicy()
    {
        using var context = CreateContext();
        var principal = CreatePrincipal();
        var page = RenderWithState(context, principal);

        var hasPolicy = await page.PublicHasPolicy(AdministratorPolicy);

        await Assert.That(hasPolicy).IsTrue();
    }

    [Test]
    public async Task HasPolicyReturnsFalseWhenTheUserDoesNotSatisfyThePolicy()
    {
        using var context = CreateContext();
        var principal = CreatePrincipal();
        var page = RenderWithState(context, principal);

        var hasPolicy = await page.PublicHasPolicy(AuditorPolicy);

        await Assert.That(hasPolicy).IsFalse();
    }

    [Test]
    public async Task LoggerUsesTheInjectedLoggerFactory()
    {
        using var context = CreateContext();
        var page = context.Render<TestPrincipalPage>().Instance;

        await Assert.That(page.HasLogger).IsTrue();
    }

    private const string AdministratorPolicy = "Administrator";

    private const string AuditorPolicy = "Auditor";

    private static TestPrincipalPage RenderWithState(BunitContext context, ClaimsPrincipal principal)
    {
        var state = Task.FromResult(new AuthenticationState(principal));

        return context.Render<TestPrincipalPage>(parameters => parameters.AddCascadingValue(state)).Instance;
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "tester"),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimNames.UserId, "1234"),
        };
        var identity = new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();

        context.Services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // bunit registers a placeholder IAuthorizationService, so register a real one built from its own container
        context.Services.AddSingleton(CreateAuthorizationService());

        return context;
    }

    private static IAuthorizationService CreateAuthorizationService()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(AdministratorPolicy, policy => policy.RequireRole("Administrator"));
            options.AddPolicy(AuditorPolicy, policy => policy.RequireRole("Auditor"));
        });

        var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IAuthorizationService>();
    }
}
