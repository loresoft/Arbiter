using Arbiter.Services.Extensions;

namespace Arbiter.Services.Tests.Extensions;

public class EnumerableExtensionsTests
{
    [Test]
    public void ToDelimitedString_Strings_UsesCommaByDefault()
    {
        string?[] values = ["a", "b", "c"];

        var result = values.ToDelimitedString();

        result.Should().Be("a,b,c");
    }

    [Test]
    public void ToDelimitedString_Strings_UsesCustomDelimiter()
    {
        string?[] values = ["a", "b", "c"];

        var result = values.ToDelimitedString(" | ");

        result.Should().Be("a | b | c");
    }

    [Test]
    public void ToDelimitedString_NullDelimiter_UsesComma()
    {
        string?[] values = ["a", "b"];

        var result = values.ToDelimitedString(null);

        result.Should().Be("a,b");
    }

    [Test]
    public void ToDelimitedString_NullValues_RendersEmpty()
    {
        string?[] values = ["a", null, "c"];

        var result = values.ToDelimitedString();

        result.Should().Be("a,,c");
    }

    [Test]
    public void ToDelimitedString_Values_FormatsEachValue()
    {
        int?[] values = [1, 2, 3];

        var result = values.ToDelimitedString(";");

        result.Should().Be("1;2;3");
    }

    [Test]
    public void ToDelimitedString_Empty_ReturnsEmptyString()
    {
        var values = Array.Empty<string?>();

        var result = values.ToDelimitedString();

        result.Should().BeEmpty();
    }
}
