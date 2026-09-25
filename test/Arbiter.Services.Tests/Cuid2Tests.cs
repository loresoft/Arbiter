using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Arbiter.Services.Tests;

public class Cuid2Tests
{
    [Test]
    public void NewCuid2_Default_HasDefaultLength()
    {
        SkipIfUnsupported();

        var id = Cuid2.NewCuid();

        id.ToString().Length.Should().Be(Cuid2.DefaultLength);
    }

    [Test]
    [Arguments(2)]
    [Arguments(5)]
    [Arguments(10)]
    [Arguments(24)]
    [Arguments(31)]
    [Arguments(32)]
    public void NewCuid2_WithLength_HasRequestedLength(int length)
    {
        SkipIfUnsupported();

        var id = Cuid2.NewCuid(length);

        id.Length.Should().Be(length);
    }

    [Test]
    [Arguments(-1)]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(33)]
    [Arguments(100)]
    public void NewCuid2_WithInvalidLength_Throws(int length)
    {
        var action = () => Cuid2.NewCuid(length);

        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void NewCuid2_GeneratedId_IsValid()
    {
        SkipIfUnsupported();

        var id = Cuid2.NewCuid();

        Cuid2.IsValid(id.ToString()).Should().BeTrue();
    }

    [Test]
    public void NewCuid2_ManyIds_AreUnique()
    {
        SkipIfUnsupported();

        const int count = 100_000;
        var ids = new HashSet<Cuid2>(count);
        for (int i = 0; i < count; i++)
            ids.Add(Cuid2.NewCuid());

        ids.Count.Should().Be(count);
    }

    [Test]
    public void NewCuid2_ParallelGeneration_AreUnique()
    {
        SkipIfUnsupported();

        const int count = 100_000;
        var ids = new ConcurrentDictionary<Cuid2, byte>();
        Parallel.For(0, count, _ => ids.TryAdd(Cuid2.NewCuid(), 0));

        ids.Count.Should().Be(count);
    }

    [Test]
    public void Parse_RoundTrip_ReturnsEqualValue()
    {
        SkipIfUnsupported();

        var id = Cuid2.NewCuid();

        var parsed = Cuid2.Parse(id.ToString());

        parsed.Should().Be(id);
    }

    [Test]
    public void TryParse_Utf8RoundTrip_ReturnsEqualValue()
    {
        SkipIfUnsupported();

        var id = Cuid2.NewCuid();
        var utf8 = Encoding.UTF8.GetBytes(id.ToString());

        Cuid2.TryParse(utf8, out var parsed).Should().BeTrue();
        parsed.Should().Be(id);
    }

    [Test]
    [Arguments("")]
    [Arguments("a")]
    [Arguments("1abc")]
    [Arguments("Abc")]
    [Arguments("abC")]
    [Arguments("ab-c")]
    [Arguments("ab c")]
    [Arguments("abcdefghijklmnopqrstuvwxyz0123456")]
    public void TryParse_InvalidValue_ReturnsFalse(string value)
    {
        var success = Cuid2.TryParse(value, out var result);

        success.Should().BeFalse();
        result.Should().Be(Cuid2.Empty);
    }

    [Test]
    public void TryParse_Null_ReturnsFalse()
    {
        Cuid2.TryParse((string?)null, out _).Should().BeFalse();
    }

    [Test]
    public void Parse_InvalidValue_ThrowsFormatException()
    {
        var action = () => Cuid2.Parse("NOT-VALID");

        action.Should().Throw<FormatException>();
    }

    [Test]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        var action = () => Cuid2.Parse(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Equality_SameValue_AreEqual()
    {
        var left = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        var right = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");

        (left == right).Should().BeTrue();
    }

    [Test]
    public void Equality_SameValue_HaveSameHashCode()
    {
        var left = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        var right = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");

        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Test]
    public void Equality_DifferentValue_AreNotEqual()
    {
        var left = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        var right = Cuid2.Parse("pfh0haxfpzowht3oi213cqos");

        (left != right).Should().BeTrue();
    }

    [Test]
    public void Equality_PrefixValue_AreNotEqual()
    {
        var left = Cuid2.Parse("abc");
        var right = Cuid2.Parse("abcd");

        left.Equals(right).Should().BeFalse();
    }

    [Test]
    public void CompareTo_OrdersOrdinally()
    {
        var left = Cuid2.Parse("abc");
        var right = Cuid2.Parse("abd");

        left.CompareTo(right).Should().BeNegative();
    }

    [Test]
    public void Default_IsEmpty()
    {
        var id = default(Cuid2);

        id.ToString().Should().BeEmpty();
    }

    [Test]
    public void TryFormat_SmallBuffer_ReturnsFalse()
    {
        var id = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        Span<char> buffer = stackalloc char[10];

        var success = id.TryFormat(buffer, out int written);

        success.Should().BeFalse();
        written.Should().Be(0);
    }

    [Test]
    public void TryFormat_Chars_WritesValue()
    {
        var id = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        var buffer = new char[Cuid2.MaxLength];

        id.TryFormat(buffer, out int written).Should().BeTrue();
        new string(buffer, 0, written).Should().Be("tz4a98xxat96iws9zmbrgj3a");
    }

    [Test]
    public void TryFormat_Utf8_WritesValue()
    {
        var id = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        var buffer = new byte[Cuid2.MaxLength];

        id.TryFormat(buffer, out int written).Should().BeTrue();
        Encoding.UTF8.GetString(buffer, 0, written).Should().Be("tz4a98xxat96iws9zmbrgj3a");
    }

    [Test]
    public void Interpolation_UsesValue()
    {
        var id = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");

        var text = $"id:{id}";

        text.Should().Be("id:tz4a98xxat96iws9zmbrgj3a");
    }

    [Test]
    public void Json_Serialize_WritesString()
    {
        var id = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");

        var json = JsonSerializer.Serialize(id);

        json.Should().Be("\"tz4a98xxat96iws9zmbrgj3a\"");
    }

    [Test]
    public void Json_RoundTripInObject_ReturnsEqualValue()
    {
        var id = Cuid2.Parse("tz4a98xxat96iws9zmbrgj3a");
        var model = new Cuid2Model(id, "Test");

        var json = JsonSerializer.Serialize(model);
        var result = JsonSerializer.Deserialize<Cuid2Model>(json);

        result.Should().Be(model);
    }

    [Test]
    public void Json_Null_ReadsEmpty()
    {
        var result = JsonSerializer.Deserialize<Cuid2>("null");

        result.Should().Be(Cuid2.Empty);
    }

    [Test]
    public void Json_Empty_WritesNull()
    {
        var json = JsonSerializer.Serialize(Cuid2.Empty);

        json.Should().Be("null");
    }

    [Test]
    public void Json_EscapedValue_Reads()
    {
        var result = JsonSerializer.Deserialize<Cuid2>("\"\\u0074z4a98xxat96iws9zmbrgj3a\"");

        result.ToString().Should().Be("tz4a98xxat96iws9zmbrgj3a");
    }

    [Test]
    public void Json_InvalidValue_Throws()
    {
        var action = () => JsonSerializer.Deserialize<Cuid2>("\"NOT-VALID\"");

        action.Should().Throw<JsonException>();
    }

    [Test]
    public void Json_NumberToken_Throws()
    {
        var action = () => JsonSerializer.Deserialize<Cuid2>("123");

        action.Should().Throw<JsonException>();
    }

    internal static void SkipIfUnsupported()
        => Skip.When(!Cuid2.IsSupported, "SHA3-512 is not supported on this platform.");

    public record Cuid2Model(Cuid2 Id, string Name);
}
