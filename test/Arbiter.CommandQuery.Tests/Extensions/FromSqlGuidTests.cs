using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class FromSqlGuidTests
{
    [Test]
    public void FromSqlGuid_OnEmptyGuid_ShouldReturnEmptyGuid()
    {
        // Act
        var result = Guid.Empty.FromSqlGuid();

        // Assert
        result.Should().Be(Guid.Empty);
    }

    [Test]
    public void FromSqlGuid_ThenToSqlGuid_ShouldRestoreOriginalGuid()
    {
        // Arrange
        var sqlGuid = Guid.NewGuid();

        // Act
        var restored = sqlGuid.FromSqlGuid().ToSqlGuid();

        // Assert
        restored.Should().Be(sqlGuid);
    }

    [Test]
    public void FromSqlGuid_KnownSqlGuid_ShouldRestoreOriginalGuid()
    {
        // Arrange - inverse of ToSqlGuid_KnownInput_ShouldProduceExpectedByteOrdering
        var sqlGuid = new Guid("ffeeddcc-bbaa-9988-6677-001122334455");
        var expected = new Guid("00112233-4455-6677-8899-aabbccddeeff");

        // Act
        var result = sqlGuid.FromSqlGuid();

        // Assert
        result.Should().Be(expected);
    }

    [Test]
    public void FromSqlGuid_ShouldRestoreChronologicalComparability()
    {
        // Arrange - two SQL GUIDs generated from timestamps one hour apart
        var t1 = DateTimeOffset.UnixEpoch.AddMilliseconds(1_000_000);
        var t2 = t1.AddHours(1);
        var sqlGuid1 = Guid.NewSqlGuid(t1);
        var sqlGuid2 = Guid.NewSqlGuid(t2);

        // Act - restore standard .NET ordering so Guid.CompareTo reflects the original timestamp order
        var netGuid1 = sqlGuid1.FromSqlGuid();
        var netGuid2 = sqlGuid2.FromSqlGuid();

        // Assert
        netGuid1.CompareTo(netGuid2).Should().BeLessThan(0);
    }
}
