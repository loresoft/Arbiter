using System.Buffers.Binary;
using System.Text;
using AwesomeAssertions;
using TUnit.Core;

namespace Arbiter.Services.Tests;

public class TypeBufferTests
{
    #region Prefix Tests

    [Test]
    public void Prefix_WithValidTypeNameAndPayload_ReturnsCorrectBuffer()
    {
        // Arrange
        var typeName = "MyType";
        var payload = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var result = TypeBuffer.Prefix(typeName, payload);

        // Assert
        var resultSpan = result.Span;
        var typeNameByteCount = Encoding.UTF8.GetByteCount(typeName);

        // Verify total length
        result.Length.Should().Be(4 + typeNameByteCount + payload.Length);

        // Verify type name length (first 4 bytes)
        var storedLength = BinaryPrimitives.ReadInt32BigEndian(resultSpan);
        storedLength.Should().Be(typeNameByteCount);

        // Verify type name
        var extractedTypeName = Encoding.UTF8.GetString(resultSpan.Slice(4, typeNameByteCount));
        extractedTypeName.Should().Be(typeName);

        // Verify payload
        var extractedPayload = resultSpan[(4 + typeNameByteCount)..];
        extractedPayload.SequenceEqual(payload).Should().BeTrue();
    }

    [Test]
    public void Prefix_WithEmptyPayload_ReturnsBufferWithTypeNameOnly()
    {
        // Arrange
        var typeName = "EmptyPayloadType";
        var payload = ReadOnlySpan<byte>.Empty;

        // Act
        var result = TypeBuffer.Prefix(typeName, payload);

        // Assert
        var typeNameByteCount = Encoding.UTF8.GetByteCount(typeName);
        result.Length.Should().Be(4 + typeNameByteCount);
    }

    [Test]
    public void Prefix_WithUnicodeTypeName_EncodesCorrectly()
    {
        // Arrange
        var typeName = "类型名称🚀";
        var payload = new byte[] { 10, 20, 30 };

        // Act
        var result = TypeBuffer.Prefix(typeName, payload);

        // Assert
        var resultSpan = result.Span;
        var typeNameByteCount = Encoding.UTF8.GetByteCount(typeName);
        var extractedTypeName = Encoding.UTF8.GetString(resultSpan.Slice(4, typeNameByteCount));
        extractedTypeName.Should().Be(typeName);
    }

    [Test]
    public void Prefix_WithLongTypeName_HandlesCorrectly()
    {
        // Arrange
        var typeName = new string('A', 1000);
        var payload = new byte[] { 1, 2, 3 };

        // Act
        var result = TypeBuffer.Prefix(typeName, payload);

        // Assert
        var resultSpan = result.Span;
        var typeNameByteCount = Encoding.UTF8.GetByteCount(typeName);
        var storedLength = BinaryPrimitives.ReadInt32BigEndian(resultSpan);
        storedLength.Should().Be(typeNameByteCount);
    }

    [Test]
    public void Prefix_WithNullTypeName_ThrowsArgumentException()
    {
        // Arrange
        string typeName = null!;
        var payload = new byte[] { 1, 2, 3 };

        // Act & Assert
        var action = () => TypeBuffer.Prefix(typeName, payload);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("typeName");
    }

    [Test]
    public void Prefix_WithEmptyTypeName_ThrowsArgumentException()
    {
        // Arrange
        var typeName = string.Empty;
        var payload = new byte[] { 1, 2, 3 };

        // Act & Assert
        var action = () => TypeBuffer.Prefix(typeName, payload);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("typeName");
    }

    #endregion

    #region Extract Tests

    [Test]
    public void Extract_WithValidBuffer_ReturnsTypeNameAndPayload()
    {
        // Arrange
        var typeName = "TestType";
        var payload = new byte[] { 5, 6, 7, 8, 9 };
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var (extractedTypeName, extractedPayload) = TypeBuffer.Extract(prefixed.Span);

        // Assert
        extractedTypeName.Should().Be(typeName);
        extractedPayload.Span.SequenceEqual(payload).Should().BeTrue();
    }

    [Test]
    public void Extract_WithEmptyPayload_ReturnsTypeNameAndEmptyPayload()
    {
        // Arrange
        var typeName = "TypeWithoutPayload";
        var payload = ReadOnlySpan<byte>.Empty;
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var (extractedTypeName, extractedPayload) = TypeBuffer.Extract(prefixed.Span);

        // Assert
        extractedTypeName.Should().Be(typeName);
        extractedPayload.Length.Should().Be(0);
    }

    [Test]
    public void Extract_WithUnicodeTypeName_ExtractsCorrectly()
    {
        // Arrange
        var typeName = "类型🎯";
        var payload = new byte[] { 100, 101, 102 };
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var (extractedTypeName, extractedPayload) = TypeBuffer.Extract(prefixed.Span);

        // Assert
        extractedTypeName.Should().Be(typeName);
        extractedPayload.Span.SequenceEqual(payload).Should().BeTrue();
    }

    [Test]
    public void Extract_RoundTrip_PreservesData()
    {
        // Arrange
        var typeName = "ComplexType.Namespace.FullName";
        var payload = new byte[256];
        Random.Shared.NextBytes(payload);

        // Act
        var prefixed = TypeBuffer.Prefix(typeName, payload);
        var (extractedTypeName, extractedPayload) = TypeBuffer.Extract(prefixed.Span);

        // Assert
        extractedTypeName.Should().Be(typeName);
        extractedPayload.Span.SequenceEqual(payload).Should().BeTrue();
    }

    [Test]
    public void Extract_WithBufferTooShort_ThrowsArgumentException()
    {
        // Arrange
        var buffer = new byte[] { 1, 2, 3 }; // Less than 4 bytes

        // Act & Assert
        var action = () => TypeBuffer.Extract(buffer);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("prefixedBuffer")
            .WithMessage("*too short*");
    }

    [Test]
    public void Extract_WithCorruptedBuffer_ThrowsArgumentException()
    {
        // Arrange
        var buffer = new byte[10];
        BinaryPrimitives.WriteInt32BigEndian(buffer, 100); // Length exceeds buffer size

        // Act & Assert
        var action = () => TypeBuffer.Extract(buffer);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("prefixedBuffer")
            .WithMessage("*corrupted*");
    }

    [Test]
    public void Extract_WithEmptyBuffer_ThrowsArgumentException()
    {
        // Arrange
        var buffer = Array.Empty<byte>();

        // Act & Assert
        var action = () => TypeBuffer.Extract(buffer);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("prefixedBuffer");
    }

    #endregion

    #region Peek Tests

    [Test]
    public void Peek_WithValidBuffer_ReturnsTypeName()
    {
        // Arrange
        var typeName = "PeekType";
        var payload = new byte[] { 1, 2, 3, 4, 5 };
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var peekedTypeName = TypeBuffer.Peek(prefixed.Span);

        // Assert
        peekedTypeName.Should().Be(typeName);
    }

    [Test]
    public void Peek_WithUnicodeTypeName_ReturnsCorrectTypeName()
    {
        // Arrange
        var typeName = "窥视类型🔍";
        var payload = new byte[] { 10, 20, 30 };
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var peekedTypeName = TypeBuffer.Peek(prefixed.Span);

        // Assert
        peekedTypeName.Should().Be(typeName);
    }

    [Test]
    public void Peek_WithEmptyPayload_ReturnsTypeName()
    {
        // Arrange
        var typeName = "NoPayloadType";
        var payload = ReadOnlySpan<byte>.Empty;
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var peekedTypeName = TypeBuffer.Peek(prefixed.Span);

        // Assert
        peekedTypeName.Should().Be(typeName);
    }

    [Test]
    public void Peek_WithLargePayload_DoesNotReadPayload()
    {
        // Arrange
        var typeName = "ShortType";
        var payload = new byte[10000]; // Large payload
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var peekedTypeName = TypeBuffer.Peek(prefixed.Span);

        // Assert
        peekedTypeName.Should().Be(typeName);
    }

    [Test]
    public void Peek_WithBufferTooShort_ThrowsArgumentException()
    {
        // Arrange
        var buffer = new byte[] { 1, 2 }; // Less than 4 bytes

        // Act & Assert
        var action = () => TypeBuffer.Peek(buffer);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("prefixedBuffer")
            .WithMessage("*too short*");
    }

    [Test]
    public void Peek_WithCorruptedBuffer_ThrowsArgumentException()
    {
        // Arrange
        var buffer = new byte[8];
        BinaryPrimitives.WriteInt32BigEndian(buffer, 50); // Length exceeds buffer size

        // Act & Assert
        var action = () => TypeBuffer.Peek(buffer);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("prefixedBuffer")
            .WithMessage("*corrupted*");
    }

    [Test]
    public void Peek_WithEmptyBuffer_ThrowsArgumentException()
    {
        // Arrange
        var buffer = Array.Empty<byte>();

        // Act & Assert
        var action = () => TypeBuffer.Peek(buffer);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("prefixedBuffer");
    }

    [Test]
    public void Peek_MultipleCallsOnSameBuffer_ReturnsSameResult()
    {
        // Arrange
        var typeName = "ConsistentType";
        var payload = new byte[] { 7, 8, 9 };
        var prefixed = TypeBuffer.Prefix(typeName, payload);

        // Act
        var result1 = TypeBuffer.Peek(prefixed.Span);
        var result2 = TypeBuffer.Peek(prefixed.Span);
        var result3 = TypeBuffer.Peek(prefixed.Span);

        // Assert
        result1.Should().Be(result2);
        result2.Should().Be(result3);
    }

    #endregion

    #region Integration Tests

    [Test]
    public void Integration_PrefixExtractPeek_WorkTogether()
    {
        // Arrange
        var typeName = "IntegrationTestType";
        var payload = new byte[] { 11, 22, 33, 44, 55 };

        // Act
        var prefixed = TypeBuffer.Prefix(typeName, payload);
        var peekedTypeName = TypeBuffer.Peek(prefixed.Span);
        var (extractedTypeName, extractedPayload) = TypeBuffer.Extract(prefixed.Span);

        // Assert
        peekedTypeName.Should().Be(typeName);
        extractedTypeName.Should().Be(typeName);
        extractedPayload.Span.SequenceEqual(payload).Should().BeTrue();
    }

    [Test]
    [Arguments("Type1", new byte[] { 1 })]
    [Arguments("VeryLongTypeNameWithMultipleWords", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })]
    [Arguments("A", new byte[] { 255 })]
    [Arguments("Namespace.Class.SubClass", new byte[] { })]
    public void VariousInputs_WorkCorrectly(string typeName, byte[] payload)
    {
        // Act
        var prefixed = TypeBuffer.Prefix(typeName, payload);
        var (extractedTypeName, extractedPayload) = TypeBuffer.Extract(prefixed.Span);

        // Assert
        extractedTypeName.Should().Be(typeName);
        extractedPayload.Span.SequenceEqual(payload).Should().BeTrue();
    }

    #endregion
}
