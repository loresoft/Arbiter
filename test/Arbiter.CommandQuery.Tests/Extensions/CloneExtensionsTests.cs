using System.Text.Json;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class CloneExtensionsTests
{
    private static CloneSample CreateSample() => new()
    {
        Id = 42,
        Name = "Sample",
        Created = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
        Tags = ["one", "two"],
        Child = new CloneChild { Value = 3.5 },
    };

    private static void AssertDeepClone(CloneSample original, CloneSample clone)
    {
        clone.Should().NotBeSameAs(original);
        clone.Should().BeEquivalentTo(original);
        clone.Tags.Should().NotBeSameAs(original.Tags);
        clone.Child.Should().NotBeSameAs(original.Child);
    }

    [Test]
    public void JsonClone_CreatesDeepCopy()
    {
        var original = CreateSample();

        var clone = original.JsonClone();

        AssertDeepClone(original, clone);
    }

    [Test]
    public void JsonClone_WithOptions_CreatesDeepCopy()
    {
        var original = CreateSample();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var clone = original.JsonClone(options);

        AssertDeepClone(original, clone);
    }

    [Test]
    public void JsonClone_WithTypeInfo_CreatesDeepCopy()
    {
        var original = CreateSample();

        var clone = original.JsonClone(CloneJsonContext.Default.CloneSample);

        AssertDeepClone(original, clone);
    }

    [Test]
    public void JsonClone_ModifyingClone_DoesNotAffectOriginal()
    {
        var original = CreateSample();

        var clone = original.JsonClone();
        clone.Name = "Changed";
        clone.Tags.Add("three");
        clone.Child!.Value = 99;

        original.Name.Should().Be("Sample");
        original.Tags.Should().HaveCount(2);
        original.Child!.Value.Should().Be(3.5);
    }

    [Test]
    public void JsonClone_Null_ReturnsNull()
    {
        CloneSample? original = null;

        original.JsonClone().Should().BeNull();
        original.JsonClone(CloneJsonContext.Default.CloneSample).Should().BeNull();
    }

    [Test]
    public void JsonClone_NullTypeInfo_Throws()
    {
        var original = CreateSample();

        var action = () => original.JsonClone((System.Text.Json.Serialization.Metadata.JsonTypeInfo<CloneSample>)null!);

        action.Should().Throw<ArgumentNullException>();
    }
}

public class CloneSample
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateTimeOffset Created { get; set; }

    public List<string> Tags { get; set; } = [];

    public CloneChild? Child { get; set; }
}

public class CloneChild
{
    public double Value { get; set; }
}

[JsonSerializable(typeof(CloneSample))]
internal partial class CloneJsonContext : JsonSerializerContext;
