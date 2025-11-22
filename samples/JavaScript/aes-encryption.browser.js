/**
 * Provides AES-256-CBC encryption and decryption functionality with password-based key derivation.
 * 
 * This module uses AES-256-CBC encryption with PKCS7 padding, which is a standard configuration
 * compatible across multiple programming languages and platforms. Each encryption operation
 * generates a unique salt and initialization vector (IV) for enhanced security.
 * 
 * Compatible with the C# AesEncryption implementation in this project.
 * 
 * Browser-compatible version using Web Crypto API.
 * Requires a modern browser with crypto.subtle support.
 */

/**
 * AES-256-CBC with PKCS7 padding (standard across languages)
 */
const KEY_SIZE = 256;
const IV_SIZE = 16; // 128 bits
const SALT_SIZE = 16; // 128 bits
const PASSWORD_SIZE = 32; // 256 bits
const PBKDF2_ITERATIONS = 10000;

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
export function generatePassword(passwordSize = PASSWORD_SIZE) {
    if (passwordSize <= 0) {
        throw new RangeError('passwordSize must be greater than zero');
    }

    const keyBytes = new Uint8Array(passwordSize);
    crypto.getRandomValues(keyBytes);
    return base64UrlEncode(keyBytes);
}

/**
 * Encrypts a string using AES-256-CBC encryption with password-based key derivation.
 * 
 * @param {string} plainText - The text to encrypt. Cannot be null or empty.
 * @param {string} password - The password used for encryption. Cannot be null or empty.
 * @returns {Promise<string>} A Base64-encoded string containing the salt, initialization vector (IV), and encrypted data.
 *                            The format is: [Salt (16 bytes)][IV (16 bytes)][Encrypted Data].
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
 * const encrypted = await encrypt('Hello, World!', 'SecurePassword123');
 * console.log(encrypted); // Base64-encoded string with salt, IV, and encrypted data
 */
export async function encrypt(plainText, password) {
    if (!plainText) {
        throw new Error('plainText cannot be null or empty');
    }
    if (!password) {
        throw new Error('password cannot be null or empty');
    }

    // Generate random salt for each encryption
    const salt = new Uint8Array(SALT_SIZE);
    crypto.getRandomValues(salt);

    // Derive key from password using PBKDF2
    const key = await deriveKey(password, salt);

    // Generate random IV for each encryption
    const iv = new Uint8Array(IV_SIZE);
    crypto.getRandomValues(iv);

    // Convert plain text to UTF-8 bytes
    const encoder = new TextEncoder();
    const plainBytes = encoder.encode(plainText);

    // Encrypt the data
    const encrypted = await crypto.subtle.encrypt(
        { name: 'AES-CBC', iv: iv },
        key,
        plainBytes
    );

    // Combine salt + IV + encrypted data
    const result = new Uint8Array(SALT_SIZE + IV_SIZE + encrypted.byteLength);
    result.set(salt, 0);
    result.set(iv, SALT_SIZE);
    result.set(new Uint8Array(encrypted), SALT_SIZE + IV_SIZE);

    // Return as Base64
    return arrayBufferToBase64(result);
}

/**
 * Decrypts an AES-256-CBC encrypted string using password-based key derivation.
 * 
 * @param {string} cipherText - The Base64-encoded encrypted data containing the salt, IV, and encrypted content.
 *                              Cannot be null or empty.
 * @param {string} password - The password used for decryption. Must match the password used during encryption.
 *                           Cannot be null or empty.
 * @returns {Promise<string>} The decrypted plaintext string.
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
 * const decrypted = await decrypt(encrypted, 'SecurePassword123');
 * console.log(decrypted); // 'Hello, World!'
 */
export async function decrypt(cipherText, password) {
    if (!cipherText) {
        throw new Error('cipherText cannot be null or empty');
    }
    if (!password) {
        throw new Error('password cannot be null or empty');
    }

    // Decode from Base64
    let fullCipher;
    try {
        fullCipher = base64ToArrayBuffer(cipherText);
    } catch (error) {
        throw new Error('cipherText is not valid Base64: ' + error.message);
    }

    // Check minimum length
    if (fullCipher.byteLength < SALT_SIZE + IV_SIZE) {
        throw new Error('cipherText is too short to contain salt and IV');
    }

    const fullCipherBytes = new Uint8Array(fullCipher);

    // Extract salt from the beginning
    const salt = fullCipherBytes.slice(0, SALT_SIZE);

    // Derive key from password using PBKDF2
    const key = await deriveKey(password, salt);

    // Extract IV (after salt)
    const iv = fullCipherBytes.slice(SALT_SIZE, SALT_SIZE + IV_SIZE);

    // Get encrypted data (everything after salt + IV)
    const encrypted = fullCipherBytes.slice(SALT_SIZE + IV_SIZE);

    // Decrypt the data
    let decrypted;
    try {
        decrypted = await crypto.subtle.decrypt(
            { name: 'AES-CBC', iv: iv },
            key,
            encrypted
        );
    } catch (error) {
        throw new Error('Decryption failed. The password may be incorrect or the data may be corrupted: ' + error.message);
    }

    // Convert to UTF-8 string
    const decoder = new TextDecoder();
    return decoder.decode(decrypted);
}

/**
 * Derives an AES-256 key from a password and salt using PBKDF2.
 * 
 * @param {string} password - The password to derive the key from.
 * @param {Uint8Array} salt - The salt to use for key derivation.
 * @returns {Promise<CryptoKey>} The derived AES-256 key.
 * @private
 */
async function deriveKey(password, salt) {
    const encoder = new TextEncoder();
    const passwordBytes = encoder.encode(password);

    // Import the password as a key
    const baseKey = await crypto.subtle.importKey(
        'raw',
        passwordBytes,
        { name: 'PBKDF2' },
        false,
        ['deriveBits', 'deriveKey']
    );

    // Derive the AES key using PBKDF2
    return await crypto.subtle.deriveKey(
        {
            name: 'PBKDF2',
            salt: salt,
            iterations: PBKDF2_ITERATIONS,
            hash: 'SHA-256'
        },
        baseKey,
        { name: 'AES-CBC', length: KEY_SIZE },
        false,
        ['encrypt', 'decrypt']
    );
}

/**
 * Encodes a Uint8Array to Base64Url format (URL-safe Base64).
 * 
 * @param {Uint8Array} bytes - The bytes to encode.
 * @returns {string} Base64Url-encoded string.
 * @private
 */
function base64UrlEncode(bytes) {
    const base64 = arrayBufferToBase64(bytes);
    return base64
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=/g, '');
}

/**
 * Converts an ArrayBuffer or Uint8Array to Base64 string.
 * 
 * @param {ArrayBuffer|Uint8Array} buffer - The buffer to convert.
 * @returns {string} Base64-encoded string.
 * @private
 */
function arrayBufferToBase64(buffer) {
    const bytes = buffer instanceof Uint8Array ? buffer : new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

/**
 * Converts a Base64 string to ArrayBuffer.
 * 
 * @param {string} base64 - The Base64 string to convert.
 * @returns {ArrayBuffer} The decoded array buffer.
 * @private
 */
function base64ToArrayBuffer(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
}
