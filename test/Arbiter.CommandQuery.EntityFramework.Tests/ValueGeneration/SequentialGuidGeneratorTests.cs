using Arbiter.CommandQuery.EntityFramework.ValueGeneration;
using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.EntityFramework.Tests.ValueGeneration;

public class SequentialGuidGeneratorTests
{
    private readonly SequentialGuidGenerator _generator = new();

    [Test]
    public void GeneratesTemporaryValues_IsFalse()
    {
        _generator.GeneratesTemporaryValues.Should().BeFalse();
    }

    [Test]
    public void Next_ReturnsNonEmptyGuid()
    {
        var result = _generator.Next(null!);

        result.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void Next_ReturnsUniqueValues()
    {
        var first = _generator.Next(null!);
        var second = _generator.Next(null!);

        first.Should().NotBe(second);
    }

    [Test]
    public async Task Next_AcrossMilliseconds_ReturnsIncreasingValues()
    {
        var first = _generator.Next(null!);

        // Ensure a new timestamp millisecond so that the temporal component increases.
        await Task.Delay(2);

        var second = _generator.Next(null!);

        // Convert SQL-ordered GUIDs back to standard .NET ordering so that
        // Guid.CompareTo reflects the original UUIDv7 timestamp ordering.
        var firstNet = first.FromSqlGuid();
        var secondNet = second.FromSqlGuid();

        firstNet.CompareTo(secondNet).Should().BeLessThan(0);
    }
}
