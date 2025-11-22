/**
 * Provides AES-256-CBC encryption and decryption functionality with password-based key derivation.
 * 
 * This module uses AES-256-CBC encryption with PKCS7 padding, which is a standard configuration
 * compatible across multiple programming languages and platforms. Each encryption operation
 * generates a unique salt and initialization vector (IV) for enhanced security.
 * 
 * Compatible with the C# AesEncryption implementation in this project.
 */

const crypto = require('crypto');

/**
 * AES-256-CBC with PKCS7 padding (standard across languages)
 */
const KEY_SIZE = 256;
const IV_SIZE = 16; // 128 bits
const SALT_SIZE = 16; // 128 bits
const PASSWORD_SIZE = 32; // 256 bits
const PBKDF2_ITERATIONS = 10000;
const PBKDF2_DIGEST = 'sha256';

/**
 * Generates a cryptographically secure random password, encoded as a Base64Url string.
 * 
 * @param {number} [passwordSize=32] - The size of the password in bytes. Defaults to 32 bytes (256 bits). Must be greater than zero.
 * @returns {string} A Base64Url-encoded string representing the cryptographically secure random password.
 * @throws {RangeError} Thrown when passwordSize is zero or negative.
 * 
 * @remarks
 * A minimum size of 32 bytes (256 bits) is recommended for security. The generated password
 * uses cryptographically secure random bytes and is encoded using Base64Url format for safe
 * transmission and storage.
 * 
 * @example
 * const password = generatePassword();
 * console.log(password); // e.g., "kX7y9_zAb1cD2eF3gH4iJ5kL6mN7oP8qR9sT0uV1wX2"
 */
function generatePassword(passwordSize = PASSWORD_SIZE) {
    if (passwordSize <= 0) {
        throw new RangeError('passwordSize must be greater than zero');
    }

    const keyBytes = crypto.randomBytes(passwordSize);
    return base64UrlEncode(keyBytes);
}

/**
 * Encrypts a string using AES-256-CBC encryption with password-based key derivation.
 * 
 * @param {string} plainText - The text to encrypt. Cannot be null or empty.
 * @param {string} password - The password used for encryption. Cannot be null or empty.
 * @returns {string} A Base64-encoded string containing the salt, initialization vector (IV), and encrypted data.
 *                   The format is: [Salt (16 bytes)][IV (16 bytes)][Encrypted Data].
 * @throws {Error} Thrown when plainText or password is null or empty.
 * 
 * @remarks
 * This method generates a unique random salt and IV for each encryption operation, ensuring
 * that identical plaintext encrypted with the same password will produce different ciphertext.
 * 
 * The password is converted to a 256-bit encryption key using PBKDF2 (Password-Based Key
 * Derivation Function 2) with 10,000 iterations and SHA-256 hashing.
 * 
 * @example
 * const encrypted = encrypt('Hello, World!', 'SecurePassword123');
 * console.log(encrypted); // Base64-encoded string with salt, IV, and encrypted data
 */
function encrypt(plainText, password) {
    if (!plainText) {
        throw new Error('plainText cannot be null or empty');
    }
    if (!password) {
        throw new Error('password cannot be null or empty');
    }

    // Generate random salt for each encryption
    const salt = crypto.randomBytes(SALT_SIZE);

    // Derive key from password using PBKDF2
    const key = crypto.pbkdf2Sync(password, salt, PBKDF2_ITERATIONS, KEY_SIZE / 8, PBKDF2_DIGEST);

    // Generate random IV for each encryption
    const iv = crypto.randomBytes(IV_SIZE);

    // Create cipher
    const cipher = crypto.createCipheriv('aes-256-cbc', key, iv);

    // Encrypt the data
    let encrypted = cipher.update(plainText, 'utf8');
    encrypted = Buffer.concat([encrypted, cipher.final()]);

    // Combine salt + IV + encrypted data
    const result = Buffer.concat([salt, iv, encrypted]);

    // Return as Base64
    return result.toString('base64');
}

/**
 * Decrypts an AES-256-CBC encrypted string using password-based key derivation.
 * 
 * @param {string} cipherText - The Base64-encoded encrypted data containing the salt, IV, and encrypted content.
 *                              Cannot be null or empty.
 * @param {string} password - The password used for decryption. Must match the password used during encryption.
 *                           Cannot be null or empty.
 * @returns {string} The decrypted plaintext string.
 * @throws {Error} Thrown when cipherText or password is null or empty.
 * @throws {Error} Thrown when cipherText is not valid Base64.
 * @throws {Error} Thrown when decryption fails, which may occur if:
 *                 - The password is incorrect
 *                 - The ciphertext has been tampered with or corrupted
 *                 - The ciphertext format is invalid
 * 
 * @remarks
 * This method extracts the salt and IV from the beginning of the ciphertext, derives the
 * decryption key using PBKDF2 with the same parameters as encryption (10,000 iterations, SHA-256),
 * and then decrypts the data.
 * 
 * The expected format of the ciphertext is: [Salt (16 bytes)][IV (16 bytes)][Encrypted Data].
 * 
 * @example
 * const decrypted = decrypt(encrypted, 'SecurePassword123');
 * console.log(decrypted); // 'Hello, World!'
 */
function decrypt(cipherText, password) {
    if (!cipherText) {
        throw new Error('cipherText cannot be null or empty');
    }
    if (!password) {
        throw new Error('password cannot be null or empty');
    }

    // Decode from Base64
    let fullCipher;
    try {
        fullCipher = Buffer.from(cipherText, 'base64');
    } catch (error) {
        throw new Error('cipherText is not valid Base64: ' + error.message);
    }

    // Check minimum length
    if (fullCipher.length < SALT_SIZE + IV_SIZE) {
        throw new Error('cipherText is too short to contain salt and IV');
    }

    // Extract salt from the beginning
    const salt = fullCipher.slice(0, SALT_SIZE);

    // Derive key from password using PBKDF2
    const key = crypto.pbkdf2Sync(password, salt, PBKDF2_ITERATIONS, KEY_SIZE / 8, PBKDF2_DIGEST);

    // Extract IV (after salt)
    const iv = fullCipher.slice(SALT_SIZE, SALT_SIZE + IV_SIZE);

    // Get encrypted data (everything after salt + IV)
    const encrypted = fullCipher.slice(SALT_SIZE + IV_SIZE);

    // Create decipher
    const decipher = crypto.createDecipheriv('aes-256-cbc', key, iv);

    // Decrypt the data
    let decrypted = decipher.update(encrypted);
    decrypted = Buffer.concat([decrypted, decipher.final()]);

    // Return as UTF-8 string
    return decrypted.toString('utf8');
}

/**
 * Encodes a buffer to Base64Url format (URL-safe Base64).
 * 
 * @param {Buffer} buffer - The buffer to encode.
 * @returns {string} Base64Url-encoded string.
 * @private
 */
function base64UrlEncode(buffer) {
    return buffer.toString('base64')
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=/g, '');
}

/**
 * Decodes a Base64Url string to a buffer.
 * 
 * @param {string} str - The Base64Url-encoded string.
 * @returns {Buffer} Decoded buffer.
 * @private
 */
function base64UrlDecode(str) {
    // Add padding if needed
    let base64 = str.replace(/-/g, '+').replace(/_/g, '/');
    while (base64.length % 4) {
        base64 += '=';
    }
    return Buffer.from(base64, 'base64');
}

module.exports = {
    generatePassword,
    encrypt,
    decrypt
};
