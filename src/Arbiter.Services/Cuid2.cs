// Ignore Spelling: Cuid

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Arbiter.Services;

/// <summary>
/// A secure, collision-resistant identifier implementing the
/// <see href="https://github.com/paralleldrive/cuid2">Cuid2</see> specification.
/// </summary>
/// <remarks>
/// Identifiers are stored inline (no heap allocation) as up to 32 ASCII characters.
/// Generation requires platform support for SHA3-512; see <see cref="IsSupported"/>.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("Design", "MA0097:A class that implements IComparable<T> or IComparable should override comparison operators", Justification = "Cuid2 is not k-sortable; relational operators would imply chronological ordering.")]
public readonly struct Cuid2
    : IEquatable<Cuid2>, IComparable<Cuid2>, IComparable, ISpanFormattable, IUtf8SpanFormattable, ISpanParsable<Cuid2>
{
    /// <summary>The default identifier length.</summary>
    public const int DefaultLength = 24;

    /// <summary>The minimum identifier length.</summary>
    public const int MinLength = 2;

    /// <summary>The maximum identifier length.</summary>
    public const int MaxLength = 32;

    // spec: initialCountMax = 476782367
    private const int InitialCountMax = 476782367;

    // SHA3-512 digest (512 bits) needs at most 100 base36 digits; padded for 6-digit groups
    private const int HashBase36Capacity = 108;

    private static ReadOnlySpan<byte> Alphabet => "0123456789abcdefghijklmnopqrstuvwxyz"u8;

    private static ReadOnlySpan<byte> Letters => "abcdefghijklmnopqrstuvwxyz"u8;

    private static long _counter = RandomNumberGenerator.GetInt32(InitialCountMax) - 1L;

    private static byte[]? _fingerprint;

    private readonly Cuid2Buffer _buffer;
    private readonly byte _length;

    private Cuid2(ReadOnlySpan<byte> ascii)
    {
        Debug.Assert(ascii.Length <= MaxLength);

        ascii.CopyTo(_buffer);
        _length = (byte)ascii.Length;
    }

    [UnscopedRef]
    private ReadOnlySpan<byte> Span => ((ReadOnlySpan<byte>)_buffer)[.._length];


    /// <summary>An empty (default) identifier.</summary>
    public static Cuid2 Empty => default;

    /// <summary>Gets a value indicating whether Cuid2 generation (SHA3-512) is supported on the current platform.</summary>
    public static bool IsSupported => SHA3_512.IsSupported;


    /// <summary>Gets the number of characters in this identifier.</summary>
    public int Length => _length;

    /// <summary>Gets a value indicating whether this identifier is empty.</summary>
    public bool IsEmpty => _length == 0;


    /// <summary>Creates a new identifier with the <see cref="DefaultLength"/>.</summary>
    /// <returns>A new <see cref="Cuid2"/>.</returns>
    /// <exception cref="PlatformNotSupportedException">SHA3-512 is not supported on this platform.</exception>
    public static Cuid2 NewCuid() => NewCuid(DefaultLength);

    /// <summary>Creates a new identifier with the specified length.</summary>
    /// <param name="length">The identifier length, between <see cref="MinLength"/> and <see cref="MaxLength"/>.</param>
    /// <returns>A new <see cref="Cuid2"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is out of range.</exception>
    /// <exception cref="PlatformNotSupportedException">SHA3-512 is not supported on this platform.</exception>
    public static Cuid2 NewCuid(int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, MinLength);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, MaxLength);
        EnsureSupported();

        var fingerprint = GetFingerprint();

        // input = time + salt + count + fingerprint
        Span<byte> input = stackalloc byte[128];
        int written = WriteBase36(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), input);

        var salt = input.Slice(written, length);
        RandomNumberGenerator.GetItems(Alphabet, salt);
        written += length;

        long count = Interlocked.Increment(ref _counter);
        written += WriteBase36(count, input[written..]);

        fingerprint.CopyTo(input[written..]);
        written += fingerprint.Length;

        Span<byte> hash = stackalloc byte[HashBase36Capacity];
        int hashLength = Hash(input[..written], hash);
        Debug.Assert(hashLength >= MaxLength);

        // id = firstLetter + hash.substring(1, length)
        Span<byte> result = stackalloc byte[MaxLength];
        RandomNumberGenerator.GetItems(Letters, result[..1]);
        hash[1..length].CopyTo(result[1..]);

        return new Cuid2(result[..length]);
    }


    /// <summary>Determines whether the specified text is a valid Cuid2.</summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid([NotNullWhen(true)] string? value)
        => value is not null && IsValid(value.AsSpan());

    /// <summary>Determines whether the specified text is a valid Cuid2.</summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<char> value)
    {
        if (value.Length is < MinLength or > MaxLength)
            return false;

        if (!char.IsAsciiLetterLower(value[0]))
            return false;

        foreach (var c in value[1..])
        {
            if (!char.IsAsciiLetterLower(c) && !char.IsAsciiDigit(c))
                return false;
        }

        return true;
    }

    /// <summary>Determines whether the specified UTF-8 text is a valid Cuid2.</summary>
    /// <param name="utf8Value">The UTF-8 value to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise <see langword="false"/>.</returns>
    public static bool IsValid(ReadOnlySpan<byte> utf8Value)
    {
        if (utf8Value.Length is < MinLength or > MaxLength)
            return false;

        if (!IsLower(utf8Value[0]))
            return false;

        foreach (var b in utf8Value[1..])
        {
            if (!IsLower(b) && !IsDigit(b))
                return false;
        }

        return true;
    }


    /// <summary>Parses a string into a <see cref="Cuid2"/>.</summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is null.</exception>
    /// <exception cref="FormatException"><paramref name="s"/> is not a valid Cuid2.</exception>
    public static Cuid2 Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan());
    }

    /// <inheritdoc/>
    static Cuid2 IParsable<Cuid2>.Parse(string s, IFormatProvider? provider) => Parse(s);

    /// <summary>Parses a span of characters into a <see cref="Cuid2"/>.</summary>
    /// <param name="s">The characters to parse.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="FormatException"><paramref name="s"/> is not a valid Cuid2.</exception>
    public static Cuid2 Parse(ReadOnlySpan<char> s)
    {
        if (!TryParse(s, out var result))
            throw new FormatException("The value is not a valid Cuid2.");

        return result;
    }

    /// <inheritdoc/>
    static Cuid2 ISpanParsable<Cuid2>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s);


    /// <summary>Tries to parse a string into a <see cref="Cuid2"/>.</summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">The parsed value, or <see cref="Empty"/> on failure.</param>
    /// <returns><see langword="true"/> if parsing succeeded.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, out Cuid2 result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        return TryParse(s.AsSpan(), out result);
    }

    /// <inheritdoc/>
    static bool IParsable<Cuid2>.TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out Cuid2 result)
        => TryParse(s, out result);

    /// <summary>Tries to parse a span of characters into a <see cref="Cuid2"/>.</summary>
    /// <param name="s">The characters to parse.</param>
    /// <param name="result">The parsed value, or <see cref="Empty"/> on failure.</param>
    /// <returns><see langword="true"/> if parsing succeeded.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, out Cuid2 result)
    {
        if (!IsValid(s))
        {
            result = default;
            return false;
        }

        Span<byte> ascii = stackalloc byte[MaxLength];
        int length = Encoding.ASCII.GetBytes(s, ascii);

        result = new Cuid2(ascii[..length]);
        return true;
    }

    /// <inheritdoc/>
    static bool ISpanParsable<Cuid2>.TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out Cuid2 result)
        => TryParse(s, out result);

    /// <summary>Tries to parse UTF-8 text into a <see cref="Cuid2"/>.</summary>
    /// <param name="utf8Text">The UTF-8 text to parse.</param>
    /// <param name="result">The parsed value, or <see cref="Empty"/> on failure.</param>
    /// <returns><see langword="true"/> if parsing succeeded.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out Cuid2 result)
    {
        if (!IsValid(utf8Text))
        {
            result = default;
            return false;
        }

        result = new Cuid2(utf8Text);
        return true;
    }


    /// <inheritdoc/>
    public override string ToString()
    {
        if (_length == 0)
            return string.Empty;

        return Encoding.ASCII.GetString(Span);
    }

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();


    /// <inheritdoc/>
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => TryFormat(destination, out charsWritten);

    /// <inheritdoc/>
    bool IUtf8SpanFormattable.TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        => TryFormat(utf8Destination, out bytesWritten);

    /// <summary>Tries to format this identifier into the provided character span.</summary>
    /// <param name="destination">The destination span.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <returns><see langword="true"/> if formatting succeeded; <see langword="false"/> if the destination is too small.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten)
    {
        if (destination.Length < _length)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = Encoding.ASCII.GetChars(Span, destination);
        return true;
    }

    /// <summary>Tries to format this identifier as UTF-8 into the provided byte span.</summary>
    /// <param name="utf8Destination">The destination span.</param>
    /// <param name="bytesWritten">The number of bytes written.</param>
    /// <returns><see langword="true"/> if formatting succeeded; <see langword="false"/> if the destination is too small.</returns>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten)
    {
        if (!Span.TryCopyTo(utf8Destination))
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = _length;
        return true;
    }


    /// <inheritdoc/>
    public bool Equals(Cuid2 other) => Span.SequenceEqual(other.Span);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is Cuid2 other && Equals(other);


    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(Span);

        return hash.ToHashCode();
    }


    /// <summary>Compares identifiers using ordinal (byte-wise) ordering.</summary>
    /// <remarks>
    /// Cuid2 values are intentionally not k-sortable; this ordering is stable and deterministic
    /// (useful for sorted collections and repeatable output) but does not reflect creation time.
    /// </remarks>
    /// <param name="other">The identifier to compare with.</param>
    /// <returns>A value indicating the relative ordinal order.</returns>
    public int CompareTo(Cuid2 other) => Span.SequenceCompareTo(other.Span);

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is not Cuid2 other)
            throw new ArgumentException($"Object must be of type {nameof(Cuid2)}.", nameof(obj));

        return CompareTo(other);
    }


    /// <summary>Determines whether two identifiers are equal.</summary>
    public static bool operator ==(Cuid2 left, Cuid2 right) => left.Equals(right);

    /// <summary>Determines whether two identifiers are not equal.</summary>
    public static bool operator !=(Cuid2 left, Cuid2 right) => !left.Equals(right);


    /// <summary>
    /// Spec <c>hash</c>: SHA3-512 of <paramref name="input"/>, as an unsigned big-endian integer
    /// rendered in base36 with the first digit dropped.
    /// </summary>
    /// <param name="input">The input bytes.</param>
    /// <param name="destination">The destination, at least 108 bytes.</param>
    /// <returns>The number of ASCII characters written.</returns>
    internal static int Hash(ReadOnlySpan<byte> input, Span<byte> destination)
    {
        EnsureSupported();

        Span<byte> digest = stackalloc byte[SHA3_512.HashSizeInBytes];
        SHA3_512.HashData(input, digest);

        Span<byte> encoded = stackalloc byte[HashBase36Capacity];
        int length = EncodeBase36(digest, encoded);

        // spec: .slice(1)
        encoded[1..length].CopyTo(destination);
        return length - 1;
    }

    /// <summary>Writes a non-negative integer as lowercase base36 ASCII.</summary>
    /// <param name="value">The value to write.</param>
    /// <param name="destination">The destination, at least 13 bytes.</param>
    /// <returns>The number of characters written.</returns>
    internal static int WriteBase36(long value, Span<byte> destination)
    {
        Debug.Assert(value >= 0);

        Span<byte> buffer = stackalloc byte[13];
        int position = buffer.Length;
        var alphabet = Alphabet;

        do
        {
            (value, long remainder) = Math.DivRem(value, 36);
            buffer[--position] = alphabet[(int)remainder];
        }
        while (value != 0);

        var digits = buffer[position..];
        digits.CopyTo(destination);
        return digits.Length;
    }

    /// <summary>Encodes an unsigned big-endian integer (up to 64 bytes) as lowercase base36 ASCII.</summary>
    /// <param name="bigEndian">The big-endian unsigned integer bytes, at most 64.</param>
    /// <param name="destination">The destination, at least 108 bytes.</param>
    /// <returns>The number of characters written.</returns>
    internal static int EncodeBase36(ReadOnlySpan<byte> bigEndian, Span<byte> destination)
    {
        const uint GroupDivisor = 2176782336; // 36^6
        const int GroupDigits = 6;

        Debug.Assert(bigEndian.Length <= 64);

        int limbCount = (bigEndian.Length + 3) / 4;
        Span<uint> limbs = stackalloc uint[16];
        limbs = limbs[..limbCount];
        PackLimbs(bigEndian, limbs);

        Span<byte> buffer = stackalloc byte[HashBase36Capacity];
        int position = buffer.Length;
        var alphabet = Alphabet;
        int start = 0;

        while (start < limbs.Length && limbs[start] == 0)
            start++;

        while (start < limbs.Length)
        {
            ulong remainder = 0;
            for (int i = start; i < limbs.Length; i++)
            {
                ulong current = (remainder << 32) | limbs[i];
                limbs[i] = (uint)(current / GroupDivisor);
                remainder = current % GroupDivisor;
            }

            while (start < limbs.Length && limbs[start] == 0)
                start++;

            for (int d = 0; d < GroupDigits; d++)
            {
                buffer[--position] = alphabet[(int)(remainder % 36)];
                remainder /= 36;
            }
        }

        // trim leading zeros, keep at least one digit
        while (position < buffer.Length - 1 && buffer[position] == (byte)'0')
            position++;

        if (position == buffer.Length)
            buffer[--position] = (byte)'0';

        var digits = buffer[position..];
        digits.CopyTo(destination);
        return digits.Length;
    }


    private static void PackLimbs(ReadOnlySpan<byte> bigEndian, Span<uint> limbs)
    {
        // big-endian 32-bit limbs, left-padded with zeros
        int offset = bigEndian.Length - (limbs.Length * 4);
        for (int i = 0; i < limbs.Length; i++)
        {
            uint limb = 0;
            for (int j = 0; j < 4; j++)
            {
                int index = offset + (i * 4) + j;
                limb = (limb << 8) | (index >= 0 ? bigEndian[index] : 0u);
            }
            limbs[i] = limb;
        }
    }

    private static ReadOnlySpan<byte> GetFingerprint()
    {
        var fingerprint = Volatile.Read(ref _fingerprint);
        if (fingerprint is not null)
            return fingerprint;

        fingerprint = CreateFingerprint();
        return Interlocked.CompareExchange(ref _fingerprint, fingerprint, null) ?? fingerprint;
    }

    private static byte[] CreateFingerprint()
    {
        // spec: hash(globals + createEntropy(bigLength)).substring(0, bigLength)
        var globals = new StringBuilder();
        globals.Append(Environment.MachineName);
        globals.Append(Environment.ProcessId);
        globals.Append(Environment.CurrentManagedThreadId);
        globals.Append(Environment.TickCount64);

        foreach (var key in Environment.GetEnvironmentVariables().Keys)
            globals.Append(key);

        Span<byte> entropy = stackalloc byte[MaxLength];
        RandomNumberGenerator.GetItems(Alphabet, entropy);

        var globalBytes = Encoding.UTF8.GetBytes(globals.ToString());
        var input = new byte[globalBytes.Length + entropy.Length];
        globalBytes.CopyTo(input, 0);
        entropy.CopyTo(input.AsSpan(globalBytes.Length));

        Span<byte> hash = stackalloc byte[HashBase36Capacity];
        Hash(input, hash);

        return hash[..MaxLength].ToArray();
    }

    private static void EnsureSupported()
    {
        if (!SHA3_512.IsSupported)
            ThrowNotSupported();
    }

    [DoesNotReturn]
    private static void ThrowNotSupported()
        => throw new PlatformNotSupportedException("Cuid2 requires SHA3-512, which is not supported on this platform.");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsLower(byte b) => (uint)(b - 'a') <= 'z' - 'a';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigit(byte b) => (uint)(b - '0') <= 9;

    [InlineArray(MaxLength)]
    private struct Cuid2Buffer
    {
        private byte _element0;
    }
}
