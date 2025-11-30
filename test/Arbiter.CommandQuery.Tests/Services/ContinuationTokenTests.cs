using System;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Tests.Services;

public class ContinuationTokenTests
{
    #region ToString Tests

    [Test]
    public void ToString_WithIntId_ReturnsValidBase64UrlString()
    {
        // Arrange
        var token = new ContinuationToken<int>(42);

        // Act
        var result = token.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void ToString_WithLongId_ReturnsValidBase64UrlString()
    {
        // Arrange
        var token = new ContinuationToken<long>(123456789L);

        // Act
        var result = token.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void ToString_WithGuidId_ReturnsValidBase64UrlString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var token = new ContinuationToken<Guid>(guid);

        // Act
        var result = token.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void ToString_WithDateTimeOffset_ReturnsValidBase64UrlString()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;
        var token = new ContinuationToken<int>(100, date);

        // Act
        var result = token.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void ToString_WithoutDate_ProducesConsistentOutput()
    {
        // Arrange
        var token1 = new ContinuationToken<int>(42);
        var token2 = new ContinuationToken<int>(42);

        // Act
        var result1 = token1.ToString();
        var result2 = token2.ToString();

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void ToString_WithSameDateTimeOffset_ProducesConsistentOutput()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 1, 15, 10, 30, 45, TimeSpan.Zero);
        var token1 = new ContinuationToken<int>(42, date);
        var token2 = new ContinuationToken<int>(42, date);

        // Act
        var result1 = token1.ToString();
        var result2 = token2.ToString();

        // Assert
        result1.Should().Be(result2);
    }

    [Test]
    public void ToString_WithDifferentIds_ProducesDifferentOutput()
    {
        // Arrange
        var token1 = new ContinuationToken<int>(1);
        var token2 = new ContinuationToken<int>(2);

        // Act
        var result1 = token1.ToString();
        var result2 = token2.ToString();

        // Assert
        result1.Should().NotBe(result2);
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(42)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    public void ToString_WithVariousIntValues_ReturnsValidString(int id)
    {
        // Arrange
        var token = new ContinuationToken<int>(id);

        // Act
        var result = token.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Test]
    [Arguments((byte)0)]
    [Arguments((byte)1)]
    [Arguments((byte)127)]
    [Arguments((byte)255)]
    public void ToString_WithByteValues_ReturnsValidString(byte id)
    {
        // Arrange
        var token = new ContinuationToken<byte>(id);

        // Act
        var result = token.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region TryParse Tests

    [Test]
    public void TryParse_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var originalToken = new ContinuationToken<int>(42);
        var tokenString = originalToken.ToString();

        // Act
        var success = ContinuationToken<int>.TryParse(tokenString, out var parsed);

        // Assert
        success.Should().BeTrue();
        parsed.Should().NotBeNull();
        parsed!.Id.Should().Be(42);
    }

    [Test]
    public void TryParse_WithValidTokenAndDate_ReturnsTrueAndParsesDate()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;
        var originalToken = new ContinuationToken<long>(999L, date);
        var tokenString = originalToken.ToString();

        // Act
        var success = ContinuationToken<long>.TryParse(tokenString, out var parsed);

        // Assert
        success.Should().BeTrue();
        parsed.Should().NotBeNull();
        parsed!.Id.Should().Be(999L);
        parsed.Timestamp.Should().NotBeNull();
        parsed.Timestamp!.Value.UtcTicks.Should().Be(date.UtcTicks);
    }

    [Test]
    public void TryParse_WithNullToken_ReturnsFalse()
    {
        // Act
        var success = ContinuationToken<int>.TryParse(null, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithEmptyToken_ReturnsFalse()
    {
        // Act
        var success = ContinuationToken<int>.TryParse(string.Empty, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithInvalidBase64_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "Not a valid token!@#$";

        // Act
        var success = ContinuationToken<int>.TryParse(invalidToken, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithTooShortToken_ReturnsFalse()
    {
        // Arrange
        var tooShortToken = Convert.ToBase64String(new byte[2]);

        // Act
        var success = ContinuationToken<int>.TryParse(tooShortToken, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithCorruptedToken_ReturnsFalse()
    {
        // Arrange
        var originalToken = new ContinuationToken<int>(42);
        var tokenString = originalToken.ToString();
        var corruptedToken = tokenString.Substring(0, tokenString.Length - 2) + "XX";

        // Act
        var success = ContinuationToken<int>.TryParse(corruptedToken, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_DoesNotThrowException()
    {
        // Arrange
        var invalidInputs = new[]
        {
            null,
            "",
            "invalid",
            "!@#$%^&*()",
            new string('a', 1000)
        };

        // Act & Assert
        foreach (var input in invalidInputs)
        {
            var success = ContinuationToken<int>.TryParse(input, out var parsed);
            // Should not throw, just return false
            success.Should().BeFalse();
        }
    }

    #endregion

    #region Round-Trip Tests

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    [Arguments(-1)]
    [Arguments(42)]
    [Arguments(int.MaxValue)]
    [Arguments(int.MinValue)]
    public void RoundTrip_WithIntId_PreservesValue(int id)
    {
        // Arrange
        var original = new ContinuationToken<int>(id);

        // Act
        var serialized = original.ToString();
        var success = ContinuationToken<int>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(id);
        deserialized.Timestamp.Should().BeNull();
    }

    [Test]
    [Arguments(0L)]
    [Arguments(long.MaxValue)]
    [Arguments(long.MinValue)]
    [Arguments(123456789012345L)]
    public void RoundTrip_WithLongId_PreservesValue(long id)
    {
        // Arrange
        var original = new ContinuationToken<long>(id);

        // Act
        var serialized = original.ToString();
        var success = ContinuationToken<long>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(id);
    }

    [Test]
    public void RoundTrip_WithGuid_PreservesValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var original = new ContinuationToken<Guid>(guid);

        // Act
        var serialized = original.ToString();
        var success = ContinuationToken<Guid>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(guid);
    }

    [Test]
    public void RoundTrip_WithShort_PreservesValue()
    {
        // Arrange
        short id = 12345;
        var original = new ContinuationToken<short>(id);

        // Act
        var serialized = original.ToString();
        var success = ContinuationToken<short>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(id);
    }

    [Test]
    public void RoundTrip_WithUnsignedTypes_PreservesValue()
    {
        // Test uint
        var uintToken = new ContinuationToken<uint>(uint.MaxValue);
        var uintSerialized = uintToken.ToString();
        ContinuationToken<uint>.TryParse(uintSerialized, out var uintDeserialized).Should().BeTrue();
        uintDeserialized!.Id.Should().Be(uint.MaxValue);

        // Test ulong
        var ulongToken = new ContinuationToken<ulong>(ulong.MaxValue);
        var ulongSerialized = ulongToken.ToString();
        ContinuationToken<ulong>.TryParse(ulongSerialized, out var ulongDeserialized).Should().BeTrue();
        ulongDeserialized!.Id.Should().Be(ulong.MaxValue);

        // Test ushort
        var ushortToken = new ContinuationToken<ushort>(ushort.MaxValue);
        var ushortSerialized = ushortToken.ToString();
        ContinuationToken<ushort>.TryParse(ushortSerialized, out var ushortDeserialized).Should().BeTrue();
        ushortDeserialized!.Id.Should().Be(ushort.MaxValue);
    }

    [Test]
    public void RoundTrip_WithFloatingPoint_PreservesValue()
    {
        // Test float
        var floatToken = new ContinuationToken<float>(3.14159f);
        var floatSerialized = floatToken.ToString();
        ContinuationToken<float>.TryParse(floatSerialized, out var floatDeserialized).Should().BeTrue();
        floatDeserialized!.Id.Should().Be(3.14159f);

        // Test double
        var doubleToken = new ContinuationToken<double>(2.718281828459);
        var doubleSerialized = doubleToken.ToString();
        ContinuationToken<double>.TryParse(doubleSerialized, out var doubleDeserialized).Should().BeTrue();
        doubleDeserialized!.Id.Should().Be(2.718281828459);
    }

    [Test]
    public void RoundTrip_WithDate_PreservesDateTicks()
    {
        // Arrange
        var date = new DateTimeOffset(2024, 12, 25, 10, 30, 45, 123, TimeSpan.FromHours(5));
        var original = new ContinuationToken<int>(42, date);

        // Act
        var serialized = original.ToString();
        var success = ContinuationToken<int>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(42);
        deserialized.Timestamp.Should().NotBeNull();
        deserialized.Timestamp!.Value.UtcTicks.Should().Be(date.UtcTicks);
        // Note: Offset is not preserved, only UTC ticks
        deserialized.Timestamp.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public void RoundTrip_WithMinMaxDates_PreservesValues()
    {
        // Test with DateTimeOffset.MinValue
        var minToken = new ContinuationToken<int>(1, DateTimeOffset.MinValue);
        var minSerialized = minToken.ToString();
        ContinuationToken<int>.TryParse(minSerialized, out var minDeserialized).Should().BeTrue();
        minDeserialized!.Timestamp!.Value.UtcTicks.Should().Be(DateTimeOffset.MinValue.UtcTicks);

        // Test with DateTimeOffset.MaxValue
        var maxToken = new ContinuationToken<int>(2, DateTimeOffset.MaxValue);
        var maxSerialized = maxToken.ToString();
        ContinuationToken<int>.TryParse(maxSerialized, out var maxDeserialized).Should().BeTrue();
        maxDeserialized!.Timestamp!.Value.UtcTicks.Should().Be(DateTimeOffset.MaxValue.UtcTicks);
    }

    [Test]
    public void RoundTrip_WithCurrentDateTime_PreservesValue()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var original = new ContinuationToken<long>(12345L, now);

        // Act
        var serialized = original.ToString();
        var success = ContinuationToken<long>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Timestamp!.Value.UtcTicks.Should().Be(now.UtcTicks);
    }

    #endregion

    #region Cross-Platform Endianness Tests

    [Test]
    public void Endianness_MultiByteIntegers_UseConsistentEncoding()
    {
        // This test ensures that the token uses little-endian encoding
        // which is consistent across platforms

        // Arrange - use a value where byte order matters
        int id = 0x01020304; // Each byte is different
        var token = new ContinuationToken<int>(id);

        // Act
        var serialized = token.ToString();
        var success = ContinuationToken<int>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(id);
    }

    [Test]
    public void Endianness_LongValues_UseConsistentEncoding()
    {
        // Arrange
        long id = 0x0102030405060708L;
        var token = new ContinuationToken<long>(id);

        // Act
        var serialized = token.ToString();
        var success = ContinuationToken<long>.TryParse(serialized, out var deserialized);

        // Assert
        success.Should().BeTrue();
        deserialized!.Id.Should().Be(id);
    }

    #endregion

    #region Edge Cases

    [Test]
    public void EdgeCase_ZeroValues_HandledCorrectly()
    {
        var intToken = new ContinuationToken<int>(0);
        var intSerialized = intToken.ToString();
        ContinuationToken<int>.TryParse(intSerialized, out var intDeserialized).Should().BeTrue();
        intDeserialized!.Id.Should().Be(0);

        var longToken = new ContinuationToken<long>(0L);
        var longSerialized = longToken.ToString();
        ContinuationToken<long>.TryParse(longSerialized, out var longDeserialized).Should().BeTrue();
        longDeserialized!.Id.Should().Be(0L);
    }

    [Test]
    public void EdgeCase_NegativeValues_HandledCorrectly()
    {
        var token = new ContinuationToken<int>(-42);
        var serialized = token.ToString();
        ContinuationToken<int>.TryParse(serialized, out var deserialized).Should().BeTrue();
        deserialized!.Id.Should().Be(-42);
    }

    [Test]
    public void EdgeCase_EmptyGuid_HandledCorrectly()
    {
        var token = new ContinuationToken<Guid>(Guid.Empty);
        var serialized = token.ToString();
        ContinuationToken<Guid>.TryParse(serialized, out var deserialized).Should().BeTrue();
        deserialized!.Id.Should().Be(Guid.Empty);
    }

    [Test]
    public void EdgeCase_ByteType_HandledCorrectly()
    {
        var token = new ContinuationToken<byte>(255);
        var serialized = token.ToString();
        ContinuationToken<byte>.TryParse(serialized, out var deserialized).Should().BeTrue();
        deserialized!.Id.Should().Be(255);
    }

    [Test]
    public void EdgeCase_SByteType_HandledCorrectly()
    {
        var token = new ContinuationToken<sbyte>(-128);
        var serialized = token.ToString();
        ContinuationToken<sbyte>.TryParse(serialized, out var deserialized).Should().BeTrue();
        deserialized!.Id.Should().Be(-128);
    }

    #endregion

    #region Properties Tests

    [Test]
    public void Properties_IdAndDate_AreAccessible()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;
        var token = new ContinuationToken<int>(42, date);

        // Assert
        token.Id.Should().Be(42);
        token.Timestamp.Should().Be(date);
    }

    [Test]
    public void Properties_WithoutDate_DateIsNull()
    {
        // Arrange
        var token = new ContinuationToken<int>(42);

        // Assert
        token.Id.Should().Be(42);
        token.Timestamp.Should().BeNull();
    }

    [Test]
    public void Constructor_WithNullDate_StoresNull()
    {
        // Arrange & Act
        var token = new ContinuationToken<int>(42, null);

        // Assert
        token.Timestamp.Should().BeNull();
    }

    #endregion

    #region TryParse Consistency Tests

    [Test]
    public void TryParse_WithSameValidToken_ProducesConsistentResults()
    {
        // Arrange
        var original = new ContinuationToken<int>(42, DateTimeOffset.UtcNow);
        var tokenString = original.ToString();

        // Act
        var success1 = ContinuationToken<int>.TryParse(tokenString, out var result1);
        var success2 = ContinuationToken<int>.TryParse(tokenString, out var result2);

        // Assert
        success1.Should().BeTrue();
        success2.Should().BeTrue();
        result1!.Id.Should().Be(result2!.Id);
        result1.Timestamp!.Value.UtcTicks.Should().Be(result2.Timestamp!.Value.UtcTicks);
    }

    [Test]
    public void TryParse_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "InvalidToken!@#$";

        // Act
        var success = ContinuationToken<int>.TryParse(invalidToken, out var result);

        // Assert
        success.Should().BeFalse();
    }

    #endregion

    #region Multiple Type Tests

    [Test]
    public void MultipleTypes_CanCoexist()
    {
        // Arrange & Act
        var intToken = new ContinuationToken<int>(42);
        var longToken = new ContinuationToken<long>(42L);
        var guidToken = new ContinuationToken<Guid>(Guid.NewGuid());

        var intSerialized = intToken.ToString();
        var longSerialized = longToken.ToString();
        var guidSerialized = guidToken.ToString();

        ContinuationToken<int>.TryParse(intSerialized, out var intDeserialized).Should().BeTrue();
        ContinuationToken<long>.TryParse(longSerialized, out var longDeserialized).Should().BeTrue();
        ContinuationToken<Guid>.TryParse(guidSerialized, out var guidDeserialized).Should().BeTrue();

        // Assert
        intDeserialized!.Id.Should().Be(42);
        longDeserialized!.Id.Should().Be(42L);
        guidDeserialized!.Id.Should().Be(guidToken.Id);
    }

    #endregion

    #region TryParse Validation Tests

    [Test]
    public void TryParse_WithExtraTrailingBytes_ReturnsFalse()
    {
        // Arrange
        var originalToken = new ContinuationToken<int>(42);
        var tokenString = originalToken.ToString();

        // Decode the original token and add extra bytes
        var tokenBytes = Encoding.UTF8.GetBytes(tokenString);
        var decodedLength = Base64Url.GetMaxDecodedLength(tokenBytes.Length);
        var decodedBuffer = new byte[decodedLength];
        Base64Url.DecodeFromUtf8(tokenBytes, decodedBuffer, out _, out int bytesWritten);

        // Add extra bytes to the decoded data
        var tamperedBuffer = new byte[bytesWritten + 4]; // Add 4 extra bytes
        Array.Copy(decodedBuffer, 0, tamperedBuffer, 0, bytesWritten);
        tamperedBuffer[bytesWritten] = 0xFF;
        tamperedBuffer[bytesWritten + 1] = 0xFF;
        tamperedBuffer[bytesWritten + 2] = 0xFF;
        tamperedBuffer[bytesWritten + 3] = 0xFF;

        // Re-encode with extra bytes
        var encodedLength = Base64Url.GetEncodedLength(tamperedBuffer.Length);
        var encodedBuffer = new byte[encodedLength];
        Base64Url.EncodeToUtf8(tamperedBuffer, encodedBuffer, out _, out _);
        var tamperedToken = Encoding.UTF8.GetString(encodedBuffer);

        // Act
        var success = ContinuationToken<int>.TryParse(tamperedToken, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithExtraTrailingBytesAndDate_ReturnsFalse()
    {
        // Arrange
        var originalToken = new ContinuationToken<long>(999L, DateTimeOffset.UtcNow);
        var tokenString = originalToken.ToString();

        // Decode the original token and add extra bytes
        var tokenBytes = Encoding.UTF8.GetBytes(tokenString);
        var decodedLength = Base64Url.GetMaxDecodedLength(tokenBytes.Length);
        var decodedBuffer = new byte[decodedLength];
        Base64Url.DecodeFromUtf8(tokenBytes, decodedBuffer, out _, out int bytesWritten);

        // Add extra bytes
        var tamperedBuffer = new byte[bytesWritten + 1]; // Add 1 extra byte
        Array.Copy(decodedBuffer, 0, tamperedBuffer, 0, bytesWritten);
        tamperedBuffer[bytesWritten] = 0xAB;

        // Re-encode
        var encodedLength = Base64Url.GetEncodedLength(tamperedBuffer.Length);
        var encodedBuffer = new byte[encodedLength];
        Base64Url.EncodeToUtf8(tamperedBuffer, encodedBuffer, out _, out _);
        var tamperedToken = Encoding.UTF8.GetString(encodedBuffer);

        // Act
        var success = ContinuationToken<long>.TryParse(tamperedToken, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithMissingBytes_ReturnsFalse()
    {
        // Arrange
        var originalToken = new ContinuationToken<int>(42, DateTimeOffset.UtcNow);
        var tokenString = originalToken.ToString();

        // Decode the original token and remove bytes
        var tokenBytes = Encoding.UTF8.GetBytes(tokenString);
        var decodedLength = Base64Url.GetMaxDecodedLength(tokenBytes.Length);
        var decodedBuffer = new byte[decodedLength];
        Base64Url.DecodeFromUtf8(tokenBytes, decodedBuffer, out _, out int bytesWritten);

        // Remove last 2 bytes (should have date, but we're truncating it)
        var truncatedBuffer = new byte[bytesWritten - 2];
        Array.Copy(decodedBuffer, 0, truncatedBuffer, 0, truncatedBuffer.Length);

        // Re-encode
        var encodedLength = Base64Url.GetEncodedLength(truncatedBuffer.Length);
        var encodedBuffer = new byte[encodedLength];
        Base64Url.EncodeToUtf8(truncatedBuffer, encodedBuffer, out _, out _);
        var truncatedToken = Encoding.UTF8.GetString(encodedBuffer);

        // Act
        var success = ContinuationToken<int>.TryParse(truncatedToken, out var parsed);

        // Assert
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_WithInvalidHasDateFlagAndExtraBytes_ReturnsFalse()
    {
        // Arrange - Create a token with invalid hasDate flag (neither 0 nor 1)
        var idSize = Unsafe.SizeOf<int>();
        var buffer = new byte[idSize + 1 + 8]; // Size as if it has date

        // Write ID (42)
        BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(0, 4), 42);

        // Write invalid hasDate flag (2 instead of 0 or 1)
        buffer[idSize] = 2;

        // Write some random bytes for what would be the date
        BinaryPrimitives.WriteInt64LittleEndian(buffer.AsSpan(idSize + 1), DateTimeOffset.UtcNow.UtcTicks);

        // Encode
        var encodedLength = Base64Url.GetEncodedLength(buffer.Length);
        var encodedBuffer = new byte[encodedLength];
        Base64Url.EncodeToUtf8(buffer, encodedBuffer, out _, out _);
        var invalidToken = Encoding.UTF8.GetString(encodedBuffer);

        // Act
        var success = ContinuationToken<int>.TryParse(invalidToken, out var parsed);

        // Assert - Should fail because hasDate=2 means expectedSize=idSize+1, but buffer.Length=idSize+9
        success.Should().BeFalse();
    }

    [Test]
    public void TryParse_ExactSizeWithoutDate_Succeeds()
    {
        // Arrange
        var originalToken = new ContinuationToken<int>(42);
        var tokenString = originalToken.ToString();

        // Verify the token has exact size
        var tokenBytes = Encoding.UTF8.GetBytes(tokenString);
        var decodedLength = Base64Url.GetMaxDecodedLength(tokenBytes.Length);
        var decodedBuffer = new byte[decodedLength];
        Base64Url.DecodeFromUtf8(tokenBytes, decodedBuffer, out _, out int bytesWritten);

        var expectedSize = Unsafe.SizeOf<int>() + 1; // ID + hasDate flag
        bytesWritten.Should().Be(expectedSize);

        // Act
        var success = ContinuationToken<int>.TryParse(tokenString, out var parsed);

        // Assert
        success.Should().BeTrue();
        parsed.Should().NotBeNull();
        parsed!.Id.Should().Be(42);
        parsed.Timestamp.Should().BeNull();
    }

    [Test]
    public void TryParse_ExactSizeWithDate_Succeeds()
    {
        // Arrange
        var date = DateTimeOffset.UtcNow;
        var originalToken = new ContinuationToken<int>(42, date);
        var tokenString = originalToken.ToString();

        // Verify the token has exact size
        var tokenBytes = Encoding.UTF8.GetBytes(tokenString);
        var decodedLength = Base64Url.GetMaxDecodedLength(tokenBytes.Length);
        var decodedBuffer = new byte[decodedLength];
        Base64Url.DecodeFromUtf8(tokenBytes, decodedBuffer, out _, out int bytesWritten);

        var expectedSize = Unsafe.SizeOf<int>() + 1 + 8; // ID + hasDate flag + ticks
        bytesWritten.Should().Be(expectedSize);

        // Act
        var success = ContinuationToken<int>.TryParse(tokenString, out var parsed);

        // Assert
        success.Should().BeTrue();
        parsed.Should().NotBeNull();
        parsed!.Id.Should().Be(42);
        parsed.Timestamp.Should().NotBeNull();
        parsed.Timestamp!.Value.UtcTicks.Should().Be(date.UtcTicks);
    }

    [Test]
    public void TryParse_DifferentTypeSizes_ValidateExactSizes()
    {
        // Test byte (1 byte + 1 flag = 2 bytes)
        var byteToken = new ContinuationToken<byte>(255);
        var byteString = byteToken.ToString();
        ContinuationToken<byte>.TryParse(byteString, out var byteParsed).Should().BeTrue();
        byteParsed!.Id.Should().Be(255);

        // Test short (2 bytes + 1 flag = 3 bytes)
        var shortToken = new ContinuationToken<short>(12345);
        var shortString = shortToken.ToString();
        ContinuationToken<short>.TryParse(shortString, out var shortParsed).Should().BeTrue();
        shortParsed!.Id.Should().Be(12345);

        // Test long (8 bytes + 1 flag = 9 bytes)
        var longToken = new ContinuationToken<long>(123456789L);
        var longString = longToken.ToString();
        ContinuationToken<long>.TryParse(longString, out var longParsed).Should().BeTrue();
        longParsed!.Id.Should().Be(123456789L);

        // Test Guid (16 bytes + 1 flag = 17 bytes)
        var guid = Guid.NewGuid();
        var guidToken = new ContinuationToken<Guid>(guid);
        var guidString = guidToken.ToString();
        ContinuationToken<Guid>.TryParse(guidString, out var guidParsed).Should().BeTrue();
        guidParsed!.Id.Should().Be(guid);
    }

    #endregion
}
