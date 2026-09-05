using System.Security.Claims;

using Arbiter.CommandQuery.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.CommandQuery.Tests.Services;

public class TenantResolverTests
{
    private static TenantResolver<TKey> CreateResolver<TKey>()
        where TKey : IParsable<TKey>
        => new(new PrincipalReader(NullLogger<PrincipalReader>.Instance));

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "Identity.Application", ClaimTypes.Name, ClaimTypes.Role));

    [Test]
    public async Task GetTenantIdWhenIntReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "42"));

        var result = await CreateResolver<int>().GetTenantId(principal);

        result.Should().Be(42);
    }

    [Test]
    public async Task GetTenantIdWhenLongReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "9223372036854775807"));

        var result = await CreateResolver<long>().GetTenantId(principal);

        result.Should().Be(long.MaxValue);
    }

    [Test]
    public async Task GetTenantIdWhenGuidReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, expected.ToString()));

        var result = await CreateResolver<Guid>().GetTenantId(principal);

        result.Should().Be(expected);
    }

    [Test]
    public async Task GetTenantIdWhenStringReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "tenant-1"));

        var result = await CreateResolver<string>().GetTenantId(principal);

        result.Should().Be("tenant-1");
    }

    [Test]
    public async Task GetTenantIdWhenNullPrincipalReturnsDefault()
    {
        var result = await CreateResolver<int>().GetTenantId(null);

        result.Should().Be(0);
    }

    [Test]
    public async Task GetTenantIdWhenClaimMissingReturnsDefault()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = await CreateResolver<Guid>().GetTenantId(principal);

        result.Should().Be(Guid.Empty);
    }

    [Test]
    public async Task GetTenantIdWhenNotParsableReturnsDefault()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "not-a-number"));

        var result = await CreateResolver<int>().GetTenantId(principal);

        result.Should().Be(0);
    }

    [Test]
    public void ConstructorWhenNullReaderThrows()
    {
        var act = () => new TenantResolver<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
