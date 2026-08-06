using System.Buffers;
using System.Runtime.CompilerServices;

namespace Arbiter.Services;

/// <summary>
/// High-performance Base62 encoder and decoder for arbitrary binary data.
/// </summary>
/// <remarks>
/// <para>
/// The API mirrors <see cref="System.Buffers.Text.Base64Url"/> so the two encoders can be used
/// interchangeably.
/// </para>
/// <para>
/// Uses the alphabet <c>0-9</c>, <c>A-Z</c>, then <c>a-z</c>. The encoding is URL-safe, unpadded, and
/// case-sensitive when decoding.
/// </para>
/// <para>
/// <b>This format is Arbiter specific and is not compatible with canonical (GMP or BigInteger style)
/// Base62 implementations.</b> Data is encoded in fixed 8 byte blocks: every full block produces exactly
/// 11 characters, and a trailing partial block produces a fixed number of characters based on its size.
/// Round-trip fidelity is only guaranteed through this class.
/// </para>
/// <para>
/// The chunked scheme is used because it runs in linear time using only <see cref="ulong"/> arithmetic,
/// whereas canonical Base62 requires quadratic time big integer division. It also preserves leading zero
/// bytes, which canonical Base62 cannot do without an out-of-band convention. The trade-off is that the
/// output is roughly 2.3% longer than canonical Base62.
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
/// string encoded = Base62.EncodeToString(data);
///
/// Span&lt;byte&gt; buffer = stackalloc byte[Base62.GetMaxDecodedLength(encoded.Length)];
/// if (Base62.TryDecodeFromChars(encoded, buffer, out int bytesWritten))
/// {
///     ReadOnlySpan&lt;byte&gt; decoded = buffer[..bytesWritten];
/// }
/// </code>
/// </example>
public static class Base62
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly sbyte[] DecodeMap = BuildDecodeMap();

    private const int Base = 62;

    // number of bytes processed per block
    private const int BlockBytes = 8;

    // number of characters produced by a full block
    private const int BlockCharacters = 11;

    // maximum number of characters or bytes to allocate on the stack before renting from the pool
    private const int StackThreshold = 256;

    // number of characters required to encode a block of the given byte count
    private static ReadOnlySpan<byte> ByteCountToCharCount => [0, 2, 3, 5, 6, 7, 9, 10, 11];

    // number of bytes represented by a block of the given character count, 255 when not valid
    private static ReadOnlySpan<byte> CharCountToByteCount => [0, 255, 1, 2, 255, 3, 4, 5, 255, 6, 7];

    private const string InvalidFormatMessage = "The input is not a valid Base62 string.";
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
    /// Encodes binary data into a Base62 string.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <returns>
    /// A Base62 encoded string, or <see cref="string.Empty"/> when <paramref name="source"/> is empty.
    /// </returns>
    /// <example>
    /// <code>
    /// string encoded = Base62.EncodeToString("Hello"u8);
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
    /// Encodes binary data into a new Base62 character array.
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
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters written to <paramref name="destination"/>;
    /// otherwise, zero.
    /// </param>
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
    /// Encodes binary data into a new UTF-8 encoded Base62 byte array.
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
    /// Encodes binary data as UTF-8 characters into the specified destination buffer.
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
    /// Attempts to encode binary data as UTF-8 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the UTF-8 encoded characters.</param>
    /// <param name="bytesWritten">
    /// When this method returns, contains the number of bytes written to <paramref name="destination"/>;
    /// otherwise, zero.
    /// </param>
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
    /// Decodes Base62 characters into a new byte array.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <returns>An array containing the decoded bytes.</returns>
    /// <exception cref="FormatException"><paramref name="source"/> is not valid Base62.</exception>
    /// <example>
    /// <code>
    /// byte[] decoded = Base62.DecodeFromChars("0FzUM");
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
    /// Decodes Base62 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
    /// <exception cref="FormatException"><paramref name="source"/> is not valid Base62.</exception>
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
    /// Attempts to decode Base62 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <param name="bytesWritten">
    /// When this method returns, contains the number of bytes written to <paramref name="destination"/>;
    /// otherwise, zero.
    /// </param>
    /// <returns><c>true</c> if the input was successfully decoded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method never throws. It returns <c>false</c> when the input contains a character that is not
    /// part of the alphabet, when the input length is not a valid block length, when a block value
    /// overflows the number of bytes it represents, or when <paramref name="destination"/> is too small.
    /// Use <see cref="GetMaxDecodedLength(int)"/> to size <paramref name="destination"/>.
    /// </remarks>
    public static bool TryDecodeFromChars(ReadOnlySpan<char> source, Span<byte> destination, out int bytesWritten)
        => DecodeFromCharsCore(source, destination, validateOnly: false, out bytesWritten) == DecodeStatus.Done;

    /// <summary>
    /// Decodes UTF-8 encoded Base62 characters into a new byte array.
    /// </summary>
    /// <param name="source">The UTF-8 encoded characters to decode.</param>
    /// <returns>An array containing the decoded bytes.</returns>
    /// <exception cref="FormatException"><paramref name="source"/> is not valid Base62.</exception>
    public static byte[] DecodeFromUtf8(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty)
            return [];

        if (!TryGetDecodedLength(source.Length, out int byteCount))
            throw new FormatException(InvalidFormatMessage);

        var destination = new byte[byteCount];

        var status = DecodeFromUtf8Core(source, destination, validateOnly: false, out _);
        if (status != DecodeStatus.Done)
            throw new FormatException(InvalidFormatMessage);

        return destination;
    }

    /// <summary>
    /// Decodes UTF-8 encoded Base62 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The UTF-8 encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <returns>The number of bytes written to <paramref name="destination"/>.</returns>
    /// <exception cref="FormatException"><paramref name="source"/> is not valid Base62.</exception>
    /// <exception cref="ArgumentException"><paramref name="destination"/> is too small.</exception>
    public static int DecodeFromUtf8(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        var status = DecodeFromUtf8Core(source, destination, validateOnly: false, out int bytesWritten);

        return status switch
        {
            DecodeStatus.Done => bytesWritten,
            DecodeStatus.DestinationTooSmall => throw new ArgumentException(DestinationTooSmallMessage, nameof(destination)),
            _ => throw new FormatException(InvalidFormatMessage),
        };
    }

    /// <summary>
    /// Attempts to decode UTF-8 encoded Base62 characters into the specified destination buffer.
    /// </summary>
    /// <param name="source">The UTF-8 encoded characters to decode.</param>
    /// <param name="destination">The buffer that receives the decoded bytes.</param>
    /// <param name="bytesWritten">
    /// When this method returns, contains the number of bytes written to <paramref name="destination"/>;
    /// otherwise, zero.
    /// </param>
    /// <returns><c>true</c> if the input was successfully decoded; otherwise, <c>false</c>.</returns>
    public static bool TryDecodeFromUtf8(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
        => DecodeFromUtf8Core(source, destination, validateOnly: false, out bytesWritten) == DecodeStatus.Done;


    /// <summary>
    /// Determines whether the specified characters are valid Base62.
    /// </summary>
    /// <param name="source">The characters to validate.</param>
    /// <returns><c>true</c> if <paramref name="source"/> can be decoded; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<char> source)
        => DecodeFromCharsCore(source, default, validateOnly: true, out _) == DecodeStatus.Done;

    /// <summary>
    /// Determines whether the specified characters are valid Base62 and reports the decoded length.
    /// </summary>
    /// <param name="source">The characters to validate.</param>
    /// <param name="decodedLength">
    /// When this method returns, contains the number of bytes the input decodes to; otherwise, zero.
    /// </param>
    /// <returns><c>true</c> if <paramref name="source"/> can be decoded; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<char> source, out int decodedLength)
        => DecodeFromCharsCore(source, default, validateOnly: true, out decodedLength) == DecodeStatus.Done;

    /// <summary>
    /// Determines whether the specified UTF-8 encoded characters are valid Base62.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to validate.</param>
    /// <returns><c>true</c> if <paramref name="utf8Source"/> can be decoded; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Source)
        => DecodeFromUtf8Core(utf8Source, default, validateOnly: true, out _) == DecodeStatus.Done;

    /// <summary>
    /// Determines whether the specified UTF-8 encoded characters are valid Base62 and reports the decoded length.
    /// </summary>
    /// <param name="utf8Source">The UTF-8 encoded characters to validate.</param>
    /// <param name="decodedLength">
    /// When this method returns, contains the number of bytes the input decodes to; otherwise, zero.
    /// </param>
    /// <returns><c>true</c> if <paramref name="utf8Source"/> can be decoded; otherwise, <c>false</c>.</returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Source, out int decodedLength)
        => DecodeFromUtf8Core(utf8Source, default, validateOnly: true, out decodedLength) == DecodeStatus.Done;


    /// <summary>
    /// Encodes binary data into the specified destination buffer using fixed 8 byte blocks.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the encoded characters.</param>
    private static void EncodeToCharsCore(ReadOnlySpan<byte> source, Span<char> destination)
    {
        var alphabet = Alphabet.AsSpan();

        int index = 0;

        while (!source.IsEmpty)
        {
            int blockBytes = Math.Min(BlockBytes, source.Length);

            // read the block as a big-endian unsigned integer
            ulong value = 0;
            for (int i = 0; i < blockBytes; i++)
                value = (value << 8) | source[i];

            // write the fixed number of digits, least significant last
            int charCount = ByteCountToCharCount[blockBytes];
            for (int i = charCount - 1; i >= 0; i--)
            {
                destination[index + i] = alphabet[(int)(value % Base)];
                value /= Base;
            }

            index += charCount;
            source = source[blockBytes..];
        }
    }

    /// <summary>
    /// Encodes binary data into the specified UTF-8 destination buffer using fixed 8 byte blocks.
    /// </summary>
    /// <param name="source">The binary data to encode.</param>
    /// <param name="destination">The buffer that receives the UTF-8 encoded characters.</param>
    private static void EncodeToUtf8Core(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        var alphabet = Alphabet.AsSpan();

        int index = 0;

        while (!source.IsEmpty)
        {
            int blockBytes = Math.Min(BlockBytes, source.Length);

            // read the block as a big-endian unsigned integer
            ulong value = 0;
            for (int i = 0; i < blockBytes; i++)
                value = (value << 8) | source[i];

            // write the fixed number of digits, least significant last
            int charCount = ByteCountToCharCount[blockBytes];
            for (int i = charCount - 1; i >= 0; i--)
            {
                destination[index + i] = (byte)alphabet[(int)(value % Base)];
                value /= Base;
            }

            index += charCount;
            source = source[blockBytes..];
        }
    }

    /// <summary>
    /// Decodes or validates Base62 characters.
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

        int written = 0;

        while (!source.IsEmpty)
        {
            int charCount = Math.Min(BlockCharacters, source.Length);
            int blockBytes = charCount == BlockCharacters ? BlockBytes : CharCountToByteCount[charCount];

            ulong value = 0;
            for (int i = 0; i < charCount; i++)
            {
                int digit = GetDigitValue(source[i]);
                if (digit < 0)
                    return DecodeStatus.InvalidData;

                // guard against overflowing the 64 bit accumulator
                if (value > (ulong.MaxValue - (ulong)digit) / Base)
                    return DecodeStatus.InvalidData;

                value = (value * Base) + (ulong)digit;
            }

            // a partial block must fit in the number of bytes it represents
            if (blockBytes < BlockBytes && value >= 1UL << (blockBytes * 8))
                return DecodeStatus.InvalidData;

            if (!validateOnly)
            {
                for (int i = blockBytes - 1; i >= 0; i--)
                {
                    destination[written + i] = (byte)value;
                    value >>= 8;
                }
            }

            written += blockBytes;
            source = source[charCount..];
        }

        bytesWritten = written;
        return DecodeStatus.Done;
    }

    /// <summary>
    /// Decodes or validates UTF-8 encoded Base62 characters.
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

        int written = 0;

        while (!source.IsEmpty)
        {
            int charCount = Math.Min(BlockCharacters, source.Length);
            int blockBytes = charCount == BlockCharacters ? BlockBytes : CharCountToByteCount[charCount];

            ulong value = 0;
            for (int i = 0; i < charCount; i++)
            {
                int digit = GetDigitValue(source[i]);
                if (digit < 0)
                    return DecodeStatus.InvalidData;

                // guard against overflowing the 64 bit accumulator
                if (value > (ulong.MaxValue - (ulong)digit) / Base)
                    return DecodeStatus.InvalidData;

                value = (value * Base) + (ulong)digit;
            }

            // a partial block must fit in the number of bytes it represents
            if (blockBytes < BlockBytes && value >= 1UL << (blockBytes * 8))
                return DecodeStatus.InvalidData;

            if (!validateOnly)
            {
                for (int i = blockBytes - 1; i >= 0; i--)
                {
                    destination[written + i] = (byte)value;
                    value >>= 8;
                }
            }

            written += blockBytes;
            source = source[charCount..];
        }

        bytesWritten = written;
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
    /// Gets the numeric value of a Base62 character.
    /// </summary>
    /// <param name="c">The character code to decode.</param>
    /// <returns>The numeric value (0-61), or -1 if the character is invalid.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetDigitValue(int c)
    {
        var map = DecodeMap;

        if ((uint)c >= (uint)map.Length)
            return -1;

        return map[c];
    }

    /// <summary>
    /// Builds the lookup table used to decode Base62 characters.
    /// </summary>
    /// <returns>An array mapping ASCII character codes to Base62 digit values.</returns>
    private static sbyte[] BuildDecodeMap()
    {
        var map = new sbyte[128];
        Array.Fill(map, (sbyte)-1);

        for (int i = 0; i < Alphabet.Length; i++)
            map[Alphabet[i]] = (sbyte)i;

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
