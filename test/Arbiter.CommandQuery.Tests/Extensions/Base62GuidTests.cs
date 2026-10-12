using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class Base62GuidTests
{
    [Test]
    public void ToBase62_ThenFromBase62_ShouldRestoreOriginalGuid()
    {
        // Arrange
        var original = Guid.NewGuid();

        // Act
        var encoded = original.ToBase62();
        var restored = Guid.FromBase62(encoded);

        // Assert
        restored.Should().Be(original);
    }

    [Test]
    public void ToBase62_ShouldProduceAlphanumericString()
    {
        // Arrange
        var original = new Guid("00112233-4455-6677-8899-aabbccddeeff");

        // Act
        var encoded = original.ToBase62();

        // Assert
        encoded.Should().NotBeNullOrEmpty();
        encoded.All(char.IsAsciiLetterOrDigit).Should().BeTrue();
    }

    [Test]
    public void ToBase62_ShouldBeDeterministic()
    {
        // Arrange
        var original = Guid.NewGuid();

        // Act
        var first = original.ToBase62();
        var second = original.ToBase62();

        // Assert
        first.Should().Be(second);
    }

    [Test]
    public void ToBase62_DifferentGuids_ShouldProduceDifferentStrings()
    {
        // Arrange
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        // Act
        var firstEncoded = first.ToBase62();
        var secondEncoded = second.ToBase62();

        // Assert
        firstEncoded.Should().NotBe(secondEncoded);
    }

    [Test]
    public void ToBase62_EmptyGuid_ShouldRoundTrip()
    {
        // Act
        var encoded = Guid.Empty.ToBase62();
        var restored = Guid.FromBase62(encoded);

        // Assert
        restored.Should().Be(Guid.Empty);
    }

    [Test]
    public void ToBase62_AllBitsSetGuid_ShouldRoundTrip()
    {
        // Arrange
        var original = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");

        // Act
        var encoded = original.ToBase62();
        var restored = Guid.FromBase62(encoded);

        // Assert
        restored.Should().Be(original);
    }

    [Test]
    public void ToBase62_ManyGuids_ShouldRoundTrip()
    {
        for (var i = 0; i < 1000; i++)
        {
            var original = Guid.NewGuid();

            var restored = Guid.FromBase62(original.ToBase62());

            restored.Should().Be(original);
        }
    }

    [Test]
    public void FromBase62_NullValue_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => Guid.FromBase62(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void FromBase62_InvalidCharacters_ShouldThrowFormatException()
    {
        // Act
        var action = () => Guid.FromBase62("!!!invalid-base62!!!");

        // Assert
        action.Should().Throw<FormatException>();
    }

    [Test]
    public void FromBase62_EmptyString_ShouldThrowFormatException()
    {
        // Act
        var action = () => Guid.FromBase62(string.Empty);

        // Assert
        action.Should().Throw<FormatException>();
    }

    [Test]
    public void FromBase62_WrongLength_ShouldThrowFormatException()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4 };
        var encoded = Arbiter.Services.Base62.EncodeToString(bytes);

        // Act
        var action = () => Guid.FromBase62(encoded);

        // Assert
        action.Should().Throw<FormatException>();
    }
}
