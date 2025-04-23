using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class StringExtensionsTests
{
    [Test]
    public void CombineTests()
    {
        var result = "/".Combine("/api/user");
        result.Should().Be("/api/user");

        result = "/api".Combine("/user");
        result.Should().Be("/api/user");

        result = "/api/".Combine("user");
        result.Should().Be("/api/user");

        result = "/api".Combine("user");
        result.Should().Be("/api/user");
    }
}
