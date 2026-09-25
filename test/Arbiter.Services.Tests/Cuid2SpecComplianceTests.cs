using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Arbiter.Services;

namespace Arbiter.Services.Tests;

/// <summary>
/// Compatibility tests against the Cuid2 specification: https://github.com/paralleldrive/cuid2
/// </summary>
public partial class Cuid2SpecComplianceTests
{
    private const string Base36Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

    [GeneratedRegex("^[a-z][0-9a-z]+$")]
    private static partial Regex CuidPattern();

    [Test]
    public void DefaultLength_MatchesSpec()
    {
        Cuid2.DefaultLength.Should().Be(24);
    }

    [Test]
    public void MaxLength_MatchesSpecBigLength()
    {
        Cuid2.MaxLength.Should().Be(32);
    }

    [Test]
    public void GeneratedIds_MatchSpecPattern()
    {
        Cuid2Tests.SkipIfUnsupported();

        var invalid = Enumerable.Range(0, 10_000)
            .Select(_ => Cuid2.NewCuid().ToString())
            .Where(id => !CuidPattern().IsMatch(id))
            .ToList();

        invalid.Should().BeEmpty();
    }

    [Test]
    [Arguments(2)]
    [Arguments(10)]
    [Arguments(32)]
    public void GeneratedIds_WithLength_MatchSpecPattern(int length)
    {
        Cuid2Tests.SkipIfUnsupported();

        var id = Cuid2.NewCuid(length).ToString();

        CuidPattern().IsMatch(id).Should().BeTrue();
    }

    [Test]
    public void GeneratedIds_FirstLetter_CoversAlphabet()
    {
        Cuid2Tests.SkipIfUnsupported();

        var letters = Enumerable.Range(0, 20_000)
            .Select(_ => Cuid2.NewCuid().ToString()[0])
            .ToHashSet();

        letters.Count.Should().Be(26);
    }

    [Test]
    public void GeneratedIds_Characters_AreUniformlyDistributed()
    {
        Cuid2Tests.SkipIfUnsupported();

        const int count = 50_000;
        var histogram = new int[36];
        for (int i = 0; i < count; i++)
        {
            var id = Cuid2.NewCuid().ToString();
            foreach (var c in id.AsSpan(1))
                histogram[Base36Alphabet.IndexOf(c, StringComparison.Ordinal)]++;
        }

        double expected = count * (Cuid2.DefaultLength - 1) / 36.0;
        var outliers = histogram
            .Where(bucket => Math.Abs(bucket - expected) / expected > 0.05)
            .ToList();

        outliers.Should().BeEmpty();
    }

    [Test]
    public void GeneratedIds_NoCollisions()
    {
        Cuid2Tests.SkipIfUnsupported();

        const int count = 1_000_000;
        var ids = new HashSet<Cuid2>(count);
        for (int i = 0; i < count; i++)
            ids.Add(Cuid2.NewCuid());

        ids.Count.Should().Be(count);
    }

    [Test]
    [Arguments("", false)]
    [Arguments("a", false)]
    [Arguments("ab", true)]
    [Arguments("a1", true)]
    [Arguments("tz4a98xxat96iws9zmbrgj3a", true)]
    [Arguments("abcdefghijklmnopqrstuvwxyz012345", true)]
    [Arguments("abcdefghijklmnopqrstuvwxyz0123456", false)]
    [Arguments("1bc", false)]
    [Arguments("Abc", false)]
    [Arguments("aBc", false)]
    [Arguments("ab_c", false)]
    [Arguments("ab c", false)]
    [Arguments("abc\u00e9", false)]
    public void IsValid_MatchesSpecIsCuid(string value, bool expected)
    {
        Cuid2.IsValid(value).Should().Be(expected);
    }

    [Test]
    [Arguments("ab", true)]
    [Arguments("a", false)]
    [Arguments("Ab", false)]
    public void IsValid_Utf8_MatchesSpecIsCuid(string value, bool expected)
    {
        var utf8 = Encoding.UTF8.GetBytes(value);

        Cuid2.IsValid(utf8).Should().Be(expected);
    }

    [Test]
    public void IsValid_Null_ReturnsFalse()
    {
        Cuid2.IsValid((string?)null).Should().BeFalse();
    }

    [Test]
    [Arguments(0L, "0")]
    [Arguments(35L, "z")]
    [Arguments(36L, "10")]
    [Arguments(476782367L, "7vv3mn")]
    [Arguments(long.MaxValue, "1y2p0ij32e8e7")]
    public void WriteBase36_MatchesJavaScriptToString36(long value, string expected)
    {
        Span<byte> buffer = stackalloc byte[16];

        int written = Cuid2.WriteBase36(value, buffer);

        Encoding.ASCII.GetString(buffer[..written]).Should().Be(expected);
    }

    [Test]
    [Arguments(new byte[] { 0 }, "0")]
    [Arguments(new byte[] { 35 }, "z")]
    [Arguments(new byte[] { 36 }, "10")]
    [Arguments(new byte[] { 1, 0 }, "74")]
    [Arguments(new byte[] { 0, 0, 1, 0 }, "74")]
    public void EncodeBase36_KnownVectors(byte[] input, string expected)
    {
        var buffer = new byte[108];

        int written = Cuid2.EncodeBase36(input, buffer);

        Encoding.ASCII.GetString(buffer, 0, written).Should().Be(expected);
    }

    [Test]
    public void EncodeBase36_MaxValue_MatchesBigIntegerReference()
    {
        var input = Enumerable.Repeat((byte)0xFF, 64).ToArray();
        var buffer = new byte[108];

        int written = Cuid2.EncodeBase36(input, buffer);

        Encoding.ASCII.GetString(buffer, 0, written).Should().Be(ReferenceBase36(input));
    }

    [Test]
    public void EncodeBase36_RandomValues_MatchBigIntegerReference()
    {
        var buffer = new byte[108];
        var input = new byte[64];
        var mismatches = new List<string>();

        for (int i = 0; i < 1_000; i++)
        {
            RandomNumberGenerator.Fill(input);
            int written = Cuid2.EncodeBase36(input, buffer);

            var actual = Encoding.ASCII.GetString(buffer, 0, written);
            var expected = ReferenceBase36(input);
            if (actual != expected)
                mismatches.Add(Convert.ToHexString(input));
        }

        mismatches.Should().BeEmpty();
    }

    [Test]
    [Arguments("")]
    [Arguments("hello")]
    [Arguments("lk1xyz0abcdefghijklmnopqrstuvwx1234abcdefghijklmnopqrstuvwxyz012345")]
    public void Hash_MatchesSpecReference(string input)
    {
        Cuid2Tests.SkipIfUnsupported();

        var bytes = Encoding.UTF8.GetBytes(input);
        var buffer = new byte[108];

        int written = Cuid2.Hash(bytes, buffer);

        Encoding.ASCII.GetString(buffer, 0, written).Should().Be(ReferenceHash(bytes));
    }

    [Test]
    public void Hash_Output_IsLongerThanMaxLength()
    {
        Cuid2Tests.SkipIfUnsupported();

        var bytes = Encoding.UTF8.GetBytes("hello");
        var buffer = new byte[108];

        int written = Cuid2.Hash(bytes, buffer);

        written.Should().BeGreaterThan(Cuid2.MaxLength);
    }

    // spec: bufToBigInt(sha3(input)).toString(36).slice(1)
    private static string ReferenceHash(byte[] input)
    {
        var digest = SHA3_512.HashData(input);
        return ReferenceBase36(digest)[1..];
    }

    private static string ReferenceBase36(byte[] bigEndian)
    {
        var value = new BigInteger(bigEndian, isUnsigned: true, isBigEndian: true);
        if (value.IsZero)
            return "0";

        var builder = new StringBuilder();
        while (!value.IsZero)
        {
            value = BigInteger.DivRem(value, 36, out var remainder);
            builder.Insert(0, Base36Alphabet[(int)remainder]);
        }

        return builder.ToString();
    }
}
