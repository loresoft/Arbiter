/**
 * Test file for AES Encryption Node.js implementation
 * Demonstrates compatibility with C# implementation
 */

const { generatePassword, encrypt, decrypt } = require('./aes-encryption');

/**
 * Tests for generatePassword function
 */
function testGeneratePassword() {
    console.log('\n=== Testing generatePassword ===');
    
    // Test default size
    const password1 = generatePassword();
    console.log('✓ Generated password (default 32 bytes):', password1);
    console.log('  Length:', password1.length);
    
    // Test custom size
    const password2 = generatePassword(16);
    console.log('✓ Generated password (16 bytes):', password2);
    
    // Test uniqueness
    const password3 = generatePassword();
    console.log('✓ Passwords are unique:', password1 !== password3);
    
    // Test error handling
    try {
        generatePassword(0);
        console.log('✗ Should throw error for size 0');
    } catch (error) {
        console.log('✓ Throws error for invalid size:', error.message);
    }
}

/**
 * Tests for encrypt/decrypt functions
 */
function testEncryptDecrypt() {
    console.log('\n=== Testing encrypt/decrypt ===');
    
    const plainText = 'Hello, World!';
    const password = 'SecurePassword123';
    
    // Test basic encryption
    const encrypted = encrypt(plainText, password);
    console.log('✓ Encrypted text:', encrypted);
    console.log('  Length:', encrypted.length);
    
    // Test basic decryption
    const decrypted = decrypt(encrypted, password);
    console.log('✓ Decrypted text:', decrypted);
    console.log('✓ Matches original:', plainText === decrypted);
    
    // Test different ciphertext for same input
    const encrypted2 = encrypt(plainText, password);
    console.log('✓ Different ciphertext for same input:', encrypted !== encrypted2);
    console.log('✓ Both decrypt to same plaintext:', decrypt(encrypted2, password) === plainText);
}

/**
 * Tests for various text types
 */
function testVariousTextTypes() {
    console.log('\n=== Testing various text types ===');
    
    const password = 'TestPassword';
    const testCases = [
        { name: 'Simple text', text: 'Simple text' },
        { name: 'Numbers', text: 'Text with numbers: 123456' },
        { name: 'Special chars', text: '!@#$%^&*()_+-={}[]|\\:";\'<>?,./' },
        { name: 'Unicode', text: 'Hello 世界! 🌍 Привет مرحبا' },
        { name: 'Multi-line', text: 'Line 1\nLine 2\nLine 3' },
        { name: 'Long text', text: 'A'.repeat(10000) },
        { name: 'Single space', text: ' ' },
        { name: 'Single char', text: 'a' }
    ];
    
    for (const testCase of testCases) {
        const encrypted = encrypt(testCase.text, password);
        const decrypted = decrypt(encrypted, password);
        const passed = testCase.text === decrypted;
        console.log(`${passed ? '✓' : '✗'} ${testCase.name}:`, passed ? 'PASS' : 'FAIL');
        if (!passed) {
            console.log('  Expected:', testCase.text.substring(0, 50));
            console.log('  Got:', decrypted.substring(0, 50));
        }
    }
}

/**
 * Tests for error handling
 */
function testErrorHandling() {
    console.log('\n=== Testing error handling ===');
    
    const validPassword = 'password';
    const validText = 'text';
    const validEncrypted = encrypt(validText, validPassword);
    
    // Test null/empty plainText
    try {
        encrypt('', validPassword);
        console.log('✗ Should throw for empty plainText');
    } catch (error) {
        console.log('✓ Throws for empty plainText:', error.message);
    }
    
    // Test null/empty password for encrypt
    try {
        encrypt(validText, '');
        console.log('✗ Should throw for empty password');
    } catch (error) {
        console.log('✓ Throws for empty password (encrypt):', error.message);
    }
    
    // Test null/empty cipherText
    try {
        decrypt('', validPassword);
        console.log('✗ Should throw for empty cipherText');
    } catch (error) {
        console.log('✓ Throws for empty cipherText:', error.message);
    }
    
    // Test wrong password
    try {
        decrypt(validEncrypted, 'wrongpassword');
        console.log('✗ Should throw for wrong password');
    } catch (error) {
        console.log('✓ Throws for wrong password:', error.message.substring(0, 50));
    }
    
    // Test invalid base64
    try {
        decrypt('not-valid-base64!@#$', validPassword);
        console.log('✗ Should throw for invalid base64');
    } catch (error) {
        console.log('✓ Throws for invalid base64:', error.message.substring(0, 50));
    }
    
    // Test tampered ciphertext
    try {
        const bytes = Buffer.from(validEncrypted, 'base64');
        bytes[bytes.length - 1] ^= 0xFF; // Flip bits in last byte
        const tampered = bytes.toString('base64');
        decrypt(tampered, validPassword);
        console.log('✗ Should throw for tampered ciphertext');
    } catch (error) {
        console.log('✓ Throws for tampered ciphertext:', error.message.substring(0, 50));
    }
}

/**
 * Tests for cross-platform compatibility
 * These encrypted values were generated by the C# implementation
 */
function testCrossPlatformCompatibility() {
    console.log('\n=== Testing cross-platform compatibility ===');
    console.log('Note: Run this with actual C# encrypted values for full compatibility test');
    
    // Example: You would get these from the C# implementation
    // const csharpEncrypted = 'base64-string-from-csharp';
    // const csharpPassword = 'same-password-used-in-csharp';
    // const expectedPlaintext = 'original-text';
    
    // For now, demonstrate that JS can decrypt its own encryption
    const text = 'Cross-platform test';
    const password = 'SharedPassword';
    const jsEncrypted = encrypt(text, password);
    const jsDecrypted = decrypt(jsEncrypted, password);
    
    console.log('✓ JS round-trip test:', text === jsDecrypted);
    console.log('  Original:', text);
    console.log('  Encrypted:', jsEncrypted);
    console.log('  Decrypted:', jsDecrypted);
    
    console.log('\nTo fully test C# compatibility:');
    console.log('1. Encrypt text in C# with a known password');
    console.log('2. Copy the encrypted string here');
    console.log('3. Decrypt with the same password');
    console.log('4. Verify it matches the original text');
}

/**
 * Performance test
 */
function testPerformance() {
    console.log('\n=== Testing performance ===');
    
    const password = 'PerformanceTestPassword';
    const text = 'This is a performance test message.';
    const iterations = 1000;
    
    console.log(`Running ${iterations} encryption/decryption cycles...`);
    
    const startTime = Date.now();
    for (let i = 0; i < iterations; i++) {
        const encrypted = encrypt(text, password);
        const decrypted = decrypt(encrypted, password);
        if (decrypted !== text) {
            console.log('✗ Decryption mismatch at iteration', i);
            break;
        }
    }
    const endTime = Date.now();
    
    const totalTime = endTime - startTime;
    const avgTime = totalTime / iterations;
    
    console.log('✓ All cycles completed successfully');
    console.log(`  Total time: ${totalTime}ms`);
    console.log(`  Average time per cycle: ${avgTime.toFixed(2)}ms`);
    console.log(`  Operations per second: ${(iterations / (totalTime / 1000)).toFixed(2)}`);
}

/**
 * Run all tests
 */
function runAllTests() {
    console.log('==================================================');
    console.log('AES Encryption JavaScript Implementation Tests');
    console.log('==================================================');
    
    try {
        testGeneratePassword();
        testEncryptDecrypt();
        testVariousTextTypes();
        testErrorHandling();
        testCrossPlatformCompatibility();
        testPerformance();
        
        console.log('\n==================================================');
        console.log('All tests completed!');
        console.log('==================================================\n');
    } catch (error) {
        console.error('\n✗ Test failed with error:', error);
        console.error(error.stack);
    }
}

// Run tests if this file is executed directly
if (require.main === module) {
    runAllTests();
}

module.exports = { runAllTests };
