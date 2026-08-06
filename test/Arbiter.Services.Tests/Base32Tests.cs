using System.Text;

using Arbiter.Services;

namespace Arbiter.Services.Tests;

public class Base32Tests
{
    [Test]
    public void EncodeToString_EmptyData_ReturnsEmptyString()
    {
        var encoded = Base32.EncodeToString([]);

        encoded.Should().BeEmpty();
    }

    [Test]
    public void DecodeFromChars_EmptyString_ReturnsEmptyArray()
    {
        var decoded = Base32.DecodeFromChars([]);

        decoded.Should().BeEmpty();
    }

    [Test]
    public void EncodeToString_KnownValue_ReturnsExpectedCrockfordString()
    {
        var encoded = Base32.EncodeToString("Hello"u8);

        encoded.Should().Be("91JPRV3F");
    }

    [Test]
    [Arguments(new byte[] { 0x00 }, "00")]
    [Arguments(new byte[] { 0xFF }, "ZW")]
    [Arguments(new byte[] { 0x00, 0x00 }, "0000")]
    public void EncodeToString_SmallValues_ReturnsExpectedString(byte[] data, string expected)
    {
        var encoded = Base32.EncodeToString(data);

        encoded.Should().Be(expected);
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    [Arguments(5)]
    [Arguments(6)]
    [Arguments(7)]
    [Arguments(8)]
    [Arguments(9)]
    [Arguments(10)]
    [Arguments(64)]
    [Arguments(1024)]
    public void EncodeDecode_RandomData_RoundTrips(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        var encoded = Base32.EncodeToString(data);
        var decoded = Base32.DecodeFromChars(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    [Arguments(1)]
    [Arguments(5)]
    [Arguments(8)]
    [Arguments(64)]
    public void EncodeToUtf8_DecodeFromUtf8_RoundTrips(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        var encoded = Base32.EncodeToUtf8(data);
        var decoded = Base32.DecodeFromUtf8(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    public void EncodeToUtf8_MatchesEncodeToString()
    {
        var data = new byte[32];
        new Random(7).NextBytes(data);

        var encoded = Base32.EncodeToString(data);
        var utf8 = Base32.EncodeToUtf8(data);

        Encoding.UTF8.GetString(utf8).Should().Be(encoded);
    }

    [Test]
    public void EncodeToChars_MatchesEncodeToString()
    {
        var data = new byte[32];
        new Random(7).NextBytes(data);

        var encoded = Base32.EncodeToString(data);
        var chars = Base32.EncodeToChars(data);

        new string(chars).Should().Be(encoded);
    }

    [Test]
    public void EncodeToChars_WithDestination_WritesExpectedCharacters()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var destination = new char[Base32.GetEncodedLength(data.Length)];
        var charsWritten = Base32.EncodeToChars(data, destination);

        charsWritten.Should().Be(destination.Length);
        new string(destination).Should().Be(Base32.EncodeToString(data));
    }

    [Test]
    public void EncodeToChars_DestinationTooSmall_Throws()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var act = () => Base32.EncodeToChars(data, new char[2]);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void TryEncodeToChars_DestinationTooSmall_ReturnsFalse()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var canEncode = Base32.TryEncodeToChars(data, new char[2], out int charsWritten);

        canEncode.Should().BeFalse();
        charsWritten.Should().Be(0);
    }

    [Test]
    public void TryEncodeToUtf8_DestinationTooSmall_ReturnsFalse()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var canEncode = Base32.TryEncodeToUtf8(data, new byte[2], out int bytesWritten);

        canEncode.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Test]
    public void EncodeDecode_LeadingZeroBytes_RoundTrips()
    {
        byte[] data = [0x00, 0x00, 0x00, 0x2A];

        var encoded = Base32.EncodeToString(data);
        var decoded = Base32.DecodeFromChars(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    public void EncodeToString_AnyData_UsesOnlyCrockfordAlphabet()
    {
        var data = new byte[256];
        new Random(42).NextBytes(data);

        var encoded = Base32.EncodeToString(data);

        encoded.Should().MatchRegex("^[0-9A-HJKMNP-TV-Z]+$");
    }

    [Test]
    public void DecodeFromChars_LowercaseInput_DecodesSameAsUppercase()
    {
        var encoded = Base32.EncodeToString("Hello"u8);

        var decoded = Base32.DecodeFromChars(encoded.ToLowerInvariant());

        decoded.Should().Equal("Hello"u8.ToArray());
    }

    [Test]
    [Arguments("I")]
    [Arguments("L")]
    [Arguments("i")]
    [Arguments("l")]
    public void DecodeFromChars_AmbiguousOneCharacters_DecodeAsOne(string character)
    {
        var decoded = Base32.DecodeFromChars(character + "0");

        decoded.Should().Equal(Base32.DecodeFromChars("10"));
    }

    [Test]
    [Arguments("O")]
    [Arguments("o")]
    public void DecodeFromChars_AmbiguousZeroCharacters_DecodeAsZero(string character)
    {
        var decoded = Base32.DecodeFromChars(character + "0");

        decoded.Should().Equal([0x00]);
    }

    [Test]
    [Arguments("9!")]
    [Arguments("9U")]
    [Arguments("9 ")]
    [Arguments("9\u00E9")]
    public void TryDecodeFromChars_InvalidCharacter_ReturnsFalse(string encoded)
    {
        var canDecode = Base32.TryDecodeFromChars(encoded, new byte[8], out int bytesWritten);

        canDecode.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Test]
    public void DecodeFromChars_InvalidCharacter_ThrowsFormatException()
    {
        var act = () => Base32.DecodeFromChars("9!");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void DecodeFromChars_InvalidLength_ThrowsFormatException()
    {
        // a single character cannot produce a whole byte
        var act = () => Base32.DecodeFromChars("9");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void DecodeFromChars_NonZeroPaddingBits_ThrowsFormatException()
    {
        // "ZZ" leaves non-zero bits after the single decoded byte
        var act = () => Base32.DecodeFromChars("ZZ");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void DecodeFromChars_DestinationTooSmall_ThrowsArgumentException()
    {
        var encoded = Base32.EncodeToString("Hello"u8);

        var act = () => Base32.DecodeFromChars(encoded, new byte[2]);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void TryDecodeFromChars_DestinationTooSmall_ReturnsFalse()
    {
        var encoded = Base32.EncodeToString("Hello"u8);

        var canDecode = Base32.TryDecodeFromChars(encoded, new byte[2], out int bytesWritten);

        canDecode.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Test]
    public void DecodeFromChars_ExactDestination_WritesAllBytes()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7];
        var encoded = Base32.EncodeToString(data);

        var destination = new byte[Base32.GetMaxDecodedLength(encoded.Length)];
        var bytesWritten = Base32.DecodeFromChars(encoded, destination);

        bytesWritten.Should().Be(data.Length);
        destination[..bytesWritten].Should().Equal(data);
    }

    [Test]
    public void TryDecodeFromUtf8_ValidInput_ReturnsTrue()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7];
        var encoded = Base32.EncodeToUtf8(data);

        var destination = new byte[Base32.GetMaxDecodedLength(encoded.Length)];
        var canDecode = Base32.TryDecodeFromUtf8(encoded, destination, out int bytesWritten);

        canDecode.Should().BeTrue();
        bytesWritten.Should().Be(data.Length);
        destination[..bytesWritten].Should().Equal(data);
    }

    [Test]
    public void IsValid_ValidInput_ReturnsTrueWithDecodedLength()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7];
        var encoded = Base32.EncodeToString(data);

        var isValid = Base32.IsValid(encoded, out int decodedLength);

        isValid.Should().BeTrue();
        decodedLength.Should().Be(data.Length);
    }

    [Test]
    public void IsValid_EmptyInput_ReturnsTrue()
    {
        Base32.IsValid(ReadOnlySpan<char>.Empty).Should().BeTrue();
    }

    [Test]
    [Arguments("9!")]
    [Arguments("9")]
    [Arguments("ZZ")]
    public void IsValid_InvalidInput_ReturnsFalse(string encoded)
    {
        var isValid = Base32.IsValid(encoded, out int decodedLength);

        isValid.Should().BeFalse();
        decodedLength.Should().Be(0);
    }

    [Test]
    public void IsValid_Utf8Input_MatchesCharInput()
    {
        byte[] data = [1, 2, 3, 4, 5];
        var encoded = Base32.EncodeToUtf8(data);

        var isValid = Base32.IsValid(encoded, out int decodedLength);

        isValid.Should().BeTrue();
        decodedLength.Should().Be(data.Length);
    }

    [Test]
    [Arguments(0, 0)]
    [Arguments(1, 2)]
    [Arguments(2, 4)]
    [Arguments(5, 8)]
    [Arguments(10, 16)]
    public void GetEncodedLength_ReturnsExpectedLength(int bytesLength, int expected)
    {
        Base32.GetEncodedLength(bytesLength).Should().Be(expected);
    }

    [Test]
    [Arguments(1)]
    [Arguments(7)]
    [Arguments(64)]
    public void GetEncodedLength_MatchesActualEncodedLength(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        Base32.EncodeToString(data).Length.Should().Be(Base32.GetEncodedLength(length));
    }

    [Test]
    public void GetEncodedLength_NegativeCount_Throws()
    {
        var act = () => Base32.GetEncodedLength(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetMaxDecodedLength_NegativeCount_Throws()
    {
        var act = () => Base32.GetMaxDecodedLength(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
