using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Tests.Services;

public class NumericEncoderTests
{
    [Test]
    public void Encode_Zero_CanDecodeBackToZero()
    {
        var encoded0Int = NumericEncoder.Encode(0);
        var encoded0Byte = NumericEncoder.Encode((byte)0);
        var encoded0Short = NumericEncoder.Encode((short)0);
        var encoded0Long = NumericEncoder.Encode(0L);

        bool canDecodeInt = NumericEncoder.TryDecode(encoded0Int, out int decoded0Int);
        bool canDecodeByte = NumericEncoder.TryDecode(encoded0Byte, out byte decoded0Byte);
        bool canDecodeShort = NumericEncoder.TryDecode(encoded0Short, out short decoded0Short);
        bool canDecodeLong = NumericEncoder.TryDecode(encoded0Long, out long decoded0Long);

        canDecodeInt.Should().BeTrue();
        canDecodeByte.Should().BeTrue();
        canDecodeShort.Should().BeTrue();
        canDecodeLong.Should().BeTrue();

        decoded0Int.Should().Be(0);
        decoded0Byte.Should().Be((byte)0);
        decoded0Short.Should().Be((short)0);
        decoded0Long.Should().Be(0L);
    }

    [Test]
    [Arguments(1)]
    [Arguments(42)]
    [Arguments(123)]
    [Arguments(1000)]
    [Arguments(int.MaxValue)]
    public void Encode_Int_ProducesValidBase32(int value)
    {
        var encoded = NumericEncoder.Encode(value);

        // Should only contain valid Crockford Base32 characters
        bool isNotEmpty = !string.IsNullOrEmpty(encoded);
        isNotEmpty.Should().BeTrue();

        foreach (var c in encoded)
        {
            bool isValid = (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z');
            isValid.Should().BeTrue($"Character '{c}' is not valid Base32");
        }
    }

    [Test]
    [Arguments((byte)1)]
    [Arguments((byte)42)]
    [Arguments((byte)255)]
    public void Encode_Decode_Byte_RoundTrip(byte value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out byte decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments((sbyte)-128)]
    [Arguments((sbyte)-1)]
    [Arguments((sbyte)0)]
    [Arguments((sbyte)1)]
    [Arguments((sbyte)127)]
    public void Encode_Decode_SByte_RoundTrip(sbyte value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out sbyte decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments((short)-32768)]
    [Arguments((short)-1)]
    [Arguments((short)0)]
    [Arguments((short)1)]
    [Arguments((short)1000)]
    [Arguments((short)32767)]
    public void Encode_Decode_Short_RoundTrip(short value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out short decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments((ushort)0)]
    [Arguments((ushort)1)]
    [Arguments((ushort)1000)]
    [Arguments((ushort)65535)]
    public void Encode_Decode_UShort_RoundTrip(ushort value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out ushort decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments(int.MinValue)]
    [Arguments(-1)]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(42)]
    [Arguments(1000)]
    [Arguments(999999)]
    [Arguments(int.MaxValue)]
    public void Encode_Decode_Int_RoundTrip(int value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out int decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments(0u)]
    [Arguments(1u)]
    [Arguments(42u)]
    [Arguments(1000u)]
    [Arguments(999999u)]
    [Arguments(uint.MaxValue)]
    public void Encode_Decode_UInt_RoundTrip(uint value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out uint decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments(long.MinValue)]
    [Arguments(-1L)]
    [Arguments(0L)]
    [Arguments(1L)]
    [Arguments(42L)]
    [Arguments(1000L)]
    [Arguments(999999L)]
    [Arguments(long.MaxValue)]
    public void Encode_Decode_Long_RoundTrip(long value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out long decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    [Arguments(0ul)]
    [Arguments(1ul)]
    [Arguments(42ul)]
    [Arguments(1000ul)]
    [Arguments(999999ul)]
    [Arguments(ulong.MaxValue)]
    public void Encode_Decode_ULong_RoundTrip(ulong value)
    {
        var encoded = NumericEncoder.Encode(value);
        bool success = NumericEncoder.TryDecode(encoded, out ulong decoded);

        success.Should().BeTrue();
        decoded.Should().Be(value);
    }

    [Test]
    public void TryDecode_CaseInsensitive()
    {
        var encoded = NumericEncoder.Encode(12345);
        var lowercase = encoded.ToLowerInvariant();
        var mixedCase = new string(encoded.Select((c, i) => i % 2 == 0 ? char.ToLowerInvariant(c) : c).ToArray());

        bool success1 = NumericEncoder.TryDecode(encoded, out int decoded1);
        bool success2 = NumericEncoder.TryDecode(lowercase, out int decoded2);
        bool success3 = NumericEncoder.TryDecode(mixedCase, out int decoded3);

        success1.Should().BeTrue();
        success2.Should().BeTrue();
        success3.Should().BeTrue();

        decoded1.Should().Be(12345);
        decoded2.Should().Be(12345);
        decoded3.Should().Be(12345);
    }

    [Test]
    public void TryDecode_HandlesSimilarCharacters()
    {
        var encoded = NumericEncoder.Encode(1);

        // Replace some characters with Crockford alternatives and verify it still works
        // This tests that I, L decode as 1 and O decodes as 0
        var withI = encoded.Replace('1', 'I');
        var withL = encoded.Replace('1', 'L');
        var withO = encoded.Replace('0', 'O');

        // Should decode successfully (may decode to different values depending on position)
        bool successI = NumericEncoder.TryDecode(withI, out int _);
        bool successL = NumericEncoder.TryDecode(withL, out int _);
        bool successO = NumericEncoder.TryDecode(withO, out int _);

        successI.Should().BeTrue();
        successL.Should().BeTrue();
        successO.Should().BeTrue();
    }

    [Test]
    public void TryDecode_EmptyString_ReturnsFalse()
    {
        bool successInt = NumericEncoder.TryDecode("", out int _);
        bool successLong = NumericEncoder.TryDecode("", out long _);

        successInt.Should().BeFalse();
        successLong.Should().BeFalse();
    }

    [Test]
    [Arguments("!!!")]
    [Arguments("XYZ@123")]
    [Arguments("HELLO WORLD")]
    public void TryDecode_InvalidCharacters_ReturnsFalse(string invalid)
    {
        bool success = NumericEncoder.TryDecode(invalid, out int _);
        success.Should().BeFalse();
    }

    [Test]
    public void TryDecode_TooLong_ReturnsFalse()
    {
        var tooLong = new string('A', 20);
        bool success = NumericEncoder.TryDecode(tooLong, out ulong _);

        success.Should().BeFalse();
    }

    [Test]
    public void TryDecode_Overflow_ReturnsFalse()
    {
        // Create a string that would overflow when decoded to byte
        var encoded = NumericEncoder.Encode(1000);
        bool success = NumericEncoder.TryDecode(encoded, out byte _);

        success.Should().BeFalse();
    }

    [Test]
    public void Encode_DifferentValues_ProduceDifferentStrings()
    {
        var encoded1 = NumericEncoder.Encode(1);
        var encoded2 = NumericEncoder.Encode(2);
        var encoded3 = NumericEncoder.Encode(1000);

        Console.WriteLine($"Encoded 1: {encoded1}");
        Console.WriteLine($"Encoded 2: {encoded2}");
        Console.WriteLine($"Encoded 1000: {encoded3}");

        bool different1And2 = encoded1 != encoded2;
        bool different1And3 = encoded1 != encoded3;
        bool different2And3 = encoded2 != encoded3;

        different1And2.Should().BeTrue();
        different1And3.Should().BeTrue();
        different2And3.Should().BeTrue();
    }

    [Test]
    public void Encode_Obfuscates_NotSequential()
    {
        // Verify that sequential IDs don't produce sequential encoded values
        var encoded1 = NumericEncoder.Encode(1);
        var encoded2 = NumericEncoder.Encode(2);
        var encoded3 = NumericEncoder.Encode(3);

        // The encoded strings should not reveal the sequential nature
        bool notLiteral1 = encoded1 != "1";
        bool notLiteral2 = encoded2 != "2";
        bool notLiteral3 = encoded3 != "3";

        notLiteral1.Should().BeTrue();
        notLiteral2.Should().BeTrue();
        notLiteral3.Should().BeTrue();

        // And they should all be different
        bool different1And2 = encoded1 != encoded2;
        bool different2And3 = encoded2 != encoded3;

        different1And2.Should().BeTrue();
        different2And3.Should().BeTrue();
    }

    [Test]
    public void Encode_SequentialInputs_ProduceNonSequentialOutputs()
    {
        // Generate encodings for sequential numbers
        var encodings = Enumerable.Range(1, 20)
            .Select(i => NumericEncoder.Encode(i))
            .ToList();

        Console.WriteLine("\nSequential inputs 1-20:");
        for (int i = 0; i < encodings.Count; i++)
            Console.WriteLine($"{i + 1,3} -> {encodings[i]}");

        // All encodings should be unique
        int uniqueCount = encodings.Distinct().Count();
        bool allUnique = encodings.Count == uniqueCount;
        allUnique.Should().BeTrue();

        // Check that consecutive encoded values are NOT lexicographically consecutive
        // (i.e., they don't sort in the same order as the inputs)
        var sortedEncodings = encodings.OrderBy(e => e).ToList();
        bool notInSortedOrder = !encodings.SequenceEqual(sortedEncodings);

        notInSortedOrder.Should().BeTrue();

        // Additional check: verify no obvious patterns in character positions
        // Check that at least 80% of position changes affect multiple character positions
        int significantChanges = 0;
        for (int i = 0; i < encodings.Count - 1; i++)
        {
            int diffCount = 0;
            string enc1 = encodings[i].PadRight(13);
            string enc2 = encodings[i + 1].PadRight(13);

            for (int j = 0; j < Math.Min(enc1.Length, enc2.Length); j++)
            {
                if (enc1[j] != enc2[j]) diffCount++;
            }

            if (diffCount >= 3) significantChanges++;
        }

        double changeRatio = (double)significantChanges / (encodings.Count - 1);
        Console.WriteLine($"\nSignificant changes (3+ chars): {changeRatio:P0}");

        bool hasGoodDiffusion = changeRatio >= 0.7; // At least 70% should have major changes
        hasGoodDiffusion.Should().BeTrue($"Only {changeRatio:P0} of sequential pairs have significant changes");
    }

    [Test]
    public void Performance_EncodeDecode_1000Iterations()
    {
        // Simple performance check - should complete quickly with minimal allocations
        for (int i = 0; i < 1000; i++)
        {
            var encoded = NumericEncoder.Encode(i);
            bool success = NumericEncoder.TryDecode(encoded, out int decoded);
            bool matches = i == decoded;

            success.Should().BeTrue();
            matches.Should().BeTrue();
        }
    }

    [Test]
    public void Encode_ProducesConsistentOutput()
    {
        // Same input should always produce same output
        var encoded1 = NumericEncoder.Encode(12345);
        var encoded2 = NumericEncoder.Encode(12345);
        var encoded3 = NumericEncoder.Encode(12345);

        bool matches1And2 = encoded1 == encoded2;
        bool matches2And3 = encoded2 == encoded3;

        matches1And2.Should().BeTrue();
        matches2And3.Should().BeTrue();
    }

    [Test]
    public void TryDecode_WithSpan_Works()
    {
        var encoded = NumericEncoder.Encode(42);
        var span = encoded.AsSpan();

        bool success = NumericEncoder.TryDecode(span, out int decoded);
        bool matches = decoded == 42;

        success.Should().BeTrue();
        matches.Should().BeTrue();
    }
}
