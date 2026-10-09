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

    [Test]
    [Arguments("4111111111111111", 4, 4, '*', null, "4111********1111")]
    [Arguments("123-45-6789", 0, 4, '*', 5, "*****6789")]
    [Arguments("P@ssw0rd123", 0, 0, '*', 8, "********")]
    [Arguments("abcdef", 0, 0, '*', null, "******")]
    [Arguments("abcdef", 2, 0, '#', null, "ab####")]
    [Arguments("abcdef", 0, 2, 'X', null, "XXXXef")]
    [Arguments("abcdef", 3, 3, '*', null, "abcdef")]
    [Arguments("abcdef", 1, 1, '*', 0, "af")]
    [Arguments("abcdef", 1, 1, '*', -3, "af")]
    [Arguments("abc", 1, 1, '*', 10, "a**********c")]
    [Arguments("abcdef", 0, 0, '\0', null, "")]
    public void MaskTests(string input, int unmaskedStart, int unmaskedEnd, char maskChar, int? maskedCount, string expected)
    {
        var result = input.Mask(unmaskedStart, unmaskedEnd, maskChar, maskedCount);
        result.Should().Be(expected);
    }

    [Test]
    public void MaskNullReturnsNull()
    {
        string? input = null;

        var result = input.Mask(2, 2);

        result.Should().BeNull();
    }

    [Test]
    public void MaskEmptyReturnsEmpty()
    {
        var result = string.Empty.Mask(0, 0);

        result.Should().BeEmpty();
    }

    [Test]
    public void MaskUnmaskedExceedsLengthThrows()
    {
        var act = () => "abc".Mask(2, 2);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void MaskLongInputUsesPooledBuffer()
    {
        var input = new string('a', 300) + "1234";

        var result = input.Mask(0, 4);

        var expected = new string('*', 300) + "1234";
        result.Should().Be(expected);
    }
}
