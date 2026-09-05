using System.Globalization;
using System.Security.Claims;

using Arbiter.CommandQuery.Extensions;

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

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentCulture;

        public CultureScope(string name)
            => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);

        public void Dispose()
            => CultureInfo.CurrentCulture = _original;
    }
}
