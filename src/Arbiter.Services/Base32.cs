using System.Buffers;
using System.Runtime.CompilerServices;

namespace Arbiter.Services;

/// <summary>
/// High-performance Crockford Base32 encoder and decoder for arbitrary binary data.
/// </summary>
/// <remarks>
/// <para>
/// The API mirrors <see cref="System.Buffers.Text.Base64Url"/> so the two encoders can be used
/// interchangeably.
/// </para>
/// <para>
/// Uses the Crockford Base32 alphabet <c>0123456789ABCDEFGHJKMNPQRSTVWXYZ</c>, which excludes the
/// letters <c>I</c>, <c>L</c>, <c>O</c>, and <c>U</c> to avoid visual ambiguity. The encoding is
/// URL-safe, case-insensitive when decoding, and unpadded.
/// </para>
/// <para>
/// Data is encoded using standard 5-bit grouping, so every 5 bytes produce 8 characters. This is
/// compatible with other Crockford Base32 implementations.
/// </para>
/// <para>
/// Decoding applies Crockford error correction: <c>I</c> and <c>L</c> decode as <c>1</c>, and
/// <c>O</c> decodes as <c>0</c>. Lowercase characters are accepted.
/// </para>
/// <para>
/// Because the alphabet is ASCII, the UTF-8 overloads write and read one byte per character without
/// transcoding. Methods that accept a destination buffer are allocation free.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// byte[] data = [0x01, 0x02, 0x03, 0x04];
///
/// string encoded = Base32.EncodeToString(data);
///
/// Span&lt;byte&gt; buffer = stackalloc byte[Base32.GetMaxDecodedLength(encoded.Length)];
/// if (Base32.TryDecodeFromChars(encoded, buffer, out int bytesWritten))
/// {
///     ReadOnlySpan&lt;byte&gt; decoded = buffer[..bytesWritten];
/// }
/// </code>
/// </example>
public static class Base32
{
    // Crockford Base32 alphabet (excludes I, L, O, U to avoid confusion)
    private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    private static readonly sbyte[] DecodeMap = BuildDecodeMap();

    // number of bits encoded per character
    private const int BitsPerCharacter = 5;

    // number of bytes processed per block
    private const int BlockBytes = 5;

    // number of characters produced by a full block
    private const int BlockCharacters = 8;

    // maximum number of characters or bytes to allocate on the stack before renting from the pool
    private const int StackThreshold = 256;

    // number of characters required to encode a block of the given byte count
    private static ReadOnlySpan<byte> ByteCountToCharCount => [0, 2, 4, 5, 7];

    // number of bytes represented by a block of the given character count, 255 when not valid
    private static ReadOnlySpan<byte> CharCountToByteCount => [0, 255, 1, 255, 2, 3, 255, 4];

    private const string InvalidFormatMessage = "The input is not a valid Base32 string.";
    private const string DestinationTooSmallMessage = "The destination buffer is too small.";


    /// <summary>
    /// Gets the exact number of characters produced by encoding the specified number of bytes.
    /// </summary>
    /// <param name="bytesLength">The number of bytes to encode.</param>
    /// <returns>The number of characters required to encode <paramref name="bytesLength"/> bytes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bytesLength"/> is negative.</exception>
    public static int GetEncodedLength(int bytesLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bytesLength);

        int blocks = bytesLength / BlockBytes;
        int remainder = bytesLength % BlockBytes;

        return (blocks * BlockCharacters) + ByteCountToCharCount[remainder];
    }

    /// <summary>
    /// Gets the maximum number of bytes that decoding the specified number of characters can produce.
    /// </summary>
    /// <param name="encodedLength">The number of encoded characters.</param>
    /// <returns>The buffer size, in bytes, required to decode <paramref name="encodedLength"/> characters.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="encodedLength"/> is negative.</exception>
    public static int GetMaxDecodedLength(int encodedLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(encodedLength);

        int blocks = encodedLength / BlockCharacters;
        int remainder = encodedLength % BlockCharacters;

        return (blocks * BlockBytes) + (remainder * BlockBytes / BlockCharacters);
    }


    /// <summary>
    /// Encodes binary data into a Crockford Base32 string.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <returns>
    /// A Crockford Base32 encoded string, or <see cref="string.Empty"/> when <paramref name="source"/> is empty.
    /// </returns>
    /// <example>
    /// <code>
    /// string encoded = Base32.EncodeToString("Hello"u8);
    /// </code>
    /// </example>
    public static string EncodeToString(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
            return string.Empty;

        int charCount = GetEncodedLength(source.Length);

        char[]? rented = null;
        Span<char> buffer = charCount <= StackThreshold
            ? stackalloc char[StackThreshold]
            : (rented = ArrayPool<char>.Shared.Rent(charCount));

        try
        {
            var destination = buffer[..charCount];
            EncodeToCharsCore(source, destination);

            return new string(destination);
        }
        finally
        {
            if (rented is not null)
                ArrayPool<char>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Encodes binary data into a new Crockford Base32 character array.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <returns>An array containing the encoded characters.</returns>
    public static char[] EncodeToChars(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
            return [];

        var destination = new char[GetEncodedLength(source.Length)];
        EncodeToCharsCore(source, destination);

        return destination;
    }

    /// <summary>
    /// Encodes binary data into the specified character destination buffer.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the encoded characters.</param>
    /// <returns>The number of characters written to <paramref name="destination"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small.</exception>
    public static int EncodeToChars(ReadOnlySpan<byte> source, Span<char> destination)
    {
        if (!TryEncodeToChars(source, destination, out int charsWritten))
            throw new ArgumentException(DestinationTooSmallMessage, nameof(destination));

        return charsWritten;
    }

    /// <summary>
    /// Attempts to encode binary data into the specified character destination buffer.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the encoded characters.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters written.</param>
    /// <returns><c>true</c> if the data was encoded; otherwise, <c>false</c>.</returns>
    public static bool TryEncodeToChars(ReadOnlySpan<byte> source, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;

        if (source.IsEmpty)
            return true;

        int charCount = GetEncodedLength(source.Length);
        if (destination.Length < charCount)
            return false;

        EncodeToCharsCore(source, destination[..charCount]);

        charsWritten = charCount;
        return true;
    }

    /// <summary>
    /// Encodes binary data into a new UTF-8 encoded Crockford Base32 byte array.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <returns>An array containing the UTF-8 encoded characters.</returns>
    public static byte[] EncodeToUtf8(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
            return [];

        var destination = new byte[GetEncodedLength(source.Length)];
        EncodeToUtf8Core(source, destination);

        return destination;
    }

    /// <summary>
    /// Encodes binary data into the specified UTF-8 destination buffer.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the UTF-8 encoded characters.</param>
    /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small.</exception>
    public static int EncodeToUtf8(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (!TryEncodeToUtf8(source, destination, out int bytesWritten))
            throw new ArgumentException(DestinationTooSmallMessage, nameof(destination));

        return bytesWritten;
    }

    /// <summary>
    /// Attempts to encode binary data into the specified UTF-8 destination buffer.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the UTF-8 encoded characters.</param>
    /// <param name="bytesWritten">When this method returns, contains the number of bytes written.</param>
    /// <returns><c>true</c> if the data was encoded; otherwise, <c>false</c>.</returns>
    public static bool TryEncodeToUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (source.IsEmpty)
            return true;

        int charCount = GetEncodedLength(source.Length);
        if (destination.Length < charCount)
            return false;

        EncodeToUtf8Core(source, destination[..charCount]);

        bytesWritten = charCount;
        return true;
    }


    /// <summary>
    /// Decodes Crockford Base32 characters into a new byte array.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <returns>The decoded binary data.</returns>
    /// <exception cref="FormatException"><paramref name="source"/> is not valid Base32.</exception>
    /// <example>
    /// <code>
    /// byte[] decoded = Base32.DecodeFromChars("91JPRV3F");
    /// </code>
    /// </example>
    public static byte[] DecodeFromChars(ReadOnlySpan<char> source)
    {
        if (source.IsEmpty)
            return [];

        if (!TryGetDecodedLength(source.Length, out int byteCount))
            throw new FormatException(InvalidFormatMessage);

        var destination = new byte[byteCount];

        var status = DecodeFromCharsCore(source, destination, validateOnly: false, out _);
        if (status != DecodeStatus.Done)
            throw new FormatException(InvalidFormatMessage);

        return destination;
    }

    /// <summary>
    /// Decodes Crockford Base32 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
    /// <exception cref="FormatException"><paramref name="source"/> is not valid Base32.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small.</exception>
    public static int DecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination)
    {
        var status = DecodeFromCharsCore(source, destination, validateOnly: false, out int bytesWritten);

        return status switch
        {
            DecodeStatus.Done => bytesWritten,
            DecodeStatus.DestinationTooSmall => throw new ArgumentException(DestinationTooSmallMessage, nameof(destination)),
            _ => throw new FormatException(InvalidFormatMessage),
        };
    }

    /// <summary>
    /// Attempts to decode Crockford Base32 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <param name="bytesWritten">When this method returns, contains the number of bytes written.</param>
    /// <returns><c>true</c> if the input was decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination, out int bytesWritten)
        => DecodeFromCharsCore(source, destination, validateOnly: false, out bytesWritten) == DecodeStatus.Done;

    /// <summary>
    /// Decodes UTF-8 encoded Crockford Base32 characters into a new byte array.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to decode.</param>
    /// <returns>The decoded binary data.</returns>
    /// <exception cref="FormatException"><paramref name="utf8Source"/> is not valid Base32.</exception>
    public static byte[] DecodeFromUtf8(ReadOnlySpan<byte> utf8Source)
    {
        if (utf8Source.IsEmpty)
            return [];

        if (!TryGetDecodedLength(utf8Source.Length, out int byteCount))
            throw new FormatException(InvalidFormatMessage);

        var destination = new byte[byteCount];

        var status = DecodeFromUtf8Core(utf8Source, destination, validateOnly: false, out _);
        if (status != DecodeStatus.Done)
            throw new FormatException(InvalidFormatMessage);

        return destination;
    }

    /// <summary>
    /// Decodes UTF-8 encoded Crockford Base32 characters into the specified destination buffer.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
    /// <exception cref="FormatException"><paramref name="utf8Source"/> is not valid Base32.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small.</exception>
    public static int DecodeFromUtf8(ReadOnlySpan<byte> utf8Source, Span<byte> destination)
    {
        var status = DecodeFromUtf8Core(utf8Source, destination, validateOnly: false, out int bytesWritten);

        return status switch
        {
            DecodeStatus.Done => bytesWritten,
            DecodeStatus.DestinationTooSmall => throw new ArgumentException(DestinationTooSmallMessage, nameof(destination)),
            _ => throw new FormatException(InvalidFormatMessage),
        };
    }

    /// <summary>
    /// Attempts to decode UTF-8 encoded Crockford Base32 characters into the specified destination buffer.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <param name="bytesWritten">When this method returns, contains the number of bytes written.</param>
    /// <returns><c>true</c> if the input was decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecodeFromUtf8(ReadOnlySpan<byte> utf8Source, Span<byte> destination, out int bytesWritten)
        => DecodeFromUtf8Core(utf8Source, destination, validateOnly: false, out bytesWritten) == DecodeStatus.Done;


    /// <summary>
    /// Determines whether the specified characters are valid Crockford Base32.
    /// </summary>
    /// <param name="source">The characters to validate.</param>
    /// <returns><c>true</c> if <paramref name="source"/> is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<char> source)
        => DecodeFromCharsCore(source, default, validateOnly: true, out _) == DecodeStatus.Done;

    /// <summary>
    /// Determines whether the specified characters are valid Crockford Base32.
    /// </summary>
    /// <param name="source">The characters to validate.</param>
    /// <param name="decodedLength">When this method returns, contains the decoded byte count.</param>
    /// <returns><c>true</c> if <paramref name="source"/> is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<char> source, out int decodedLength)
        => DecodeFromCharsCore(source, default, validateOnly: true, out decodedLength) == DecodeStatus.Done;

    /// <summary>
    /// Determines whether the specified UTF-8 encoded characters are valid Crockford Base32.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to validate.</param>
    /// <returns><c>true</c> if <paramref name="utf8Source"/> is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Source)
        => DecodeFromUtf8Core(utf8Source, default, validateOnly: true, out _) == DecodeStatus.Done;

    /// <summary>
    /// Determines whether the specified UTF-8 encoded characters are valid Crockford Base32.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to validate.</param>
    /// <param name="decodedLength">When this method returns, contains the decoded byte count.</param>
    /// <returns><c>true</c> if <paramref name="utf8Source"/> is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Source, out int decodedLength)
        => DecodeFromUtf8Core(utf8Source, default, validateOnly: true, out decodedLength) == DecodeStatus.Done;


    /// <summary>
    /// Encodes binary data into the specified destination buffer using 5 bit grouping.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the encoded characters.</param>
    private static void EncodeToCharsCore(ReadOnlySpan<byte> source, Span<char> destination)
    {
        var alphabet = Alphabet.AsSpan();

        int bitBuffer = 0;
        int bitCount = 0;
        int index = 0;

        for (int i = 0; i < source.Length; i++)
        {
            bitBuffer = (bitBuffer << 8) | source[i];
            bitCount += 8;

            while (bitCount >= BitsPerCharacter)
            {
                bitCount -= BitsPerCharacter;
                destination[index++] = alphabet[(bitBuffer >> bitCount) & 0x1F];
            }
        }

        // encode the remaining bits, padded with zeros
        if (bitCount > 0)
            destination[index] = alphabet[(bitBuffer << (BitsPerCharacter - bitCount)) & 0x1F];
    }

    /// <summary>
    /// Encodes binary data into the specified UTF-8 destination buffer using 5 bit grouping.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the UTF-8 encoded characters.</param>
    private static void EncodeToUtf8Core(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        var alphabet = Alphabet.AsSpan();

        int bitBuffer = 0;
        int bitCount = 0;
        int index = 0;

        for (int i = 0; i < source.Length; i++)
        {
            bitBuffer = (bitBuffer << 8) | source[i];
            bitCount += 8;

            while (bitCount >= BitsPerCharacter)
            {
                bitCount -= BitsPerCharacter;
                destination[index++] = (byte)alphabet[(bitBuffer >> bitCount) & 0x1F];
            }
        }

        // encode the remaining bits, padded with zeros
        if (bitCount > 0)
            destination[index] = (byte)alphabet[(bitBuffer << (BitsPerCharacter - bitCount)) & 0x1F];
    }

    /// <summary>
    /// Decodes or validates Crockford Base32 characters.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes; ignored when validating.</param>
    /// <param name="validateOnly">When <c>true</c>, the input is validated without writing any output.</param>
    /// <param name="bytesWritten">The number of bytes written, or the decoded length when validating.</param>
    /// <returns>The status of the operation.</returns>
    private static DecodeStatus DecodeFromCharsCore(ReadOnlySpan<char> source, Span<byte> destination, bool validateOnly, out int bytesWritten)
    {
        bytesWritten = 0;

        if (source.IsEmpty)
            return DecodeStatus.Done;

        if (!TryGetDecodedLength(source.Length, out int byteCount))
            return DecodeStatus.InvalidData;

        if (!validateOnly && destination.Length < byteCount)
            return DecodeStatus.DestinationTooSmall;

        int bitBuffer = 0;
        int bitCount = 0;
        int index = 0;

        for (int i = 0; i < source.Length; i++)
        {
            int value = GetDigitValue(source[i]);
            if (value < 0)
                return DecodeStatus.InvalidData;

            bitBuffer = (bitBuffer << BitsPerCharacter) | value;
            bitCount += BitsPerCharacter;

            if (bitCount < 8)
                continue;

            bitCount -= 8;

            if (!validateOnly)
                destination[index] = (byte)((bitBuffer >> bitCount) & 0xFF);

            index++;
        }

        // remaining padding bits must be zero
        if (bitCount > 0 && (bitBuffer & ((1 << bitCount) - 1)) != 0)
            return DecodeStatus.InvalidData;

        bytesWritten = index;
        return DecodeStatus.Done;
    }

    /// <summary>
    /// Decodes or validates UTF-8 encoded Crockford Base32 characters.
    /// </summary>
    /// <param name="source">The UTF-8 encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes; ignored when validating.</param>
    /// <param name="validateOnly">When <c>true</c>, the input is validated without writing any output.</param>
    /// <param name="bytesWritten">The number of bytes written, or the decoded length when validating.</param>
    /// <returns>The status of the operation.</returns>
    private static DecodeStatus DecodeFromUtf8Core(ReadOnlySpan<byte> source, Span<byte> destination, bool validateOnly, out int bytesWritten)
    {
        bytesWritten = 0;

        if (source.IsEmpty)
            return DecodeStatus.Done;

        if (!TryGetDecodedLength(source.Length, out int byteCount))
            return DecodeStatus.InvalidData;

        if (!validateOnly && destination.Length < byteCount)
            return DecodeStatus.DestinationTooSmall;

        int bitBuffer = 0;
        int bitCount = 0;
        int index = 0;

        for (int i = 0; i < source.Length; i++)
        {
            int value = GetDigitValue(source[i]);
            if (value < 0)
                return DecodeStatus.InvalidData;

            bitBuffer = (bitBuffer << BitsPerCharacter) | value;
            bitCount += BitsPerCharacter;

            if (bitCount < 8)
                continue;

            bitCount -= 8;

            if (!validateOnly)
                destination[index] = (byte)((bitBuffer >> bitCount) & 0xFF);

            index++;
        }

        // remaining padding bits must be zero
        if (bitCount > 0 && (bitBuffer & ((1 << bitCount) - 1)) != 0)
            return DecodeStatus.InvalidData;

        bytesWritten = index;
        return DecodeStatus.Done;
    }

    /// <summary>
    /// Gets the exact number of bytes represented by the specified number of encoded characters.
    /// </summary>
    /// <param name="encodedLength">The number of encoded characters.</param>
    /// <param name="byteCount">When this method returns, contains the decoded byte count; otherwise, zero.</param>
    /// <returns><c>true</c> if <paramref name="encodedLength"/> is a valid encoded length; otherwise, <c>false</c>.</returns>
    private static bool TryGetDecodedLength(int encodedLength, out int byteCount)
    {
        byteCount = 0;

        if (encodedLength < 0)
            return false;

        int blocks = encodedLength / BlockCharacters;
        int remainder = CharCountToByteCount[encodedLength % BlockCharacters];

        if (remainder == 255)
            return false;

        byteCount = (blocks * BlockBytes) + remainder;
        return true;
    }

    /// <summary>
    /// Gets the numeric value of a Crockford Base32 character.
    /// </summary>
    /// <param name="c">The character code to decode.</param>
    /// <returns>The numeric value (0-31), or -1 if the character is invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetDigitValue(int c)
    {
        var map = DecodeMap;

        if ((uint)c >= (uint)map.Length)
            return -1;

        return map[c];
    }

    /// <summary>
    /// Builds the lookup table used to decode Crockford Base32 characters.
    /// </summary>
    /// <returns>An array mapping ASCII character codes to Base32 digit values.</returns>
    private static sbyte[] BuildDecodeMap()
    {
        var map = new sbyte[128];
        Array.Fill(map, (sbyte)-1);

        for (int i = 0; i < Alphabet.Length; i++)
        {
            map[Alphabet[i]] = (sbyte)i;

            // support lowercase as well
            if (char.IsLetter(Alphabet[i]))
                map[char.ToLowerInvariant(Alphabet[i])] = (sbyte)i;
        }

        // Crockford Base32 allows these similar-looking characters to be decoded
        map['I'] = map['i'] = 1;  // I -> 1
        map['L'] = map['l'] = 1;  // L -> 1
        map['O'] = map['o'] = 0;  // O -> 0

        return map;
    }

    /// <summary>
    /// Describes the outcome of a decode operation.
    /// </summary>
    private enum DecodeStatus
    {
        Done,
        InvalidData,
        DestinationTooSmall,
    }
}
