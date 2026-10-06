using System.Globalization;
using System.Security.Claims;

using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class ClaimsExtensionsTests
{
    private const string TestType = "test_claim";

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims)
        => new(new ClaimsIdentity(claims, "Identity.Application", ClaimTypes.Name, ClaimTypes.Role));

    private static ClaimsIdentity CreateIdentity(params Claim[] claims)
        => new(claims, "Identity.Application", ClaimTypes.Name, ClaimTypes.Role);

    [Test]
    public void GetValueWhenClaimExistsReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(TestType, "value-1"));

        var result = principal.GetValue(TestType);

        result.Should().Be("value-1");
    }

    [Test]
    public void GetValueWhenMultipleClaimsReturnsFirst()
    {
        var principal = CreatePrincipal(
            new Claim(TestType, "value-1"),
            new Claim(TestType, "value-2"));

        var result = principal.GetValue(TestType);

        result.Should().Be("value-1");
    }

    [Test]
    public void GetValueWhenClaimMissingReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = principal.GetValue(TestType);

        result.Should().BeNull();
    }

    [Test]
    public void GetValueWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetValue(TestType);

        result.Should().BeNull();
    }

    [Test]
    public void GetValueWhenNullTypeThrows()
    {
        var principal = CreatePrincipal();

        var act = () => principal.GetValue(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetValueTypedWhenIntReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(TestType, "1234"));

        var result = principal.GetValue<int>(TestType);

        result.Should().Be(1234);
    }

    [Test]
    public void GetValueTypedWhenGuidReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(TestType, expected.ToString()));

        var result = principal.GetValue<Guid>(TestType);

        result.Should().Be(expected);
    }

    [Test]
    public void GetValueTypedWhenNotParsableReturnsDefault()
    {
        var principal = CreatePrincipal(new Claim(TestType, "not-a-number"));

        var result = principal.GetValue<int>(TestType);

        result.Should().Be(0);
    }

    [Test]
    public void GetValueTypedWhenClaimMissingReturnsDefault()
    {
        var principal = CreatePrincipal();

        var result = principal.GetValue<int>(TestType);

        result.Should().Be(0);
    }

    [Test]
    public void GetValueTypedWhenNullPrincipalReturnsDefault()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetValue<Guid>(TestType);

        result.Should().Be(Guid.Empty);
    }

    [Test]
    public void GetValueTypedUsesInvariantCulture()
    {
        var principal = CreatePrincipal(new Claim(TestType, "1234.5"));

        using var scope = new CultureScope("de-DE");

        var result = principal.GetValue<double>(TestType);

        result.Should().Be(1234.5d);
    }

    [Test]
    public void GetValuesWhenMultipleClaimsReturnsAll()
    {
        var principal = CreatePrincipal(
            new Claim(TestType, "value-1"),
            new Claim(TestType, "value-2"),
            new Claim(ClaimTypes.Name, "William Adama"));

        var result = principal.GetValues(TestType);

        result.Should().BeEquivalentTo(["value-1", "value-2"]);
    }

    [Test]
    public void GetValuesWhenNoMatchReturnsEmpty()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = principal.GetValues(TestType);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetValuesWhenNullPrincipalReturnsEmpty()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetValues(TestType);

        result.Should().BeEmpty();
    }

    [Test]
    public void GetValuesTypedReturnsParsedValues()
    {
        var principal = CreatePrincipal(
            new Claim(TestType, "1"),
            new Claim(TestType, "2"),
            new Claim(TestType, "3"));

        var result = principal.GetValues<int>(TestType);

        result.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Test]
    public void GetValuesTypedSkipsUnparsableValues()
    {
        var principal = CreatePrincipal(
            new Claim(TestType, "1"),
            new Claim(TestType, "not-a-number"),
            new Claim(TestType, "3"));

        var result = principal.GetValues<int>(TestType);

        result.Should().BeEquivalentTo([1, 3]);
    }

    [Test]
    public void GetValuesTypedWhenNullPrincipalReturnsEmpty()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetValues<int>(TestType);

        result.Should().BeEmpty();
    }

    [Test]
    public void AddClaimToPrincipalAddsToIdentity()
    {
        var principal = CreatePrincipal();

        var result = principal.AddClaim(TestType, "value-1");

        result.Should().BeSameAs(principal);
        principal.GetValue(TestType).Should().Be("value-1");
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public void AddClaimWhenNullOrEmptyValueSkipsClaim(string? value)
    {
        var principal = CreatePrincipal();

        principal.AddClaim(TestType, value);

        principal.GetValue(TestType).Should().BeNull();
    }

    [Test]
    public void AddClaimWhenNoClaimsIdentityThrows()
    {
        var principal = new ClaimsPrincipal();

        var act = () => principal.AddClaim(TestType, "value-1");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddClaimWhenNullPrincipalThrows()
    {
        ClaimsPrincipal? principal = null;

        var act = () => principal!.AddClaim(TestType, "value-1");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddClaimTypedWhenIntAddsValue()
    {
        var identity = CreateIdentity();

        identity.AddClaim(TestType, 1234);

        identity.FindFirst(TestType)?.Value.Should().Be("1234");
    }

    [Test]
    public void AddClaimTypedWhenGuidRoundTrips()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal();

        principal.AddClaim(TestType, expected);

        principal.GetValue<Guid>(TestType).Should().Be(expected);
    }

    [Test]
    public void AddClaimTypedUsesInvariantCulture()
    {
        var identity = CreateIdentity();

        using (var scope = new CultureScope("de-DE"))
            identity.AddClaim(TestType, 1234.5d);

        identity.FindFirst(TestType)?.Value.Should().Be("1234.5");
    }

    [Test]
    public void AddClaimTypedWhenDateTimeUsesRoundTripFormat()
    {
        var expected = new DateTime(2024, 3, 14, 15, 9, 26, 535, DateTimeKind.Utc);
        var identity = CreateIdentity();

        identity.AddClaim(TestType, expected);

        identity.FindFirst(TestType)?.Value.Should().Be("2024-03-14T15:09:26.5350000Z");
    }

    [Test]
    public void AddClaimTypedWhenDateTimeOffsetRoundTrips()
    {
        var expected = new DateTimeOffset(2024, 3, 14, 15, 9, 26, TimeSpan.FromHours(-5));
        var principal = CreatePrincipal();

        principal.AddClaim(TestType, expected);

        principal.GetValue<DateTimeOffset>(TestType).Should().Be(expected);
    }

    [Test]
    public void AddClaimTypedWhenNullValueSkipsClaim()
    {
        var identity = CreateIdentity();

        identity.AddClaim<int?>(TestType, null);

        identity.FindFirst(TestType).Should().BeNull();
    }

    [Test]
    public void AddClaimsAddsClaimForEachValue()
    {
        var principal = CreatePrincipal();

        principal.AddClaims(TestType, [1, 2, 3]);

        principal.GetValues<int>(TestType).Should().BeEquivalentTo([1, 2, 3]);
    }

    [Test]
    public void AddClaimsWhenNullValuesSkipsClaims()
    {
        var principal = CreatePrincipal();

        principal.AddClaims<int>(TestType, null);

        principal.GetValues(TestType).Should().BeEmpty();
    }

    [Test]
    public void AddClaimsEnumeratesSequenceOnce()
    {
        var identity = CreateIdentity();
        var enumerations = 0;

        IEnumerable<int> Values()
        {
            enumerations++;
            yield return 1;
            yield return 2;
        }

        identity.AddClaims(TestType, Values());

        enumerations.Should().Be(1);
        identity.FindAll(TestType).Should().HaveCount(2);
    }

    [Test]
    public void ReplaceClaimWhenExistingReplacesValue()
    {
        var principal = CreatePrincipal(new Claim(TestType, "old-value"));

        principal.ReplaceClaim(TestType, "new-value");

        principal.GetValues(TestType).Should().BeEquivalentTo(["new-value"]);
    }

    [Test]
    public void ReplaceClaimWhenMissingAddsValue()
    {
        var principal = CreatePrincipal();

        principal.ReplaceClaim(TestType, "new-value");

        principal.GetValue(TestType).Should().Be("new-value");
    }

    [Test]
    public void ReplaceClaimWhenSameValueLeavesSingleClaim()
    {
        var principal = CreatePrincipal(new Claim(TestType, "value-1"));

        principal.ReplaceClaim(TestType, "value-1");

        principal.GetValues(TestType).Should().BeEquivalentTo(["value-1"]);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public void ReplaceClaimWhenNullOrEmptyValueLeavesExisting(string? value)
    {
        var principal = CreatePrincipal(new Claim(TestType, "old-value"));

        principal.ReplaceClaim(TestType, value);

        principal.GetValue(TestType).Should().Be("old-value");
    }

    [Test]
    public void AddRoleWhenConditionTrueAddsRole()
    {
        var principal = CreatePrincipal();

        principal.AddRole("Administrator");

        principal.IsInRole("Administrator").Should().BeTrue();
    }

    [Test]
    public void AddRoleWhenConditionFalseSkipsRole()
    {
        var principal = CreatePrincipal();

        principal.AddRole("Administrator", false);

        principal.IsInRole("Administrator").Should().BeFalse();
    }

    [Test]
    public void AddRoleWhenFuncConditionTrueAddsRole()
    {
        var identity = CreateIdentity();

        identity.AddRole("Administrator", () => true);

        identity.FindFirst(identity.RoleClaimType)?.Value.Should().Be("Administrator");
    }

    [Test]
    public void AddRoleWhenFuncConditionFalseSkipsRole()
    {
        var identity = CreateIdentity();

        identity.AddRole("Administrator", () => false);

        identity.FindFirst(identity.RoleClaimType).Should().BeNull();
    }

    [Test]
    public void AddRoleWhenNullConditionThrows()
    {
        var principal = CreatePrincipal();

        var act = () => principal.AddRole("Administrator", (Func<bool>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GetEmailWhenEmailClaimTypeReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Email, "william.adama@battlestar.com"));

        var result = principal.GetEmail();

        result.Should().Be("william.adama@battlestar.com");
    }

    [Test]
    public void GetEmailWhenMultipleClaimsPrefersEmailClaimType()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimNames.EmailsClaim, "emails@battlestar.com"),
            new Claim(ClaimNames.EmailClaim, "email@battlestar.com"),
            new Claim(ClaimTypes.Email, "claimtype@battlestar.com"));

        var result = principal.GetEmail();

        result.Should().Be("claimtype@battlestar.com");
    }

    [Test]
    public void GetEmailWhenOnlyEmailClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.EmailClaim, "email@battlestar.com"));

        var result = principal.GetEmail();

        result.Should().Be("email@battlestar.com");
    }

    [Test]
    public void GetEmailWhenOnlyEmailsClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.EmailsClaim, "emails@battlestar.com"));

        var result = principal.GetEmail();

        result.Should().Be("emails@battlestar.com");
    }

    [Test]
    public void GetEmailWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetEmail();

        result.Should().BeNull();
    }

    [Test]
    public void GetIdentifierWhenNameIdentifierClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, "wadama"));

        var result = principal.GetIdentifier();

        result.Should().Be("wadama");
    }

    [Test]
    public void GetIdentifierWhenClaimMissingReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Name, "William Adama"));

        var result = principal.GetIdentifier();

        result.Should().BeNull();
    }

    [Test]
    public void GetIdentifierWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetIdentifier();

        result.Should().BeNull();
    }

    [Test]
    public void GetObjectIdWhenIdentifierClaimReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.IdentifierClaim, expected.ToString()));

        var result = principal.GetObjectId();

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenObjectIdentifierClaimReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.ObjectIdentifier, expected.ToString()));

        var result = principal.GetObjectId();

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenNameIdentifierClaimReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimTypes.NameIdentifier, expected.ToString()));

        var result = principal.GetObjectId();

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenMultipleClaimsPrefersIdentifierClaim()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimNames.ObjectIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimNames.IdentifierClaim, expected.ToString()));

        var result = principal.GetObjectId();

        result.Should().Be(expected);
    }

    [Test]
    public void GetObjectIdWhenNotGuidReturnsNull()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.ObjectIdentifier, "not-a-guid"));

        var result = principal.GetObjectId();

        result.Should().BeNull();
    }

    [Test]
    public void GetObjectIdWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetObjectId();

        result.Should().BeNull();
    }

    [Test]
    public void GetNameWhenNameClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.NameClaim, "William Adama"));

        var result = principal.GetName();

        result.Should().Be("William Adama");
    }

    [Test]
    public void GetNameWhenMultipleClaimsPrefersNameClaim()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.Name, "Identity Name"),
            new Claim(ClaimNames.NameClaim, "Claim Name"),
            new Claim(ClaimNames.Subject, "Subject Name"));

        var result = principal.GetName();

        result.Should().Be("Claim Name");
    }

    [Test]
    public void GetNameWhenOnlySubjectReturnsSubject()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.Subject, "adama"));

        var result = principal.GetName();

        result.Should().Be("adama");
    }

    [Test]
    public void GetNameWhenNoClaimsFallsBackToIdentityName()
    {
        Claim[] claims = [new Claim("custom_name", "Identity Name")];
        var identity = new ClaimsIdentity(claims, "Identity.Application", "custom_name", ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);

        var result = principal.GetName();

        result.Should().Be("Identity Name");
    }

    [Test]
    public void GetNameWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetName();

        result.Should().BeNull();
    }

    [Test]
    public void GetProviderWhenProviderClaimReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimNames.IdentityClaim, "identity-provider"),
            new Claim(ClaimNames.ProviderClaim, "provider"));

        var result = principal.GetProvider();

        result.Should().Be("provider");
    }

    [Test]
    public void GetProviderWhenOnlyIdentityClaimReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.IdentityClaim, "identity-provider"));

        var result = principal.GetProvider();

        result.Should().Be("identity-provider");
    }

    [Test]
    public void GetProviderWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetProvider();

        result.Should().BeNull();
    }

    [Test]
    public void GetUserNameWhenPreferredUserNameReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.Name, "William Adama"),
            new Claim(ClaimNames.PreferredUserName, "wadama"));

        var result = principal.GetUserName();

        result.Should().Be("wadama");
    }

    [Test]
    public void GetUserNameWhenOnlySubjectReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.Subject, "adama"));

        var result = principal.GetUserName();

        result.Should().Be("adama");
    }

    [Test]
    public void GetUserNameWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetUserName();

        result.Should().BeNull();
    }

    [Test]
    public void GetDisplayNameWhenDisplayNameClaimReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimNames.DisplayName, "Admiral Adama"),
            new Claim(ClaimNames.NameClaim, "William Adama"));

        var result = principal.GetDisplayName();

        result.Should().Be("Admiral Adama");
    }

    [Test]
    public void GetDisplayNameWhenOnlySubjectReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.Subject, "adama"));

        var result = principal.GetDisplayName();

        result.Should().Be("adama");
    }

    [Test]
    public void GetDisplayNameWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetDisplayName();

        result.Should().BeNull();
    }

    [Test]
    public void GetGivenNameWhenGivenNameClaimReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.GivenName, "Bill"),
            new Claim(ClaimNames.GivenName, "William"));

        var result = principal.GetGivenName();

        result.Should().Be("William");
    }

    [Test]
    public void GetGivenNameWhenOnlyClaimTypeReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.GivenName, "William"));

        var result = principal.GetGivenName();

        result.Should().Be("William");
    }

    [Test]
    public void GetGivenNameWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetGivenName();

        result.Should().BeNull();
    }

    [Test]
    public void GetFamilyNameWhenFamilyNameClaimReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.Surname, "Other"),
            new Claim(ClaimNames.FamilyName, "Adama"));

        var result = principal.GetFamilyName();

        result.Should().Be("Adama");
    }

    [Test]
    public void GetFamilyNameWhenOnlySurnameReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.Surname, "Adama"));

        var result = principal.GetFamilyName();

        result.Should().Be("Adama");
    }

    [Test]
    public void GetFamilyNameWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetFamilyName();

        result.Should().BeNull();
    }

    [Test]
    public void GetPhoneNumberWhenPhoneNumberClaimReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.MobilePhone, "555-0101"),
            new Claim(ClaimNames.PhoneNumber, "555-0100"));

        var result = principal.GetPhoneNumber();

        result.Should().Be("555-0100");
    }

    [Test]
    public void GetPhoneNumberWhenOnlyMobilePhoneReturnsValue()
    {
        var principal = CreatePrincipal(
            new Claim(ClaimTypes.HomePhone, "555-0102"),
            new Claim(ClaimTypes.MobilePhone, "555-0101"));

        var result = principal.GetPhoneNumber();

        result.Should().Be("555-0101");
    }

    [Test]
    public void GetPhoneNumberWhenOnlyHomePhoneReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimTypes.HomePhone, "555-0102"));

        var result = principal.GetPhoneNumber();

        result.Should().Be("555-0102");
    }

    [Test]
    public void GetPhoneNumberWhenNullPrincipalReturnsNull()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetPhoneNumber();

        result.Should().BeNull();
    }

    [Test]
    public void GetUserIdWhenIntReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "1234"));

        var result = principal.GetUserId<int>();

        result.Should().Be(1234);
    }

    [Test]
    public void GetUserIdWhenStringReturnsValue()
    {
        var principal = CreatePrincipal(new Claim(ClaimNames.UserId, "user-1"));

        var result = principal.GetUserId<string>();

        result.Should().Be("user-1");
    }

    [Test]
    public void GetUserIdWhenClaimMissingReturnsDefault()
    {
        var principal = CreatePrincipal();

        var result = principal.GetUserId<Guid>();

        result.Should().Be(Guid.Empty);
    }

    [Test]
    public void GetTenantIdWhenGuidReturnsValue()
    {
        var expected = Guid.NewGuid();
        var principal = CreatePrincipal(new Claim(ClaimNames.TenantId, expected.ToString()));

        var result = principal.GetTenantId<Guid>();

        result.Should().Be(expected);
    }

    [Test]
    public void GetTenantIdWhenNullPrincipalReturnsDefault()
    {
        ClaimsPrincipal? principal = null;

        var result = principal.GetTenantId<string>();

        result.Should().BeNull();
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentCulture;

        public CultureScope(string name)
            => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);

        public void Dispose()
            => CultureInfo.CurrentCulture = _original;
    }
}
