using System.Security.Claims;
using System.Text.Json;

using Arbiter.CommandQuery.Converters;

namespace Arbiter.CommandQuery.Tests.Converters;

public class ClaimsPrincipalConverterTests
{
    [Test]
    public void SerializeClaimsPrincipal()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ClaimsPrincipalConverter());

        var principal = MockPrincipal.Default;

        var json = JsonSerializer.Serialize(principal, options);
        json.Should().NotBeNull();

        var result = JsonSerializer.Deserialize<ClaimsPrincipal>(json, options);
        result.Should().NotBeNull();

        result.Identity.Should().NotBeNull();
        result.Identity.AuthenticationType.Should().Be(principal.Identity?.AuthenticationType);
        result.Identity.Name.Should().Be(principal.Identity?.Name);
        result.Claims.Should().HaveCount(principal.Claims.Count());
    }

    [Test]
    public void SerializeClaimsPrincipalDefault()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new ClaimsPrincipalConverter());

        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var json = JsonSerializer.Serialize(principal, options);
        json.Should().NotBeNull();

        var result = JsonSerializer.Deserialize<ClaimsPrincipal>(json, options);
        result.Should().NotBeNull();

        result.Identity.Should().NotBeNull();
        result.Identity.AuthenticationType.Should().Be(principal.Identity?.AuthenticationType);
        result.Identity.Name.Should().Be(principal.Identity?.Name);
        result.Claims.Should().HaveCount(principal.Claims.Count());
    }
}
