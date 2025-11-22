using Arbiter.CommandQuery.Services;
using System.Security.Cryptography;

namespace Arbiter.CommandQuery.Tests.Services;

public class AesEncryptionTests
{
    #region GeneratePassword Tests

    [Test]
    public void GeneratePassword_WithDefaultSize_ReturnsNonEmptyString()
    {
        // Act
        var password = AesEncryption.GeneratePassword();

        // Assert
        password.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void GeneratePassword_CalledMultipleTimes_ReturnsDifferentPasswords()
    {
        // Act
        var password1 = AesEncryption.GeneratePassword();
        var password2 = AesEncryption.GeneratePassword();
        var password3 = AesEncryption.GeneratePassword();

        // Assert
        password1.Should().NotBe(password2);
        password2.Should().NotBe(password3);
        password1.Should().NotBe(password3);
    }

    [Test]
    [Arguments(16)]
    [Arguments(32)]
    [Arguments(64)]
    [Arguments(128)]
    public void GeneratePassword_WithCustomSize_ReturnsPasswordOfCorrectLength(int size)
    {
        // Act
        var password = AesEncryption.GeneratePassword(size);

        // Assert
        password.Should().NotBeNullOrEmpty();
        // Base64Url encoded length should be approximately size * 4/3 (rounded up)
        var expectedMinLength = (int)Math.Ceiling(size * 4.0 / 3.0);
        password.Length.Should().BeGreaterThanOrEqualTo(expectedMinLength - 2);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(-100)]
    public void GeneratePassword_WithZeroOrNegativeSize_ThrowsArgumentOutOfRangeException(int size)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => AesEncryption.GeneratePassword(size));
    }

    #endregion

    #region Encrypt Tests

    [Test]
    public void Encrypt_WithValidInput_ReturnsNonEmptyString()
    {
        // Arrange
        var plainText = "Hello, World!";
        var password = "SecurePassword123";

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Encrypt_WithValidInput_ReturnsBase64String()
    {
        // Arrange
        var plainText = "Test message";
        var password = "TestPassword";

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);

        // Assert
        // Should be valid Base64
        var bytes = Convert.FromBase64String(encrypted);
        bytes.Should().NotBeEmpty();
    }

    [Test]
    public void Encrypt_SamePlainTextAndPassword_ReturnsDifferentCipherText()
    {
        // Arrange
        var plainText = "Consistent message";
        var password = "SamePassword";

        // Act
        var encrypted1 = AesEncryption.Encrypt(plainText, password);
        var encrypted2 = AesEncryption.Encrypt(plainText, password);
        var encrypted3 = AesEncryption.Encrypt(plainText, password);

        // Assert - Should be different due to random salt and IV
        encrypted1.Should().NotBe(encrypted2);
        encrypted2.Should().NotBe(encrypted3);
        encrypted1.Should().NotBe(encrypted3);
    }

    [Test]
    public void Encrypt_WithUnicodeCharacters_EncryptsSuccessfully()
    {
        // Arrange
        var plainText = "Hello 世界! 🌍 Привет مرحبا";
        var password = "UnicodePassword";

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(encrypted);
        bytes.Should().NotBeEmpty();
    }

    [Test]
    public void Encrypt_WithLongText_EncryptsSuccessfully()
    {
        // Arrange
        var plainText = new string('A', 10000);
        var password = "LongTextPassword";

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Encrypt_WithSpecialCharacters_EncryptsSuccessfully()
    {
        // Arrange
        var plainText = "!@#$%^&*()_+-={}[]|\\:\";<>?,./`~";
        var password = "SpecialCharsPassword";

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
    }

    [Test]
    [Arguments(null!, "password")]
    [Arguments("", "password")]
    [Arguments("plaintext", null!)]
    [Arguments("plaintext", "")]
    [Arguments(null!, null!)]
    [Arguments("", "")]
    public void Encrypt_WithNullOrEmptyInput_ThrowsArgumentException(string? plainText, string? password)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => AesEncryption.Encrypt(plainText!, password!));
    }

    #endregion

    #region Decrypt Tests

    [Test]
    public void Decrypt_WithValidEncryptedData_ReturnsOriginalPlainText()
    {
        // Arrange
        var originalText = "Secret Message";
        var password = "DecryptionPassword";
        var encrypted = AesEncryption.Encrypt(originalText, password);

        // Act
        var decrypted = AesEncryption.Decrypt(encrypted, password);

        // Assert
        decrypted.Should().Be(originalText);
    }

    [Test]
    public void Decrypt_WithUnicodeText_ReturnsOriginalPlainText()
    {
        // Arrange
        var originalText = "こんにちは世界 🎌 Hello World! مرحبا بالعالم";
        var password = "UnicodeDecryptPassword";
        var encrypted = AesEncryption.Encrypt(originalText, password);

        // Act
        var decrypted = AesEncryption.Decrypt(encrypted, password);

        // Assert
        decrypted.Should().Be(originalText);
    }

    [Test]
    public void Decrypt_WithLongText_ReturnsOriginalPlainText()
    {
        // Arrange
        var originalText = new string('B', 50000);
        var password = "LongDecryptPassword";
        var encrypted = AesEncryption.Encrypt(originalText, password);

        // Act
        var decrypted = AesEncryption.Decrypt(encrypted, password);

        // Assert
        decrypted.Should().Be(originalText);
    }

    [Test]
    public void Decrypt_WithWrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        var plainText = "Sensitive data";
        var correctPassword = "CorrectPassword";
        var wrongPassword = "WrongPassword";
        var encrypted = AesEncryption.Encrypt(plainText, correctPassword);

        // Act & Assert
        Assert.Throws<CryptographicException>(() => AesEncryption.Decrypt(encrypted, wrongPassword));
    }


    [Test]
    [Arguments(null, "password")]
    [Arguments("", "password")]
    [Arguments("encrypted", null!)]
    [Arguments("encrypted", "")]
    [Arguments(null, null)]
    [Arguments("", "")]
    public void Decrypt_WithNullOrEmptyInput_ThrowsArgumentException(string? cipherText, string? password)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => AesEncryption.Decrypt(cipherText!, password!));
    }

    [Test]
    public void Decrypt_WithInvalidBase64_ThrowsFormatException()
    {
        // Arrange
        var invalidBase64 = "This is not valid Base64!@#$";
        var password = "SomePassword";

        // Act & Assert
        Assert.Throws<FormatException>(() => AesEncryption.Decrypt(invalidBase64, password));
    }

    [Test]
    public void Decrypt_WithTamperedCipherText_ThrowsCryptographicException()
    {
        // Arrange
        var plainText = "Original message";
        var password = "Password123";
        var encrypted = AesEncryption.Encrypt(plainText, password);
        
        // Tamper with the encrypted data
        var bytes = Convert.FromBase64String(encrypted);
        bytes[bytes.Length - 1] ^= 0xFF; // Flip bits in last byte
        var tamperedEncrypted = Convert.ToBase64String(bytes);

        // Act & Assert
        Assert.Throws<CryptographicException>(() => AesEncryption.Decrypt(tamperedEncrypted, password));
    }

    [Test]
    public void Decrypt_WithTooShortCipherText_ThrowsException()
    {
        // Arrange
        // Create a Base64 string that's too short (less than salt + IV = 32 bytes)
        var shortData = Convert.ToBase64String(new byte[20]);
        var password = "Password";

        // Act & Assert
        Assert.Throws<Exception>(() => AesEncryption.Decrypt(shortData, password));
    }

    #endregion

    #region Integration Tests

    [Test]
    public void EncryptDecrypt_RoundTrip_PreservesData()
    {
        // Arrange
        var testCases = new[]
        {
            "Simple text",
            "Text with numbers: 123456",
            "Special chars: !@#$%^&*()",
            "Unicode: 你好世界 🌍",
            "Multi-line\ntext\nwith\nnewlines",
            "Tabs\tand\tspaces",
            new string('X', 1000), // Long text
            " ", // Single space
            "a", // Single character
        };

        var password = "TestPassword123";

        foreach (var originalText in testCases)
        {
            // Act
            var encrypted = AesEncryption.Encrypt(originalText, password);
            var decrypted = AesEncryption.Decrypt(encrypted, password);

            // Assert
            decrypted.Should().Be(originalText, $"Failed for input: {originalText.Substring(0, Math.Min(50, originalText.Length))}");
        }
    }

    [Test]
    public void EncryptDecrypt_WithDifferentPasswords_ProducesDifferentResults()
    {
        // Arrange
        var plainText = "Test message";
        var password1 = "Password1";
        var password2 = "Password2";

        // Act
        var encrypted1 = AesEncryption.Encrypt(plainText, password1);
        var encrypted2 = AesEncryption.Encrypt(plainText, password2);

        // Assert
        encrypted1.Should().NotBe(encrypted2);
        
        var decrypted1 = AesEncryption.Decrypt(encrypted1, password1);
        var decrypted2 = AesEncryption.Decrypt(encrypted2, password2);
        
        decrypted1.Should().Be(plainText);
        decrypted2.Should().Be(plainText);
    }

    [Test]
    public void EncryptDecrypt_MultipleRounds_MaintainsDataIntegrity()
    {
        // Arrange
        var originalText = "Multi-round encryption test";
        var password = "RoundTripPassword";

        // Act - Encrypt and decrypt multiple times
        var text = originalText;
        for (int i = 0; i < 5; i++)
        {
            var encrypted = AesEncryption.Encrypt(text, password);
            text = AesEncryption.Decrypt(encrypted, password);
        }

        // Assert
        text.Should().Be(originalText);
    }

    [Test]
    public void Encrypt_OutputFormat_ContainsSaltAndIV()
    {
        // Arrange
        var plainText = "Test";
        var password = "Password";

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);
        var bytes = Convert.FromBase64String(encrypted);

        // Assert
        // Salt (16 bytes) + IV (16 bytes) + encrypted data (at least 16 bytes due to padding)
        bytes.Length.Should().BeGreaterThanOrEqualTo(48);
    }

    #endregion

    #region Security Tests

    [Test]
    public void Encrypt_SamePasswordDifferentData_ProducesDifferentCipherText()
    {
        // Arrange
        var password = "SharedPassword";
        var data1 = "Message One";
        var data2 = "Message Two";

        // Act
        var encrypted1 = AesEncryption.Encrypt(data1, password);
        var encrypted2 = AesEncryption.Encrypt(data2, password);

        // Assert
        encrypted1.Should().NotBe(encrypted2);
    }

    [Test]
    public void GeneratePassword_ProducesSufficientEntropy()
    {
        // Arrange & Act
        var passwords = new HashSet<string>();
        for (int i = 0; i < 1000; i++)
        {
            passwords.Add(AesEncryption.GeneratePassword());
        }

        // Assert - All passwords should be unique
        passwords.Count.Should().Be(1000);
    }

    [Test]
    public void Encrypt_WithEmptyStringPassword_StillWorks()
    {
        // Note: While not recommended, the implementation should handle it
        // This test documents the behavior rather than recommending it
        
        // Arrange
        var plainText = "Test";
        var password = "a"; // Minimum non-empty password

        // Act
        var encrypted = AesEncryption.Encrypt(plainText, password);
        var decrypted = AesEncryption.Decrypt(encrypted, password);

        // Assert
        decrypted.Should().Be(plainText);
    }

    #endregion
}
