using System.Security.Claims;

using Arbiter.CommandQuery.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Arbiter.CommandQuery.Tests.Services;

public class PrincipalReaderTests
{
    private static PrincipalReader CreateReader()
        => new(NullLogger<PrincipalReader>.Instance);

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "Identity.Application", ClaimTypes.Name, ClaimTypes.Role));

    [Test]
    public void GetIdentifierWhenIdentityNameReturnsName()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = CreateReader().GetIdentifier(principal);

        result.Should().Be("William Adama");
    }

    [Test]
    public void GetIdentifierWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetIdentifier(null);

        result.Should().BeNull();
    }

    [Test]
    public void GetIdentifierWhenNoNameClaimReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "1234"));

        var result = CreateReader().GetIdentifier(principal);

        result.Should().BeNull();
    }

    [Test]
    public void GetNameWhenNameClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.NameClaim, "William Adama"));

        var result = CreateReader().GetName(principal);

        result.Should().Be("William Adama");
    }

    [Test]
    public void GetNameWhenMultipleClaimsPrefersNameClaim()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.Name, "Identity Name"),
            new Claim(ClaimNames.NameClaim, "Claim Name"),
            new Claim(ClaimNames.Subject, "Subject Name"));

        var result = CreateReader().GetName(principal);

        result.Should().Be("Claim Name");
    }

    [Test]
    public void GetNameWhenOnlySubjectReturnsSubject()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.Subject, "adama"));

        var result = CreateReader().GetName(principal);

        result.Should().Be("adama");
    }

    [Test]
    public void GetNameWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetName(null);

        result.Should().BeNull();
    }

    [Test]
    public void GetEmailWhenEmailClaimTypeReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Email, "william.adama@battlestar.com"));

        var result = CreateReader().GetEmail(principal);

        result.Should().Be("william.adama@battlestar.com");
    }

    [Test]
    public void GetEmailWhenMultipleClaimsPrefersEmailClaimType()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimNames.EmailsClaim, "emails@battlestar.com"),
            new Claim(ClaimNames.EmailClaim, "email@battlestar.com"),
            new Claim(ClaimTypes.Email, "claimtype@battlestar.com"));

        var result = CreateReader().GetEmail(principal);

        result.Should().Be("claimtype@battlestar.com");
    }

    [Test]
    public void GetEmailWhenOnlyEmailsClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.EmailsClaim, "emails@battlestar.com"));

        var result = CreateReader().GetEmail(principal);

        result.Should().Be("emails@battlestar.com");
    }

    [Test]
    public void GetEmailWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetEmail(null);

        result.Should().BeNull();
    }

    [Test]
    public void GetDisplayNameWhenDisplayNameClaimReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimNames.DisplayName, "Admiral Adama"),
            new Claim(ClaimNames.NameClaim, "William Adama"));

        var result = CreateReader().GetDisplayName(principal);

        result.Should().Be("Admiral Adama");
    }

    [Test]
    public void GetDisplayNameWhenOnlyPreferredUserNameReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.PreferredUserName, "wadama"));

        var result = CreateReader().GetDisplayName(principal);

        result.Should().Be("wadama");
    }

    [Test]
    public void GetDisplayNameWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetDisplayName(null);

        result.Should().BeNull();
    }

    [Test]
    public void GetObjectIdWhenIdentifierClaimReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.IdentifierClaim, expected.ToString()));

        var result = CreateReader().GetObjectId(principal);

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenObjectIdentifierClaimReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.ObjectIdentifier, expected.ToString()));

        var result = CreateReader().GetObjectId(principal);

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenNameIdentifierClaimReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, expected.ToString()));

        var result = CreateReader().GetObjectId(principal);

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenNotGuidReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.ObjectIdentifier, "not-a-guid"));

        var result = CreateReader().GetObjectId(principal);

        result.Should().BeNull();
    }

    [Test]
    public void GetObjectIdWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetObjectId(null);

        result.Should().BeNull();
    }

    [Test]
    public void GetUserIdWhenUserIdClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "1234"));

        var result = CreateReader().GetUserId(principal);

        result.Should().Be("1234");
    }

    [Test]
    public void GetUserIdWhenClaimMissingReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = CreateReader().GetUserId(principal);

        result.Should().BeNull();
    }

    [Test]
    public void GetUserIdWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetUserId(null);

        result.Should().BeNull();
    }

    [Test]
    public void GetTenantIdWhenTenantIdClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, "tenant-1"));

        var result = CreateReader().GetTenantId(principal);

        result.Should().Be("tenant-1");
    }

    [Test]
    public void GetTenantIdWhenClaimMissingReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = CreateReader().GetTenantId(principal);

        result.Should().BeNull();
    }

    [Test]
    public void GetTenantIdWhenNullPrincipalReturnsNull()
    {
        var result = CreateReader().GetTenantId(null);

        result.Should().BeNull();
    }
}
