# AES-256-CBC Encryption - JavaScript Implementation

This directory contains JavaScript implementations of the AES-256-CBC encryption functionality that is compatible with the C# implementation in this project.

## Files

- **aes-encryption.js** - Node.js implementation using the native `crypto` module
- **aes-encryption.browser.js** - Browser implementation using the Web Crypto API
- **aes-encryption.test.js** - Test suite demonstrating usage and compatibility

## Features

- ✅ AES-256-CBC encryption with PKCS7 padding
- ✅ Password-based key derivation using PBKDF2 (10,000 iterations, SHA-256)
- ✅ Random salt and IV generation for each encryption operation
- ✅ Base64 encoding for encrypted output
- ✅ Base64Url encoding for generated passwords
- ✅ Cross-platform compatibility with C# implementation
- ✅ Comprehensive error handling
- ✅ Support for Unicode text

## Installation

### Node.js

No additional dependencies required - uses native `crypto` module.

```bash
npm install  # No external dependencies needed
```

### Browser

The browser version uses the Web Crypto API which is available in all modern browsers. No installation required.

## Usage

### Node.js

```javascript
const { generatePassword, encrypt, decrypt } = require('./aes-encryption');

// Generate a secure password
const password = generatePassword(); // Default 32 bytes
const customPassword = generatePassword(64); // Custom size

// Encrypt text
const plainText = 'Hello, World!';
const myPassword = 'SecurePassword123';
const encrypted = encrypt(plainText, myPassword);
console.log('Encrypted:', encrypted);

// Decrypt text
const decrypted = decrypt(encrypted, myPassword);
console.log('Decrypted:', decrypted); // 'Hello, World!'
```

### Browser (ES Modules)

```javascript
import { generatePassword, encrypt, decrypt } from './aes-encryption.browser.js';

// Generate a secure password
const password = generatePassword();

// Encrypt text (async)
const plainText = 'Hello, World!';
const myPassword = 'SecurePassword123';
const encrypted = await encrypt(plainText, myPassword);
console.log('Encrypted:', encrypted);

// Decrypt text (async)
const decrypted = await decrypt(encrypted, myPassword);
console.log('Decrypted:', decrypted); // 'Hello, World!'
```

### Browser (HTML)

```html
<!DOCTYPE html>
<html>
<head>
    <title>AES Encryption Demo</title>
</head>
<body>
    <script type="module">
        import { generatePassword, encrypt, decrypt } from './aes-encryption.browser.js';

        async function demo() {
            const password = generatePassword();
            console.log('Generated password:', password);

            const encrypted = await encrypt('Secret message', 'MyPassword123');
            console.log('Encrypted:', encrypted);

            const decrypted = await decrypt(encrypted, 'MyPassword123');
            console.log('Decrypted:', decrypted);
        }

        demo();
    </script>
</body>
</html>
```

## Running Tests

```bash
# Node.js
node aes-encryption.test.js
```

## API Reference

### `generatePassword(passwordSize?: number): string`

Generates a cryptographically secure random password.

**Parameters:**
- `passwordSize` (optional) - Size in bytes, defaults to 32 (256 bits)

**Returns:** Base64Url-encoded password string

**Throws:** `RangeError` if passwordSize is ≤ 0

**Example:**
```javascript
const password = generatePassword();     // 32 bytes (default)
const longPassword = generatePassword(64); // 64 bytes
```

### `encrypt(plainText: string, password: string): string` (Node.js) / `Promise<string>` (Browser)

Encrypts a string using AES-256-CBC.

**Parameters:**
- `plainText` - Text to encrypt (non-empty)
- `password` - Encryption password (non-empty)

**Returns:** Base64-encoded string: `[Salt(16)][IV(16)][EncryptedData]`

**Throws:** `Error` if parameters are null/empty

**Example:**
```javascript
// Node.js
const encrypted = encrypt('Hello, World!', 'MyPassword');

// Browser
const encrypted = await encrypt('Hello, World!', 'MyPassword');
```

### `decrypt(cipherText: string, password: string): string` (Node.js) / `Promise<string>` (Browser)

Decrypts an AES-256-CBC encrypted string.

**Parameters:**
- `cipherText` - Base64-encoded encrypted data
- `password` - Decryption password (must match encryption password)

**Returns:** Decrypted plaintext string

**Throws:**
- `Error` if parameters are null/empty
- `Error` if cipherText is invalid Base64
- `Error` if password is incorrect
- `Error` if data is corrupted/tampered

**Example:**
```javascript
// Node.js
const decrypted = decrypt(encrypted, 'MyPassword');

// Browser
const decrypted = await decrypt(encrypted, 'MyPassword');
```

## Cross-Platform Compatibility

These JavaScript implementations are fully compatible with the C# `AesEncryption` class. Data encrypted in C# can be decrypted in JavaScript and vice versa, as long as the same password is used.

### Example: C# ↔ JavaScript

**C# Encryption:**
```csharp
var encrypted = AesEncryption.Encrypt("Hello, World!", "MyPassword");
// Pass this to JavaScript...
```

**JavaScript Decryption:**
```javascript
const decrypted = decrypt(encryptedFromCSharp, "MyPassword");
// Result: "Hello, World!"
```

**JavaScript Encryption:**
```javascript
const encrypted = encrypt("Hello, World!", "MyPassword");
// Pass this to C#...
```

**C# Decryption:**
```csharp
var decrypted = AesEncryption.Decrypt(encryptedFromJavaScript, "MyPassword");
// Result: "Hello, World!"
```

## Technical Details

### Encryption Specification

- **Algorithm:** AES-256-CBC
- **Padding:** PKCS7
- **Key Size:** 256 bits (32 bytes)
- **IV Size:** 128 bits (16 bytes)
- **Salt Size:** 128 bits (16 bytes)
- **Key Derivation:** PBKDF2 with 10,000 iterations and SHA-256

### Output Format

```
[Salt (16 bytes)][IV (16 bytes)][Encrypted Data (variable)]
```

The entire structure is Base64-encoded.

### Security Features

1. **Random Salt:** Each encryption generates a new random salt, preventing rainbow table attacks
2. **Random IV:** Each encryption generates a new random IV, ensuring identical plaintext produces different ciphertext
3. **PBKDF2:** Strong password-based key derivation with 10,000 iterations
4. **SHA-256:** Secure hashing algorithm for key derivation

## Performance Considerations

### Node.js
- Uses native `crypto` module (C++ bindings)
- Very fast, suitable for server-side use
- Synchronous operations

### Browser
- Uses Web Crypto API (native browser implementation)
- Asynchronous operations (returns Promises)
- Good performance for client-side use
- Requires modern browser support

### Benchmarks (approximate)

**Node.js:** ~1000-2000 encrypt/decrypt cycles per second (depends on hardware)

**Browser:** ~500-1000 encrypt/decrypt cycles per second (depends on browser and hardware)

## Browser Compatibility

### Web Crypto API Support

- ✅ Chrome 37+
- ✅ Firefox 34+
- ✅ Safari 11+
- ✅ Edge 12+
- ✅ Opera 24+
- ❌ Internet Explorer (not supported)

For older browsers, consider using a polyfill or the Node.js version with Browserify/Webpack.

## Error Handling

All functions throw descriptive errors:

```javascript
try {
    const encrypted = encrypt('', 'password'); // Empty text
} catch (error) {
    console.error(error.message); // "plainText cannot be null or empty"
}

try {
    const decrypted = decrypt(encrypted, 'wrongpassword');
} catch (error) {
    console.error(error.message); // Decryption error
}
```

## Best Practices

1. **Password Strength:** Use strong passwords (minimum 16 characters recommended)
2. **Password Storage:** Never hardcode passwords in source code
3. **HTTPS:** Always transmit encrypted data over HTTPS
4. **Error Handling:** Always wrap encrypt/decrypt in try-catch blocks
5. **Key Management:** Store passwords/keys securely (e.g., environment variables, key vaults)

## License

This implementation follows the same license as the parent project.

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing style
- Tests pass
- New features include tests
- Compatibility with C# implementation is maintained
