using System.Text;

using Arbiter.Services;

namespace Arbiter.Services.Tests;

public class Base62Tests
{
    [Test]
    public void EncodeToString_EmptyData_ReturnsEmptyString()
    {
        var encoded = Base62.EncodeToString([]);

        encoded.Should().BeEmpty();
    }

    [Test]
    public void DecodeFromChars_EmptyString_ReturnsEmptyArray()
    {
        var decoded = Base62.DecodeFromChars([]);

        decoded.Should().BeEmpty();
    }

    [Test]
    [Arguments(new byte[] { 0x00 }, "00")]
    [Arguments(new byte[] { 0x01 }, "01")]
    [Arguments(new byte[] { 0xFF }, "47")]
    [Arguments(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, "00000000000")]
    public void EncodeToString_KnownValues_ReturnsExpectedString(byte[] data, string expected)
    {
        var encoded = Base62.EncodeToString(data);

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
    [Arguments(11)]
    [Arguments(12)]
    [Arguments(13)]
    [Arguments(14)]
    [Arguments(15)]
    [Arguments(16)]
    [Arguments(64)]
    [Arguments(1023)]
    public void EncodeDecode_RandomData_RoundTrips(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        var encoded = Base62.EncodeToString(data);
        var decoded = Base62.DecodeFromChars(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    [Arguments(1)]
    [Arguments(8)]
    [Arguments(9)]
    [Arguments(64)]
    public void EncodeToUtf8_DecodeFromUtf8_RoundTrips(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        var encoded = Base62.EncodeToUtf8(data);
        var decoded = Base62.DecodeFromUtf8(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    public void EncodeToUtf8_MatchesEncodeToString()
    {
        var data = new byte[32];
        new Random(7).NextBytes(data);

        var encoded = Base62.EncodeToString(data);
        var utf8 = Base62.EncodeToUtf8(data);

        Encoding.UTF8.GetString(utf8).Should().Be(encoded);
    }

    [Test]
    public void EncodeToChars_MatchesEncodeToString()
    {
        var data = new byte[32];
        new Random(7).NextBytes(data);

        var encoded = Base62.EncodeToString(data);
        var chars = Base62.EncodeToChars(data);

        new string(chars).Should().Be(encoded);
    }

    [Test]
    public void EncodeToChars_WithDestination_WritesExpectedCharacters()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var destination = new char[Base62.GetEncodedLength(data.Length)];
        var charsWritten = Base62.EncodeToChars(data, destination);

        charsWritten.Should().Be(destination.Length);
        new string(destination).Should().Be(Base62.EncodeToString(data));
    }

    [Test]
    public void EncodeToChars_DestinationTooSmall_Throws()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var act = () => Base62.EncodeToChars(data, new char[2]);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void TryEncodeToChars_DestinationTooSmall_ReturnsFalse()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var canEncode = Base62.TryEncodeToChars(data, new char[2], out int charsWritten);

        canEncode.Should().BeFalse();
        charsWritten.Should().Be(0);
    }

    [Test]
    public void TryEncodeToUtf8_DestinationTooSmall_ReturnsFalse()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var canEncode = Base62.TryEncodeToUtf8(data, new byte[2], out int bytesWritten);

        canEncode.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Test]
    public void EncodeDecode_MaximumBlockValue_RoundTrips()
    {
        byte[] data = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF];

        var encoded = Base62.EncodeToString(data);
        var decoded = Base62.DecodeFromChars(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    public void EncodeDecode_LeadingZeroBytes_ArePreserved()
    {
        byte[] data = [0x00, 0x00, 0x00, 0x2A];

        var encoded = Base62.EncodeToString(data);
        var decoded = Base62.DecodeFromChars(encoded);

        decoded.Should().Equal(data);
    }

    [Test]
    public void EncodeToString_DifferentLeadingZeroCounts_ProduceDifferentStrings()
    {
        var single = Base62.EncodeToString([0x01]);
        var padded = Base62.EncodeToString([0x00, 0x01]);

        single.Should().NotBe(padded);
    }

    [Test]
    public void EncodeToString_AnyData_UsesOnlyBase62Alphabet()
    {
        var data = new byte[256];
        new Random(42).NextBytes(data);

        var encoded = Base62.EncodeToString(data);

        encoded.Should().MatchRegex("^[0-9A-Za-z]+$");
    }

    [Test]
    [Arguments("0!")]
    [Arguments("0-")]
    [Arguments("0 ")]
    [Arguments("0\u00E9")]
    public void TryDecodeFromChars_InvalidCharacter_ReturnsFalse(string encoded)
    {
        var canDecode = Base62.TryDecodeFromChars(encoded, new byte[8], out int bytesWritten);

        canDecode.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Test]
    public void DecodeFromChars_InvalidCharacter_ThrowsFormatException()
    {
        var act = () => Base62.DecodeFromChars("0!");

        act.Should().Throw<FormatException>();
    }

    [Test]
    [Arguments("0")]
    [Arguments("0000")]
    [Arguments("00000000")]
    [Arguments("000000000000")]
    public void DecodeFromChars_InvalidLength_ThrowsFormatException(string encoded)
    {
        var act = () => Base62.DecodeFromChars(encoded);

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void DecodeFromChars_BlockValueOverflowsUInt64_ThrowsFormatException()
    {
        // the largest 11 character value exceeds ulong.MaxValue
        var act = () => Base62.DecodeFromChars("zzzzzzzzzzz");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void DecodeFromChars_PartialBlockValueOutOfRange_ThrowsFormatException()
    {
        // two characters encode a single byte, but "zz" is 3843
        var act = () => Base62.DecodeFromChars("zz");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void DecodeFromChars_DestinationTooSmall_ThrowsArgumentException()
    {
        var encoded = Base62.EncodeToString("Hello"u8);

        var act = () => Base62.DecodeFromChars(encoded, new byte[2]);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void TryDecodeFromChars_DestinationTooSmall_ReturnsFalse()
    {
        var encoded = Base62.EncodeToString("Hello"u8);

        var canDecode = Base62.TryDecodeFromChars(encoded, new byte[2], out int bytesWritten);

        canDecode.Should().BeFalse();
        bytesWritten.Should().Be(0);
    }

    [Test]
    public void DecodeFromChars_ExactDestination_WritesAllBytes()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var encoded = Base62.EncodeToString(data);

        var destination = new byte[Base62.GetMaxDecodedLength(encoded.Length)];
        var bytesWritten = Base62.DecodeFromChars(encoded, destination);

        bytesWritten.Should().Be(data.Length);
        destination[..bytesWritten].Should().Equal(data);
    }

    [Test]
    public void TryDecodeFromUtf8_ValidInput_ReturnsTrue()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var encoded = Base62.EncodeToUtf8(data);

        var destination = new byte[Base62.GetMaxDecodedLength(encoded.Length)];
        var canDecode = Base62.TryDecodeFromUtf8(encoded, destination, out int bytesWritten);

        canDecode.Should().BeTrue();
        bytesWritten.Should().Be(data.Length);
        destination[..bytesWritten].Should().Equal(data);
    }

    [Test]
    public void IsValid_ValidInput_ReturnsTrueWithDecodedLength()
    {
        byte[] data = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var encoded = Base62.EncodeToString(data);

        var isValid = Base62.IsValid(encoded, out int decodedLength);

        isValid.Should().BeTrue();
        decodedLength.Should().Be(data.Length);
    }

    [Test]
    public void IsValid_EmptyInput_ReturnsTrue()
    {
        Base62.IsValid(ReadOnlySpan<char>.Empty).Should().BeTrue();
    }

    [Test]
    [Arguments("0!")]
    [Arguments("0")]
    [Arguments("zz")]
    [Arguments("zzzzzzzzzzz")]
    public void IsValid_InvalidInput_ReturnsFalse(string encoded)
    {
        var isValid = Base62.IsValid(encoded, out int decodedLength);

        isValid.Should().BeFalse();
        decodedLength.Should().Be(0);
    }

    [Test]
    public void IsValid_Utf8Input_MatchesCharInput()
    {
        byte[] data = [1, 2, 3, 4, 5];
        var encoded = Base62.EncodeToUtf8(data);

        var isValid = Base62.IsValid(encoded, out int decodedLength);

        isValid.Should().BeTrue();
        decodedLength.Should().Be(data.Length);
    }

    [Test]
    [Arguments(0, 0)]
    [Arguments(1, 2)]
    [Arguments(2, 3)]
    [Arguments(3, 5)]
    [Arguments(4, 6)]
    [Arguments(5, 7)]
    [Arguments(6, 9)]
    [Arguments(7, 10)]
    [Arguments(8, 11)]
    [Arguments(9, 13)]
    [Arguments(16, 22)]
    public void GetEncodedLength_ReturnsExpectedLength(int bytesLength, int expected)
    {
        Base62.GetEncodedLength(bytesLength).Should().Be(expected);
    }

    [Test]
    [Arguments(1)]
    [Arguments(7)]
    [Arguments(8)]
    [Arguments(64)]
    public void GetEncodedLength_MatchesActualEncodedLength(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        Base62.EncodeToString(data).Length.Should().Be(Base62.GetEncodedLength(length));
    }

    [Test]
    [Arguments(1)]
    [Arguments(7)]
    [Arguments(8)]
    [Arguments(20)]
    public void GetMaxDecodedLength_MatchesOriginalLength(int length)
    {
        var data = new byte[length];
        new Random(length).NextBytes(data);

        var encoded = Base62.EncodeToString(data);

        Base62.GetMaxDecodedLength(encoded.Length).Should().Be(length);
    }

    [Test]
    public void GetEncodedLength_NegativeCount_Throws()
    {
        var act = () => Base62.GetEncodedLength(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void GetMaxDecodedLength_NegativeCount_Throws()
    {
        var act = () => Base62.GetMaxDecodedLength(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
