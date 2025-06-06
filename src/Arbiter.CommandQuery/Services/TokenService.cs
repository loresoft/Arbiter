using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// Provides static methods for generating cryptographic keys, creating, and validating JSON Web Tokens (JWT) using HMAC SHA-256.
/// </summary>
/// <remarks>
/// This is a lightweight implementation of JWT for use in scenarios where you need to authenticate users or entities.
/// The payload of the token can include a subject (user ID or similar identifier) and an expiration time.
/// The token is signed using a secret key, which must be kept secure. Only HMAC SHA-256 is supported for signing tokens.
/// </remarks>
/// <example>
/// <code>
/// // Generate a new secret key
/// string secretKey = TokenService.GenerateKey();
///
/// // Create a token for a user
/// string token = TokenService.CreateToken(secretKey, "user123", TimeSpan.FromHours(12));
///
/// // Validate the token and extract the subject
/// if (TokenService.ValidateToken(secretKey, token, out var subject))
/// {
///     Console.WriteLine($"Token is valid for subject: {subject}");
/// }
/// else
/// {
///     Console.WriteLine("Token is invalid or expired.");
/// }
/// </code>
/// </example>
public static class TokenService
{
    private static readonly JsonEncodedText SubjectEncoded = JsonEncodedText.Encode("sub");
    private static readonly JsonEncodedText ExpirationEncoded = JsonEncodedText.Encode("exp");

    // header for JWT, using HS256 algorithm and JWT type.  hard-coded for simplicity as it doesn't change.
    private static ReadOnlySpan<byte> HeaderJson => "{\"alg\":\"HS256\",\"typ\":\"JWT\"}"u8;
    private static readonly string HeaderEncoded = Base64Url.EncodeToString(HeaderJson);

    /// <summary>
    /// The default key size, in bytes, for cryptographic operations.
    /// </summary>
    /// <remarks>
    /// This constant represents a 32 bytes (256 bits) key size, which is commonly used in cryptographic
    /// algorithms such as AES for secure encryption and decryption.
    /// </remarks>
    /// <seealso cref="GenerateKey(int)"/>
    public const int DefaultKeySize = 32;

    /// <summary>
    /// Represents the buffer time, in seconds, to account for clock skew in time-sensitive operations.
    /// </summary>
    /// <remarks>
    /// This constant is typically used to adjust for minor differences in system clocks when
    /// performing time-based validations or calculations, such as token expiration checks.
    /// </remarks>
    /// <seealso cref="ValidateToken(string, string, out string?)"/>
    public const int ClockSkewSeconds = 30;

    /// <summary>
    /// Generates a cryptographically secure random key, encoded as a Base64Url string.
    /// Recommend a minimum size of 32 bytes (256 bits) for security.
    /// </summary>
    /// <param name="keySize">The size of the key in bytes. Defaults to 32 (256 bits).</param>
    /// <returns>A Base64Url-encoded string representing the generated key.</returns>
    public static string GenerateKey(int keySize = DefaultKeySize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(keySize);

        Span<byte> keyBytes = stackalloc byte[keySize];
        RandomNumberGenerator.Fill(keyBytes);

        return Base64Url.EncodeToString(keyBytes);
    }

    /// <summary>
    /// Creates a JWT token for the specified subject, signed with the provided secret key.
    /// </summary>
    /// <param name="secretKey">The secret key used to sign the token.</param>
    /// <param name="subject">The subject (user or entity identifier) to include in the token.</param>
    /// <param name="lifetime">The token's lifetime. Defaults to 24 hours if not specified.</param>
    /// <returns>A signed JWT token as a string.</returns>
    /// <remarks>
    /// The payload of the token will include the subject (`sub`) and an expiration time (`exp`).
    /// The token signature is a Base64Url-encoded HMAC SHA-256 hash of the header and payload.
    /// </remarks>
    public static string CreateToken(
        string secretKey,
        string? subject = null,
        TimeSpan lifetime = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

        if (lifetime == default)
            lifetime = TimeSpan.FromHours(24);

        long expiration = DateTimeOffset.UtcNow
            .Add(lifetime)
            .ToUnixTimeSeconds();

        var payloadJson = WritePayload(expiration, subject);
        var encodedPayload = Base64Url.EncodeToString(payloadJson);

        var unsignedToken = $"{HeaderEncoded}.{encodedPayload}";
        var signatureHash = ComputeSignature(unsignedToken, secretKey);
        var signature = Base64Url.EncodeToString(signatureHash);

        // Combine header, payload, and signature into the final JWT token
        return $"{unsignedToken}.{signature}";
    }

    /// <summary>
    /// Extracts the payload from a JSON Web Token (JWT) and retrieves the expiration time and subject.  This does not validate the token's signature.
    /// </summary>
    /// <param name="token">The JWT string to parse. Must not be null, empty, or whitespace.</param>
    /// <param name="expiration">
    /// When this method returns, contains the expiration time (`exp`) of the token as a Unix timestamp,
    /// or <see langword="null"/> if the expiration time is not present in the payload.
    /// </param>
    /// <param name="subject">
    /// When this method returns, contains the subject (`sub`) of the token, or <see langword="null"/>
    /// if the subject is not present in the payload.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the payload was successfully extracted and decoded; otherwise,
    /// <see langword="false"/> if the token is invalid or the payload cannot be read.
    /// </returns>
    public static bool ReadToken(
        string token,
        out long? expiration,
        out string? subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        expiration = default;
        subject = default;

        ReadOnlySpan<char> tokenSpan = token;

        // locate '.' separators without allocation
        int firstDot = tokenSpan.IndexOf('.');
        if (firstDot < 0)
            return false;

        // start after the first dot to find the second dot
        int secondDot = tokenSpan[(firstDot + 1)..].IndexOf('.');
        if (secondDot < 0)
            return false;

        // adjust secondDot to account for the first part of the token
        secondDot += firstDot + 1;

        var payloadSpan = tokenSpan.Slice(firstDot + 1, secondDot - firstDot - 1);
        var payloadBytes = Base64Url.DecodeFromChars(payloadSpan);
        if (payloadBytes == null || payloadBytes.Length == 0)
            return false;

        return ReadPayload(payloadBytes, out expiration, out subject);
    }

    /// <summary>
    /// Validates a JWT token using the provided secret key and extracts the subject (`sub`) payload field if valid.
    /// The payload must contain an expiration time (`exp`) field for validation.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <param name="secretKey">The secret key used to validate the token.</param>
    /// <param name="subject">
    /// When this method returns, contains the subject from the token if validation succeeds; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the token is valid and not expired; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The token must be in the JWT format: `header.payload.signature`. The header and payload are Base64Url-encoded JSON objects,
    /// and the signature is a Base64Url-encoded HMAC SHA-256 hash of the header and payload.
    /// </remarks>
    public static bool ValidateToken(
        string secretKey,
        string token,
        out string? subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        subject = default;

        ReadOnlySpan<char> tokenSpan = token;

        // locate '.' separators without allocation
        int firstDot = tokenSpan.IndexOf('.');
        if (firstDot < 0)
            return false;

        // start after the first dot to find the second dot
        int secondDot = tokenSpan[(firstDot + 1)..].IndexOf('.');
        if (secondDot < 0)
            return false;

        // adjust secondDot to account for the first part of the token
        secondDot += firstDot + 1;

        // first 2 parts are header and payload (unsigned), last part is signature
        var unsignedSpan = tokenSpan[..secondDot];
        var payloadSpan = tokenSpan.Slice(firstDot + 1, secondDot - firstDot - 1);

        var signatureSpan = tokenSpan[(secondDot + 1)..];
        var signatureBytes = GetBytes(signatureSpan);

        var computedHash = ComputeSignature(unsignedSpan, secretKey);
        var computedBytes = Base64Url.EncodeToUtf8(computedHash);

        // compare signatures in a fixed-time manner to prevent timing attacks
        if (!CryptographicOperations.FixedTimeEquals(signatureBytes, computedBytes))
            return false;

        var payloadBytes = Base64Url.DecodeFromChars(payloadSpan);
        if (payloadBytes == null || payloadBytes.Length == 0)
            return false;

        if (!ReadPayload(payloadBytes, out long? expiration, out subject))
            return false;

        return ValidateExpiration(expiration);
    }

    /// <summary>
    /// Validates the signature of a token using the provided secret key. This method does not check the token's expiration or subject.
    /// </summary>
    /// <remarks>
    /// This method performs a fixed-time comparison of the token's signature to prevent timing
    /// attacks. The token must be in the format of three parts separated by periods ('.'), where the first two parts
    /// represent the unsigned header and payload, and the third part is the signature.
    /// </remarks>
    /// <param name="secretKey">The secret key used to compute the expected signature. This value cannot be null, empty, or consist only of whitespace.</param>
    /// <param name="token">The token to validate. This value cannot be null, empty, or consist only of whitespace.</param>
    /// <returns><see langword="true"/> if the token's signature matches the computed signature based on the secret key; otherwise, <see langword="false"/>.</returns>
    public static bool ValidateSignature(
        string secretKey,
        string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        ReadOnlySpan<char> tokenSpan = token;

        // locate '.' separators without allocation
        int firstDot = tokenSpan.IndexOf('.');
        if (firstDot < 0)
            return false;

        // start after the first dot to find the second dot
        int secondDot = tokenSpan[(firstDot + 1)..].IndexOf('.');
        if (secondDot < 0)
            return false;

        // adjust secondDot to account for the first part of the token
        secondDot += firstDot + 1;

        // first 2 parts are header and payload (unsigned), last part is signature
        var unsignedSpan = tokenSpan[..secondDot];

        var signatureSpan = tokenSpan[(secondDot + 1)..];
        var signatureBytes = GetBytes(signatureSpan);

        var computedHash = ComputeSignature(unsignedSpan, secretKey);
        var computedBytes = Base64Url.EncodeToUtf8(computedHash);

        // compare signatures in a fixed-time manner to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(signatureBytes, computedBytes);
    }

    /// <summary>
    /// Validates whether the specified expiration time has not yet passed, allowing for a small buffer to account for clock skew.
    /// </summary>
    /// <remarks>
    /// This method compares the current UTC time to the provided expiration time, with an additional buffer defined by
    /// the <c>ClockSkewSeconds</c> constant to account for potential clock synchronization differences.
    /// </remarks>
    /// <param name="expiration">The expiration time, represented as a Unix timestamp in seconds. Can be <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the current time is within the allowed range of the expiration time; otherwise, <see langword="false"/>.
    /// Returns <see langword="false"/> if <paramref name="expiration"/> is <see langword="null"/>.
    /// </returns>
    public static bool ValidateExpiration(long? expiration)
    {
        if (!expiration.HasValue)
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // allow a small buffer for clock skew
        return now <= expiration.Value + ClockSkewSeconds;
    }


    private static ReadOnlySpan<byte> ComputeSignature(
        ReadOnlySpan<char> data,
        ReadOnlySpan<char> key)
    {
        var keyBytes = GetBytes(key);
        var dataBytes = GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(dataBytes);
    }

    private static byte[] GetBytes(ReadOnlySpan<char> span)
    {
        int byteCount = Encoding.UTF8.GetByteCount(span);
        byte[] bytes = new byte[byteCount];

        Encoding.UTF8.GetBytes(span, bytes);

        return bytes;
    }

    private static ReadOnlySpan<byte> WritePayload(
        long expiration,
        string? subject)
    {
        ArrayBufferWriter<byte> bufferWriter = new();
        Utf8JsonWriter writer = new(bufferWriter);

        writer.WriteStartObject();

        if (!string.IsNullOrEmpty(subject))
            writer.WriteString(SubjectEncoded, subject);

        writer.WriteNumber(ExpirationEncoded, expiration);
        writer.WriteEndObject();

        writer.Flush();

        return bufferWriter.WrittenSpan;
    }

    private static bool ReadPayload(
        ReadOnlySpan<byte> payload,
        out long? expiration,
        out string? subject)
    {
        subject = default;
        expiration = default;

        Utf8JsonReader reader = new(payload);

        if (!reader.Read())
            return false;

        if (reader.TokenType != JsonTokenType.StartObject)
            return false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            // only simple properties are expected
            if (reader.TokenType != JsonTokenType.PropertyName)
                return false;

            if (reader.ValueTextEquals(SubjectEncoded.EncodedUtf8Bytes))
            {
                reader.Read(); //advance to property value
                subject = reader.GetString();
            }
            else if (reader.ValueTextEquals(ExpirationEncoded.EncodedUtf8Bytes))
            {
                reader.Read(); //advance to property value
                expiration = reader.GetInt64();
            }
            else
            {
                reader.Skip(); // skip unknown property
            }
        }

        return true;
    }
}
