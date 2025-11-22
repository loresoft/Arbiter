// Ignore Spelling: Aes

using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// Provides AES-256-CBC encryption and decryption functionality with password-based key derivation.
/// </summary>
/// <remarks>
/// This class uses AES-256-CBC encryption with PKCS7 padding, which is a standard configuration
/// compatible across multiple programming languages and platforms. Each encryption operation
/// generates a unique salt and initialization vector (IV) for enhanced security.
/// </remarks>
public static class AesEncryption
{
    // AES-256-CBC with PKCS7 padding (standard across languages)
    private const int KeySize = 256;
    private const int IvSize = 16; // 128 bits
    private const int SaltSize = 16; // 128 bits
    private const int PasswordSize = 32; // 256 bits
    private const int Iterations = 10000; // PBKDF2 iterations for key derivation

    /// <summary>
    /// Generates a cryptographically secure random password, encoded as a Base64Url string.
    /// </summary>
    /// <param name="passwordSize">The size of the password in bytes. Defaults to 32 bytes (256 bits). Must be greater than zero.</param>
    /// <returns>A Base64Url-encoded string representing the cryptographically secure random password.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="passwordSize"/> is zero or negative.</exception>
    /// <remarks>
    /// A minimum size of 32 bytes (256 bits) is recommended for security. The generated password
    /// uses <see cref="RandomNumberGenerator"/> to ensure cryptographic randomness and is encoded
    /// using Base64Url format for safe transmission and storage.
    /// </remarks>
    public static string GeneratePassword(int passwordSize = PasswordSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(passwordSize);

        Span<byte> keyBytes = stackalloc byte[passwordSize];
        RandomNumberGenerator.Fill(keyBytes);

        return Base64Url.EncodeToString(keyBytes);
    }

    /// <summary>
    /// Encrypts a string using AES-256-CBC encryption with password-based key derivation.
    /// </summary>
    /// <param name="plainText">The text to encrypt. Cannot be null or empty.</param>
    /// <param name="password">The password used for encryption. Cannot be null or empty.</param>
    /// <returns>
    /// A Base64-encoded string containing the salt, initialization vector (IV), and encrypted data.
    /// The format is: [Salt (16 bytes)][IV (16 bytes)][Encrypted Data].
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="plainText"/> or <paramref name="password"/> is null or empty.</exception>
    /// <remarks>
    /// <para>
    /// This method generates a unique random salt and IV for each encryption operation, ensuring
    /// that identical plaintext encrypted with the same password will produce different ciphertext.
    /// </para>
    /// <para>
    /// The password is converted to a 256-bit encryption key using PBKDF2 (Password-Based Key
    /// Derivation Function 2) with 10,000 iterations and SHA-256 hashing.
    /// </para>
    /// <para>
    /// The method uses stack allocation and spans for improved performance and reduced heap allocations.
    /// </para>
    /// </remarks>
    public static string Encrypt(string plainText, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);
        ArgumentException.ThrowIfNullOrEmpty(password);

        // Generate random salt for each encryption
        Span<byte> salt = stackalloc byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        // Derive key from password using PBKDF2
        Span<byte> key = stackalloc byte[KeySize / 8];
        Rfc2898DeriveBytes.Pbkdf2(password, salt, key, Iterations, HashAlgorithmName.SHA256);

        using Aes aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key.ToArray(); // Aes.Key requires byte[]
        aes.GenerateIV(); // Random IV for each encryption

        // Convert plain text to bytes
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(plainText.Length);
        byte[] plainBytes = new byte[maxByteCount];

        int plainBytesWritten = Encoding.UTF8.GetBytes(plainText, plainBytes);
        Span<byte> plainTextSpan = plainBytes.AsSpan(0, plainBytesWritten);

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        using MemoryStream ms = new();

        // Write salt at the beginning
        ms.Write(salt);

        // Write IV after salt
        ms.Write(aes.IV);

        //// ensure to write encrypted data and dispose before converting to base64
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(plainTextSpan);
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Decrypts an AES-256-CBC encrypted string using password-based key derivation.
    /// </summary>
    /// <param name="cipherText">
    /// The Base64-encoded encrypted data containing the salt, IV, and encrypted content.
    /// Cannot be null or empty.
    /// </param>
    /// <param name="password">The password used for decryption. Must match the password used during encryption. Cannot be null or empty.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cipherText"/> or <paramref name="password"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown when <paramref name="cipherText"/> is not a valid Base64 string.</exception>
    /// <exception cref="CryptographicException">
    /// Thrown when decryption fails, which may occur if:
    /// <list type="bullet">
    /// <item><description>The password is incorrect</description></item>
    /// <item><description>The ciphertext has been tampered with or corrupted</description></item>
    /// <item><description>The ciphertext format is invalid</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method extracts the salt and IV from the beginning of the ciphertext, derives the
    /// decryption key using PBKDF2 with the same parameters as encryption (10,000 iterations, SHA-256),
    /// and then decrypts the data.
    /// </para>
    /// <para>
    /// The expected format of the ciphertext is: [Salt (16 bytes)][IV (16 bytes)][Encrypted Data].
    /// </para>
    /// <para>
    /// The method uses stack allocation and spans for improved performance and reduced heap allocations.
    /// </para>
    /// </remarks>
    public static string Decrypt(string cipherText, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);
        ArgumentException.ThrowIfNullOrEmpty(password);

        ReadOnlySpan<byte> fullCipherSpan = Convert.FromBase64String(cipherText);

        // Extract salt from the beginning
        Span<byte> salt = stackalloc byte[SaltSize];
        fullCipherSpan[..SaltSize].CopyTo(salt);

        // Derive key from password using PBKDF2
        Span<byte> key = stackalloc byte[KeySize / 8];
        Rfc2898DeriveBytes.Pbkdf2(password, salt, key, Iterations, HashAlgorithmName.SHA256);

        // Extract IV (after salt)
        Span<byte> iv = stackalloc byte[IvSize];
        fullCipherSpan.Slice(SaltSize, IvSize).CopyTo(iv);

        using Aes aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key.ToArray(); // Aes.Key requires byte[]
        aes.IV = iv.ToArray(); // Aes.IV requires byte[]

        // Get encrypted data (everything after salt + IV)
        ReadOnlySpan<byte> cipherBytes = fullCipherSpan[(SaltSize + IvSize)..];

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        using MemoryStream ms = new(cipherBytes.ToArray());
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs, Encoding.UTF8);

        return sr.ReadToEnd();
    }
}
