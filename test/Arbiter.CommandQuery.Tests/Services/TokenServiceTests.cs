using System.Buffers.Text;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Tests.Services;

public class TokenServiceTests
{
    [Test]
    public void GenerateKeyReturnsBase64UrlStringOfCorrectLength()
    {
        // Act
        string key = TokenService.GenerateKey();

        // Assert
        key.Should().NotBeNullOrWhiteSpace();
        key.Length.Should().BeInRange(43, 44); // 32 bytes in base64url is 43 or 44 chars (no padding)
    }

    [Test]
    public void CreateTokenAndValidateTokenSuccess()
    {
        // Arrange
        const string subject = "test-user";
        string secretKey = TokenService.GenerateKey();
        TimeSpan lifetime = TimeSpan.FromMinutes(10);

        // Act
        string token = TokenService.CreateToken(secretKey, subject, lifetime);
        bool valid = TokenService.ValidateToken(secretKey, token, out var extractedSubject);

        // Assert
        valid.Should().BeTrue();
        extractedSubject.Should().Be(subject);
    }

    [Test]
    public void ValidateTokenReturnsFalseForInvalidSignature()
    {
        // Arrange
        const string subject = "test-user";
        string secretKey = TokenService.GenerateKey();
        string token = TokenService.CreateToken(secretKey, subject);
        string otherKey = TokenService.GenerateKey();

        // Act
        bool valid = TokenService.ValidateToken(otherKey, token, out var extractedSubject);

        // Assert
        valid.Should().BeFalse();
        extractedSubject.Should().BeNull();
    }

    [Test]
    public void ValidateTokenReturnsFalseForExpiredToken()
    {
        // Arrange
        const string subject = "test-user";
        string secretKey = TokenService.GenerateKey();

        // Token expired 1 hour ago
        string token = TokenService.CreateToken(secretKey, subject, TimeSpan.FromHours(-1));

        // Act
        bool valid = TokenService.ValidateToken(secretKey, token, out var extractedSubject);

        // Assert
        valid.Should().BeFalse();
    }

    [Test]
    public void ValidateTokenReturnsFalseIfPayloadIsTampered()
    {
        // Arrange
        string secretKey = TokenService.GenerateKey();
        string subject = "original-user";
        string token = TokenService.CreateToken(secretKey, subject);

        // Split the token into header, payload, signature
        var parts = token.Split('.');
        parts.Length.Should().Be(3);

        // Decode, modify payload, and re-encode
        var payloadBytes = Base64Url.DecodeFromChars(parts[1]);
        payloadBytes.Should().NotBeNull();

        // Change the subject in the payload JSON
        var json = System.Text.Encoding.UTF8.GetString(payloadBytes!);
        // Replace the subject value (assumes "sub":"original-user" exists)
        var tamperedJson = json.Replace(subject, "tampered-user");
        var tamperedPayloadBytes = System.Text.Encoding.UTF8.GetBytes(tamperedJson);
        var tamperedPayload = Base64Url.EncodeToString(tamperedPayloadBytes);

        // Reconstruct the token with the tampered payload but original signature
        string tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

        // Act
        bool isValid = TokenService.ValidateToken(secretKey, tamperedToken, out var extractedSubject);

        // Assert
        isValid.Should().BeFalse();
        extractedSubject.Should().BeNull();
    }

    [Test]
    public void CreateTokenWithoutSubjectValidates()
    {
        // Arrange
        string secretKey = TokenService.GenerateKey();
        // No subject
        string token = TokenService.CreateToken(secretKey, null, TimeSpan.FromMinutes(5));

        // Act
        bool valid = TokenService.ValidateToken(secretKey, token, out var extractedSubject);

        // Assert
        valid.Should().BeTrue();
        extractedSubject.Should().BeNull();
    }

    [Test]
    public void GenerateKeyThrowsOnZeroOrNegativeSize()
    {
        // Act & Assert
        Action zero = () => TokenService.GenerateKey(0);
        Action negative = () => TokenService.GenerateKey(-1);

        zero.Should().Throw<ArgumentOutOfRangeException>();
        negative.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CreateTokenThrowsOnNullOrEmptySecret()
    {
        // Act & Assert
        Action nullSecret = () => TokenService.CreateToken(null!);
        Action emptySecret = () => TokenService.CreateToken("");
        Action whitespaceSecret = () => TokenService.CreateToken("   ");

        nullSecret.Should().Throw<ArgumentException>();
        emptySecret.Should().Throw<ArgumentException>();
        whitespaceSecret.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ValidateTokenThrowsOnNullOrEmptySecretOrToken()
    {
        string validKey = TokenService.GenerateKey();
        string validToken = TokenService.CreateToken(validKey, "user");

        // Act & Assert
        Action nullSecret = () => TokenService.ValidateToken(null!, validToken, out _);
        Action emptySecret = () => TokenService.ValidateToken("", validToken, out _);
        Action whitespaceSecret = () => TokenService.ValidateToken("   ", validToken, out _);
        Action nullToken = () => TokenService.ValidateToken(validKey, null!, out _);
        Action emptyToken = () => TokenService.ValidateToken(validKey, "", out _);
        Action whitespaceToken = () => TokenService.ValidateToken(validKey, "   ", out _);

        nullSecret.Should().Throw<ArgumentException>();
        emptySecret.Should().Throw<ArgumentException>();
        whitespaceSecret.Should().Throw<ArgumentException>();
        nullToken.Should().Throw<ArgumentException>();
        emptyToken.Should().Throw<ArgumentException>();
        whitespaceToken.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ReadTokenExtractsExpirationAndSubject()
    {
        // Arrange
        string secretKey = TokenService.GenerateKey();
        const string subject = "read-token-user";
        TimeSpan lifetime = TimeSpan.FromMinutes(30);
        string token = TokenService.CreateToken(secretKey, subject, lifetime);

        // Act
        bool result = TokenService.ReadToken(token, out var expiration, out var extractedSubject);

        // Assert
        result.Should().BeTrue();
        extractedSubject.Should().Be(subject);
        expiration.Should().NotBeNull();
        expiration!.Value.Should().BeGreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    [Test]
    public void ReadTokenReturnsFalseForMalformedToken()
    {
        // Arrange
        const string malformedToken = "not.a.valid.token";

        // Act
        Action malformedTokenAction = () => TokenService.ReadToken(malformedToken, out _, out _);

        // Assert
        malformedTokenAction.Should().Throw<FormatException>();
    }

    [Test]
    public void ReadTokenThrowsOnNullOrEmptyToken()
    {
        // Act & Assert
        Action nullToken = () => TokenService.ReadToken(null!, out _, out _);
        Action emptyToken = () => TokenService.ReadToken("", out _, out _);
        Action whitespaceToken = () => TokenService.ReadToken("   ", out _, out _);

        nullToken.Should().Throw<ArgumentException>();
        emptyToken.Should().Throw<ArgumentException>();
        whitespaceToken.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ValidateSignatureReturnsTrueForValidSignature()
    {
        // Arrange
        string secretKey = TokenService.GenerateKey();
        const string subject = "sig-user";
        string token = TokenService.CreateToken(secretKey, subject);

        // Act
        bool isValid = TokenService.ValidateSignature(secretKey, token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateSignatureReturnsFalseForInvalidSignature()
    {
        // Arrange
        string secretKey = TokenService.GenerateKey();
        const string subject = "sig-user";
        string token = TokenService.CreateToken(secretKey, subject);
        string otherKey = TokenService.GenerateKey();

        // Act
        bool isValid = TokenService.ValidateSignature(otherKey, token);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void ValidateSignatureThrowsOnNullOrEmptySecretOrToken()
    {
        string validKey = TokenService.GenerateKey();
        string validToken = TokenService.CreateToken(validKey, "user");

        // Act & Assert
        Action nullSecret = () => TokenService.ValidateSignature(null!, validToken);
        Action emptySecret = () => TokenService.ValidateSignature("", validToken);
        Action whitespaceSecret = () => TokenService.ValidateSignature("   ", validToken);
        Action nullToken = () => TokenService.ValidateSignature(validKey, null!);
        Action emptyToken = () => TokenService.ValidateSignature(validKey, "");
        Action whitespaceToken = () => TokenService.ValidateSignature(validKey, "   ");

        nullSecret.Should().Throw<ArgumentException>();
        emptySecret.Should().Throw<ArgumentException>();
        whitespaceSecret.Should().Throw<ArgumentException>();
        nullToken.Should().Throw<ArgumentException>();
        emptyToken.Should().Throw<ArgumentException>();
        whitespaceToken.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ValidateSignatureReturnsFalseIfPayloadIsTampered()
    {
        // Arrange
        string secretKey = TokenService.GenerateKey();
        string subject = "original-user";
        string token = TokenService.CreateToken(secretKey, subject);

        // Split the token into header, payload, signature
        var parts = token.Split('.');
        parts.Length.Should().Be(3);

        // Decode, modify payload, and re-encode
        var payloadBytes = Base64Url.DecodeFromChars(parts[1]);
        payloadBytes.Should().NotBeNull();

        // Change the subject in the payload JSON
        var json = System.Text.Encoding.UTF8.GetString(payloadBytes!);
        // Replace the subject value (assumes "sub":"original-user" exists)
        var tamperedJson = json.Replace(subject, "tampered-user");
        var tamperedPayloadBytes = System.Text.Encoding.UTF8.GetBytes(tamperedJson);
        var tamperedPayload = Base64Url.EncodeToString(tamperedPayloadBytes);

        // Reconstruct the token with the tampered payload but original signature
        string tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

        // Act
        bool isValid = TokenService.ValidateSignature(secretKey, tamperedToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void ValidateExpirationReturnsTrueIfExpirationIsInFuture()
    {
        // Arrange
        long futureExpiration = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

        // Act
        bool isValid = TokenService.ValidateExpiration(futureExpiration);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateExpirationReturnsFalseIfExpirationIsInPast()
    {
        // Arrange
        long pastExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();

        // Act
        bool isValid = TokenService.ValidateExpiration(pastExpiration);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    public void ValidateExpirationReturnsFalseIfExpirationIsNull()
    {
        // Act
        bool isValid = TokenService.ValidateExpiration(null);

        // Assert
        isValid.Should().BeFalse();
    }
}
