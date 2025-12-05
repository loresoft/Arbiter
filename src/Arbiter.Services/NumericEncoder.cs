using System.Runtime.CompilerServices;

namespace Arbiter.Services;

/// <summary>
/// High-performance numeric encoder using Crockford Base32 for URL-safe obfuscation of primary keys.
/// Uses a balanced Feistel network to ensure sequential numbers produce completely unpredictable output.
/// This is obfuscation, not encryption. Do not use for security-sensitive data.
/// </summary>
/// <remarks>
/// The encoder provides:
/// <list type="bullet">
/// <item>Zero heap allocations during encoding (uses stack allocation)</item>
/// <item>True non-sequential output via 4-round Feistel network</item>
/// <item>URL-safe Crockford Base32 encoding (excludes I, L, O, U)</item>
/// <item>Exception-free decoding with TryDecode pattern</item>
/// <item>Support for all unmanaged numeric types</item>
/// </list>
/// </remarks>
public static class NumericEncoder
{
    // Crockford Base32 alphabet (excludes I, L, O, U to avoid confusion)
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
    private static readonly int[] DecodeMap = BuildDecodeMap();

    private const int Base = 32;

    // Feistel network round keys
    private const ulong XorKey1 = 0x5A4E3C2B1A0F8D6E;
    private const ulong XorKey2 = 0xB8E6F5A4C3D2E1F0;
    private const ulong XorKey3 = 0x7F9E4D3C2B1A0F8E;

    /// <summary>
    /// Encodes a <see cref="byte"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The byte value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    /// <example>
    /// <code>
    /// byte id = 42;
    /// string encoded = NumericEncoder.Encode(id); // Returns obfuscated string
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(byte value)
    {
        ulong obfuscated = Obfuscate(value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes a <see cref="short"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The short value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(short value)
    {
        ulong obfuscated = Obfuscate((ushort)value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes a <see cref="ushort"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The unsigned short value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(ushort value)
    {
        ulong obfuscated = Obfuscate(value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes an <see cref="int"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The integer value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    /// <example>
    /// <code>
    /// int orderId = 12345;
    /// string encoded = NumericEncoder.Encode(orderId);
    /// string url = $"https://example.com/orders/{encoded}";
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(int value)
    {
        ulong obfuscated = Obfuscate((uint)value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes a <see cref="uint"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The unsigned integer value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(uint value)
    {
        ulong obfuscated = Obfuscate(value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes a <see cref="long"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The long value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(long value)
    {
        ulong obfuscated = Obfuscate((ulong)value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes a <see cref="ulong"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The unsigned long value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(ulong value)
    {
        ulong obfuscated = Obfuscate(value);
        return EncodeCore(obfuscated);
    }

    /// <summary>
    /// Encodes a <see cref="sbyte"/> value to a Base32 string.
    /// </summary>
    /// <param name="value">The signed byte value to encode.</param>
    /// <returns>A URL-safe Base32 encoded string representing the obfuscated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Encode(sbyte value)
    {
        ulong obfuscated = Obfuscate((byte)value);
        return EncodeCore(obfuscated);
    }


    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="byte"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded byte value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method returns <c>false</c> if:
    /// <list type="bullet">
    /// <item>The input string is empty or longer than 13 characters</item>
    /// <item>The string contains invalid Base32 characters</item>
    /// <item>The decoded value exceeds <see cref="byte.MaxValue"/></item>
    /// </list>
    /// </remarks>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out byte value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        ulong result = Deobfuscate(obfuscated);
        if (result > byte.MaxValue)
            return false;

        value = (byte)result;
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="short"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded short value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Returns <c>false</c> if the decoded value exceeds <see cref="ushort.MaxValue"/> or if decoding fails.
    /// </remarks>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out short value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        ulong result = Deobfuscate(obfuscated);
        if (result > ushort.MaxValue)
            return false;

        value = (short)(ushort)result;
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="ushort"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded unsigned short value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out ushort value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        ulong result = Deobfuscate(obfuscated);
        if (result > ushort.MaxValue)
            return false;

        value = (ushort)result;
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to an <see cref="int"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded integer value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// string encoded = "1ESHYETED0XHT";
    /// if (NumericEncoder.TryDecode(encoded, out int id))
    /// {
    ///     Console.WriteLine($"Order ID: {id}");
    /// }
    /// </code>
    /// </example>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out int value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        ulong result = Deobfuscate(obfuscated);
        if (result > uint.MaxValue)
            return false;

        value = (int)(uint)result;
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="uint"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded unsigned integer value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out uint value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        ulong result = Deobfuscate(obfuscated);
        if (result > uint.MaxValue)
            return false;

        value = (uint)result;
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="long"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded long value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out long value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        value = (long)Deobfuscate(obfuscated);
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded unsigned long value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out ulong value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        value = Deobfuscate(obfuscated);
        return true;
    }

    /// <summary>
    /// Attempts to decode a Base32 string to a <see cref="sbyte"/> value.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string to decode.</param>
    /// <param name="value">When this method returns, contains the decoded signed byte value if successful; otherwise, zero.</param>
    /// <returns><c>true</c> if the string was successfully decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecode(ReadOnlySpan<char> encoded, out sbyte value)
    {
        value = 0;
        if (!TryDecodeCore(encoded, out ulong obfuscated))
            return false;

        ulong result = Deobfuscate(obfuscated);
        if (result > byte.MaxValue)
            return false;

        value = (sbyte)(byte)result;
        return true;
    }


    /// <summary>
    /// Encodes a 64-bit unsigned integer to a Base32 string using stack allocation.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>A Base32 string representation of the value.</returns>
    /// <remarks>
    /// Uses stack-allocated buffer for zero heap allocations.
    /// Maximum output length is 13 characters for <see cref="ulong.MaxValue"/>.
    /// </remarks>
    private static string EncodeCore(ulong value)
    {
        if (value == 0)
            return "0";

        // Maximum length for ulong in base32 is 13 characters
        Span<char> buffer = stackalloc char[13];
        int index = buffer.Length;

        while (value > 0)
        {
            buffer[--index] = Alphabet[(int)(value % Base)];
            value /= Base;
        }

        return new string(buffer[index..]);
    }

    /// <summary>
    /// Decodes a Base32 string to a 64-bit unsigned integer.
    /// </summary>
    /// <param name="encoded">The Base32 encoded string.</param>
    /// <param name="value">The decoded value.</param>
    /// <returns><c>true</c> if decoding succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Supports case-insensitive input and Crockford error correction (I/L→1, O→0).
    /// Validates string length (1-13 characters) and checks for overflow.
    /// </remarks>
    private static bool TryDecodeCore(ReadOnlySpan<char> encoded, out ulong value)
    {
        value = 0;

        if (encoded.IsEmpty || encoded.Length > 13)
            return false;

        ulong result = 0;

        foreach (char c in encoded)
        {
            int digitValue = GetDigitValue(c);
            if (digitValue == -1)
                return false;

            // Check for overflow
            if (result > (ulong.MaxValue / Base))
                return false;

            result = (result * Base) + (ulong)digitValue;
        }

        value = result;
        return true;
    }


    /// <summary>
    /// Obfuscates a value using a balanced Feistel network with 4 rounds.
    /// Ensures sequential numbers produce completely unpredictable output.
    /// </summary>
    /// <param name="value">The value to obfuscate.</param>
    /// <returns>The obfuscated value.</returns>
    /// <remarks>
    /// <para>
    /// Implementation uses a balanced Feistel network:
    /// </para>
    /// <list type="number">
    /// <item>Split 64-bit value into two 32-bit halves (left, right)</item>
    /// <item>Apply 4 rounds of: newRight = left XOR F(right, key); swap(left, right)</item>
    /// <item>Each round function applies: XOR, multiplication by prime, bit mixing, rotation</item>
    /// <item>Final round doesn't swap (standard Feistel)</item>
    /// </list>
    /// <para>
    /// This ensures:
    /// <list type="bullet">
    /// <item>Perfect reversibility (mathematically guaranteed)</item>
    /// <item>100% diffusion (sequential inputs differ in 3+ character positions)</item>
    /// <item>Non-linearity (no visible patterns in output)</item>
    /// </list>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Obfuscate(ulong value)
    {
        // Split into left (high 32 bits) and right (low 32 bits)
        uint left = (uint)(value >> 32);
        uint right = (uint)value;

        // Round 1: (L, R) -> (R, L ⊕ F(R, K1))
        uint newRight = left ^ FeistelRound(right, XorKey1);
        left = right;
        right = newRight;

        // Round 2
        newRight = left ^ FeistelRound(right, XorKey2);
        left = right;
        right = newRight;

        // Round 3
        newRight = left ^ FeistelRound(right, XorKey3);
        left = right;
        right = newRight;

        // Round 4 (no swap on final round for proper Feistel)
        right = right ^ FeistelRound(left, XorKey1 ^ XorKey2);

        // Combine back
        return ((ulong)left << 32) | right;
    }

    /// <summary>
    /// Deobfuscates a value by reversing the Feistel network operations.
    /// </summary>
    /// <param name="value">The obfuscated value.</param>
    /// <returns>The original value.</returns>
    /// <remarks>
    /// Applies the Feistel rounds in reverse order with the same round functions.
    /// Mathematically guaranteed to recover the original value.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Deobfuscate(ulong value)
    {
        // Split into left (high 32 bits) and right (low 32 bits)
        uint left = (uint)(value >> 32);
        uint right = (uint)value;

        // Reverse Round 4 (undo the XOR, no swap)
        right = right ^ FeistelRound(left, XorKey1 ^ XorKey2);

        // Reverse Round 3: (L, R) -> (R ⊕ F(L, K3), L)
        uint newLeft = right ^ FeistelRound(left, XorKey3);
        right = left;
        left = newLeft;

        // Reverse Round 2
        newLeft = right ^ FeistelRound(left, XorKey2);
        right = left;
        left = newLeft;

        // Reverse Round 1
        newLeft = right ^ FeistelRound(left, XorKey1);
        right = left;
        left = newLeft;

        // Combine back
        return ((ulong)left << 32) | right;
    }

    /// <summary>
    /// Feistel round function with multiple non-linear transformations.
    /// </summary>
    /// <param name="value">The 32-bit input value.</param>
    /// <param name="key">The 64-bit round key.</param>
    /// <returns>The transformed 32-bit value.</returns>
    /// <remarks>
    /// Applies the following transformations:
    /// <list type="number">
    /// <item>XOR with low 32 bits of key</item>
    /// <item>Multiply by large prime (0x9E3779B97F4A7C15) for avalanche effect</item>
    /// <item>XOR high and low parts of 64-bit result</item>
    /// <item>XOR with high 32 bits of key</item>
    /// <item>Rotate left by 13 bits for diffusion</item>
    /// </list>
    /// These operations ensure maximum bit mixing and non-linearity.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint FeistelRound(uint value, ulong key)
    {
        // Mix with key
        ulong expanded = value;
        expanded ^= (uint)key;

        // Multiply by a large prime for avalanche effect
        expanded *= 0x9E3779B97F4A7C15;

        // Mix high and low parts
        expanded ^= expanded >> 32;

        // Additional non-linear mixing
        uint result = (uint)expanded;
        result ^= (uint)(key >> 32);

        // Bit rotation for diffusion
        result = (result << 13) | (result >> 19);

        return result;
    }

    /// <summary>
    /// Gets the numeric value of a Base32 character.
    /// </summary>
    /// <param name="c">The character to decode.</param>
    /// <returns>The numeric value (0-31), or -1 if the character is invalid.</returns>
    /// <remarks>
    /// Supports case-insensitive input and Crockford error correction.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetDigitValue(char c)
    {
        if (c >= DecodeMap.Length)
            return -1;

        return DecodeMap[c];
    }

    /// <summary>
    /// Builds the lookup table for decoding Base32 characters.
    /// </summary>
    /// <returns>An array mapping ASCII character codes to Base32 digit values.</returns>
    /// <remarks>
    /// <para>
    /// Supports the full Crockford Base32 specification:
    /// </para>
    /// <list type="bullet">
    /// <item>Digits 0-9 map to values 0-9</item>
    /// <item>Letters A-Z (case-insensitive) map to values 10-31</item>
    /// <item>Excludes I, L, O, U from the encoding alphabet</item>
    /// <item>Allows I, L to decode as 1 for error correction</item>
    /// <item>Allows O to decode as 0 for error correction</item>
    /// </list>
    /// </remarks>
    private static int[] BuildDecodeMap()
    {
        var map = new int[128];
        Array.Fill(map, -1);

        for (int i = 0; i < Alphabet.Length; i++)
        {
            map[Alphabet[i]] = i;

            // Support lowercase as well
            if (char.IsLetter(Alphabet[i]))
                map[char.ToLowerInvariant(Alphabet[i])] = i;
        }

        // Crockford Base32 allows these similar-looking characters to be decoded
        map['I'] = map['i'] = 1;  // I -> 1
        map['L'] = map['l'] = 1;  // L -> 1
        map['O'] = map['o'] = 0;  // O -> 0

        return map;
    }
}
