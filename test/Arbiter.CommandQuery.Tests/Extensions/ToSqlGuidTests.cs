using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class ToSqlGuidTests
{
    [Test]
    public void ToSqlGuid_ShouldProduceDifferentGuid()
    {
        // Arrange - use a known asymmetric GUID so the reordered result is predictably different
        var original = new Guid("00112233-4455-6677-8899-aabbccddeeff");

        // Act
        var sqlGuid = original.ToSqlGuid();

        // Assert
        sqlGuid.Should().NotBe(original);
    }

    [Test]
    public void ToSqlGuid_OnEmptyGuid_ShouldReturnEmptyGuid()
    {
        // Act
        var result = Guid.Empty.ToSqlGuid();

        // Assert
        result.Should().Be(Guid.Empty);
    }

    [Test]
    public void ToSqlGuid_ThenFromSqlGuid_ShouldRestoreOriginalGuid()
    {
        // Arrange
        var original = Guid.NewGuid();

        // Act
        var restored = original.ToSqlGuid().FromSqlGuid();

        // Assert
        restored.Should().Be(original);
    }

    [Test]
    public void ToSqlGuid_KnownInput_ShouldProduceExpectedByteOrdering()
    {
        // Arrange
        // Input bytes (from TryWriteBytes, little-endian Data1/2/3):
        //   {33,22,11,00, 55,44, 77,66, 88,99,aa,bb,cc,dd,ee,ff}
        // After WriteToSqlByteOrder the destination bytes become:
        //   {cc,dd,ee,ff, aa,bb, 88,99, 66,77,00,11,22,33,44,55}
        // Which new Guid() reads as: Data1=0xffeeddcc, Data2=0xbbaa, Data3=0x9988
        var input = new Guid("00112233-4455-6677-8899-aabbccddeeff");
        var expected = new Guid("ffeeddcc-bbaa-9988-6677-001122334455");

        // Act
        var result = input.ToSqlGuid();

        // Assert
        result.Should().Be(expected);
    }
}
