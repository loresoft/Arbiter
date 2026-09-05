using System.Security.Claims;

using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class PrincipalReaderExtensionsTests
{
    private static PrincipalReader CreateReader()
        => new(NullLogger<PrincipalReader>.Instance);

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "Identity.Application", ClaimTypes.Name, ClaimTypes.Role));

    [Test]
    public void GetUserIdWhenIntReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "1234"));

        var result = CreateReader().GetUserId<int>(principal);

        result.Should().Be(1234);
    }

    [Test]
    public void GetUserIdWhenLongReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "9223372036854775807"));

        var result = CreateReader().GetUserId<long>(principal);

        result.Should().Be(long.MaxValue);
    }

    [Test]
    public void GetUserIdWhenGuidReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, expected.ToString()));

        var result = CreateReader().GetUserId<Guid>(principal);

        result.Should().Be(expected);
    }

    [Test]
    public void GetUserIdWhenStringReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "user-1"));

        var result = CreateReader().GetUserId<string>(principal);

        result.Should().Be("user-1");
    }

    [Test]
    public void GetUserIdWhenClaimMissingReturnsDefault()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = CreateReader().GetUserId<int>(principal);

        result.Should().Be(0);
    }

    [Test]
    public void GetUserIdWhenNotParsableReturnsDefault()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "not-a-number"));

        var result = CreateReader().GetUserId<int>(principal);

        result.Should().Be(0);
    }

    [Test]
    public void GetUserIdWhenNullPrincipalReturnsDefault()
    {
        var result = CreateReader().GetUserId<Guid>(null);

        result.Should().Be(Guid.Empty);
    }

    [Test]
    public void GetUserIdWhenNullReaderThrows()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "1234"));

        var act = () => PrincipalReaderExtensions.GetUserId<int>(null!, principal);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetTenantIdWhenIntReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "42"));

        var result = CreateReader().GetTenantId<int>(principal);

        result.Should().Be(42);
    }

    [Test]
    public void GetTenantIdWhenGuidReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, expected.ToString()));

        var result = CreateReader().GetTenantId<Guid>(principal);

        result.Should().Be(expected);
    }

    [Test]
    public void GetTenantIdWhenStringReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "tenant-1"));

        var result = CreateReader().GetTenantId<string>(principal);

        result.Should().Be("tenant-1");
    }

    [Test]
    public void GetTenantIdWhenClaimMissingReturnsDefault()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = CreateReader().GetTenantId<int>(principal);

        result.Should().Be(0);
    }

    [Test]
    public void GetTenantIdWhenNullReaderThrows()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "42"));

        var act = () => PrincipalReaderExtensions.GetTenantId<int>(null!, principal);

        act.Should().Throw<ArgumentNullException>();
    }
}
