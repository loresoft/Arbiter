using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class ToTimestampTests
{
    [Test]
    public void ToTimestamp_OnNewSqlGuid_ShouldReturnTimestampCloseToNow()
    {
        // Arrange & Act
        var sqlGuid = Guid.NewSqlGuid();
        var result = sqlGuid.ToTimestamp();

        // Assert – UUIDv7 timestamps have millisecond precision, so allow a
        // small window for truncation and CI scheduling delays.
        result.Should().NotBeNull();
        result!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void ToTimestamp_OnNewSqlGuid_ShouldReturnUtcOffset()
    {
        // Arrange & Act
        var result = Guid.NewSqlGuid().ToTimestamp();

        // Assert
        result.Should().NotBeNull();
        result!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void ToTimestamp_OnSqlGuidWithExplicitTimestamp_ShouldReturnEmbeddedTimestamp()
    {
        // Arrange
        var timestamp = DateTimeOffset.UnixEpoch.AddMilliseconds(1_234_567_890);

        // Act
        var result = Guid.NewSqlGuid(timestamp).ToTimestamp();

        // Assert
        result.Should().Be(timestamp);
    }

    [Test]
    public void ToTimestamp_OnStandardUuidV7_ShouldReturnEmbeddedTimestamp()
    {
        // Arrange - standard UUIDv7 (not SQL-ordered) should also be handled
        var timestamp = DateTimeOffset.UnixEpoch.AddMilliseconds(1_234_567_890);

        // Act
        var result = Guid.CreateVersion7(timestamp).ToTimestamp();

        // Assert
        result.Should().Be(timestamp);
    }

    [Test]
    public void ToTimestamp_OnEmptyGuid_ShouldReturnNull()
    {
        // Act
        var result = Guid.Empty.ToTimestamp();

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ToTimestamp_OnNonUuidV7Guid_ShouldReturnNull()
    {
        // Arrange - a deterministic GUID with version nibble 4 (UUIDv4) in the standard position
        var v4Guid = new Guid("00000000-0000-4000-8000-000000000000");

        // Act
        var result = v4Guid.ToTimestamp();

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ToTimestamp_OnConsecutiveSqlGuids_ShouldReturnNonDecreasingTimestamps()
    {
        // Arrange & Act
        var ts1 = Guid.NewSqlGuid().ToTimestamp();
        var ts2 = Guid.NewSqlGuid().ToTimestamp();

        // Assert
        ts1.Should().NotBeNull();
        ts2.Should().NotBeNull();
        ts1!.Value.Should().BeOnOrBefore(ts2!.Value);
    }
}
