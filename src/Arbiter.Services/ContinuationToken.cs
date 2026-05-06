using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace Arbiter.Services;

/// <summary>
/// Provides efficient serialization and deserialization of strongly-typed values into URL-safe Base64 continuation tokens.
/// Supports common .NET types including primitives, DateTime, DateTimeOffset, Guid, and strings.
/// Null string values are serialized as <see cref="string.Empty" />.
/// </summary>
/// <remarks>
/// <para>
/// This class is designed for implementing keyset pagination (also known as cursor-based pagination) where the continuation token
/// contains the values of the last record from the previous page. These values are then used to query the next set of records.
/// </para>
/// <para>
/// Keyset pagination offers several advantages over offset-based pagination:
/// <list type="bullet">
/// <item><description>Consistent results even when data is added or removed between page requests</description></item>
/// <item><description>Better performance as it doesn't require scanning/skipping rows</description></item>
/// <item><description>Works well with real-time data and concurrent modifications</description></item>
/// </list>
/// </para>
/// <example>
/// Implementing keyset pagination using DateTime and Id:
/// <code>
/// // Creating a continuation token from the last record on the page
/// public class EventListQuery
/// {
///     public string? ContinuationToken { get; set; }
///     public int PageSize { get; set; } = 20;
/// }
///
/// public async Task&lt;PagedResult&lt;Event&gt;&gt; GetEvents(EventListQuery query)
/// {
///     DateTime? lastCreatedDate = null;
///     int? lastId = null;
///
///     // Parse the continuation token if provided
///     if (!string.IsNullOrEmpty(query.ContinuationToken))
///     {
///         (lastCreatedDate, lastId) = ContinuationToken.Parse&lt;DateTime, int&gt;(query.ContinuationToken);
///     }
///
///     // Query using keyset pagination
///     var events = await dbContext.Events
///         .Where(e => lastCreatedDate == null ||
///                     e.CreatedDate &gt; lastCreatedDate ||
///                     (e.CreatedDate == lastCreatedDate &amp;&amp; e.Id &gt; lastId))
///         .OrderBy(e => e.CreatedDate)
///         .ThenBy(e => e.Id)
///         .Take(query.PageSize + 1)  // Take one extra to determine if there are more pages
///         .ToListAsync();
///
///     var hasMore = events.Count &gt; query.PageSize;
///     var pageItems = hasMore ? events.Take(query.PageSize).ToList() : events;
///
///     // Create continuation token from the last item
///     string? nextToken = null;
///     if (hasMore)
///     {
///         var lastItem = pageItems.Last();
///         nextToken = ContinuationToken.Create(lastItem.CreatedDate, lastItem.Id);
///     }
///
///     return new PagedResult&lt;Event&gt;
///     {
///         Items = pageItems,
///         ContinuationToken = nextToken
///     };
/// }
/// </code>
/// </example>
/// </remarks>
public static class ContinuationToken
{
    private const int MaxStackAllocSize = 512;

    // Type markers for efficient serialization
    private enum TypeMarker : byte
    {
        Boolean = 1,
        Byte = 2,
        DateTime = 3,
        DateTimeOffset = 4,
        Decimal = 5,
        Double = 6,
        Guid = 7,
        Int16 = 8,
        Int32 = 9,
        Int64 = 10,
        String = 11,
    }

    #region Create Methods

    /// <summary>
    /// Creates a continuation token from a single value.
    /// </summary>
    /// <typeparam name="T">The type of the value. Must be a supported type (bool, byte, short, int, long, decimal, double, DateTime, DateTimeOffset, Guid, or string).</typeparam>
    /// <param name="value">The value to serialize into a token.</param>
    /// <returns>A URL-safe Base64-encoded continuation token.</returns>
    /// <exception cref="NotSupportedException">Thrown when the type T is not supported.</exception>
    public static string Create<T>(T value)
    {
        var size = EstimateSize(value);
        Span<byte> buffer = size <= MaxStackAllocSize
            ? stackalloc byte[size]
            : new byte[size];

        var written = WriteValue(buffer, value);

        return Base64Url.EncodeToString(buffer[..written]);
    }

    /// <summary>
    /// Creates a continuation token from two values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <param name="value1">The first value to serialize.</param>
    /// <param name="value2">The second value to serialize.</param>
    /// <returns>A URL-safe Base64-encoded continuation token.</returns>
    /// <exception cref="NotSupportedException">Thrown when any of the types are not supported.</exception>
    public static string Create<T1, T2>(T1 value1, T2 value2)
    {
        var size = EstimateSize(value1) + EstimateSize(value2);
        Span<byte> buffer = size <= MaxStackAllocSize
            ? stackalloc byte[size]
            : new byte[size];

        var pos = WriteValue(buffer, value1);
        pos += WriteValue(buffer[pos..], value2);

        return Base64Url.EncodeToString(buffer[..pos]);
    }

    /// <summary>
    /// Creates a continuation token from three values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <param name="value1">The first value to serialize.</param>
    /// <param name="value2">The second value to serialize.</param>
    /// <param name="value3">The third value to serialize.</param>
    /// <returns>A URL-safe Base64-encoded continuation token.</returns>
    /// <exception cref="NotSupportedException">Thrown when any of the types are not supported.</exception>
    public static string Create<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
        var size = EstimateSize(value1) + EstimateSize(value2) + EstimateSize(value3);
        Span<byte> buffer = size <= MaxStackAllocSize
            ? stackalloc byte[size]
            : new byte[size];

        var pos = WriteValue(buffer, value1);
        pos += WriteValue(buffer[pos..], value2);
        pos += WriteValue(buffer[pos..], value3);

        return Base64Url.EncodeToString(buffer[..pos]);
    }

    #endregion

    #region Parse Methods

    /// <summary>
    /// Parses a continuation token and extracts a single value.
    /// </summary>
    /// <typeparam name="T">The type of the value to extract. Must match the type used when creating the token.</typeparam>
    /// <param name="token">The continuation token to parse, or <see langword="null"/> or empty to return the default value.</param>
    /// <returns>The deserialized value, or the default value when <paramref name="token"/> is <see langword="null"/> or empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token contains a type that doesn't match T.</exception>
    /// <exception cref="FormatException">Thrown when the token is not valid Base64Url or contains malformed payload data.</exception>
    public static T? Parse<T>(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return default;

        var maxDecodedLength = Base64Url.GetMaxDecodedLength(token.Length);
        Span<byte> bytes = maxDecodedLength <= MaxStackAllocSize
            ? stackalloc byte[maxDecodedLength]
            : new byte[maxDecodedLength];

        var bytesWritten = Base64Url.DecodeFromChars(token.AsSpan(), bytes);
        bytes = bytes[..bytesWritten];
        var pos = 0;

        var value = ReadValue<T>(bytes, ref pos);

        EnsureFullyConsumed(bytes, pos);

        return value;
    }

    /// <summary>
    /// Parses a continuation token and extracts two values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <param name="token">The continuation token to parse, or <see langword="null"/> or empty to return the default tuple.</param>
    /// <returns>A tuple containing the two deserialized values, or the default tuple when <paramref name="token"/> is <see langword="null"/> or empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token contains types that don't match T1 or T2.</exception>
    /// <exception cref="FormatException">Thrown when the token is not valid Base64Url or contains malformed payload data.</exception>
    public static (T1, T2) Parse<T1, T2>(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return default;

        var maxDecodedLength = Base64Url.GetMaxDecodedLength(token.Length);
        Span<byte> bytes = maxDecodedLength <= MaxStackAllocSize
            ? stackalloc byte[maxDecodedLength]
            : new byte[maxDecodedLength];

        var bytesWritten = Base64Url.DecodeFromChars(token.AsSpan(), bytes);
        bytes = bytes[..bytesWritten];
        var pos = 0;

        var v1 = ReadValue<T1>(bytes, ref pos);
        var v2 = ReadValue<T2>(bytes, ref pos);

        EnsureFullyConsumed(bytes, pos);

        return (v1, v2);
    }

    /// <summary>
    /// Parsing a continuation token and extracts three values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <param name="token">The continuation token to parse, or <see langword="null"/> or empty to return the default tuple.</param>
    /// <returns>A tuple containing the three deserialized values, or the default tuple when <paramref name="token"/> is <see langword="null"/> or empty.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the token contains types that don't match T1, T2, or T3.</exception>
    /// <exception cref="FormatException">Thrown when the token is not valid Base64Url or contains malformed payload data.</exception>
    public static (T1, T2, T3) Parse<T1, T2, T3>(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return default;

        var maxDecodedLength = Base64Url.GetMaxDecodedLength(token.Length);
        Span<byte> bytes = maxDecodedLength <= MaxStackAllocSize
            ? stackalloc byte[maxDecodedLength]
            : new byte[maxDecodedLength];

        var bytesWritten = Base64Url.DecodeFromChars(token.AsSpan(), bytes);
        bytes = bytes[..bytesWritten];
        var pos = 0;

        var v1 = ReadValue<T1>(bytes, ref pos);
        var v2 = ReadValue<T2>(bytes, ref pos);
        var v3 = ReadValue<T3>(bytes, ref pos);

        EnsureFullyConsumed(bytes, pos);

        return (v1, v2, v3);
    }

    #endregion

    #region Serialization

    private static int EstimateSize<T>(T value)
    {
        if (typeof(T) == typeof(bool))
            return 2;

        if (typeof(T) == typeof(byte))
            return 2;

        if (typeof(T) == typeof(DateTime))
            return 10;

        if (typeof(T) == typeof(DateTimeOffset))
            return 11;

        if (typeof(T) == typeof(decimal))
            return 17;

        if (typeof(T) == typeof(double))
            return 9;

        if (typeof(T) == typeof(Guid))
            return 17;

        if (typeof(T) == typeof(short))
            return 3;

        if (typeof(T) == typeof(int))
            return 5;

        if (typeof(T) == typeof(long))
            return 9;

        if (typeof(T) == typeof(string))
        {
            var text = Unsafe.As<T, string>(ref value) ?? string.Empty;
            return 5 + Encoding.UTF8.GetByteCount(text);
        }

        throw new NotSupportedException($"Type {typeof(T).Name} not supported");
    }

    private static int WriteValue<T>(Span<byte> buffer, T value)
    {
        if (typeof(T) == typeof(bool))
            return WriteBoolean(buffer, Unsafe.As<T, bool>(ref value));

        if (typeof(T) == typeof(byte))
            return WriteByte(buffer, Unsafe.As<T, byte>(ref value));

        if (typeof(T) == typeof(DateTime))
            return WriteDateTime(buffer, Unsafe.As<T, DateTime>(ref value));

        if (typeof(T) == typeof(DateTimeOffset))
            return WriteDateTimeOffset(buffer, Unsafe.As<T, DateTimeOffset>(ref value));

        if (typeof(T) == typeof(decimal))
            return WriteDecimal(buffer, Unsafe.As<T, decimal>(ref value));

        if (typeof(T) == typeof(double))
            return WriteDouble(buffer, Unsafe.As<T, double>(ref value));

        if (typeof(T) == typeof(Guid))
            return WriteGuid(buffer, Unsafe.As<T, Guid>(ref value));

        if (typeof(T) == typeof(short))
            return WriteInt16(buffer, Unsafe.As<T, short>(ref value));

        if (typeof(T) == typeof(int))
            return WriteInt32(buffer, Unsafe.As<T, int>(ref value));

        if (typeof(T) == typeof(long))
            return WriteInt64(buffer, Unsafe.As<T, long>(ref value));

        if (typeof(T) == typeof(string))
            return WriteString(buffer, Unsafe.As<T, string>(ref value));

        throw new NotSupportedException($"Type {typeof(T).Name} not supported");
    }

    private static T ReadValue<T>(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 1);

        var marker = (TypeMarker)buffer[position++];

        return marker switch
        {
            TypeMarker.Boolean when typeof(T) == typeof(bool) => As<bool, T>(ReadBoolean(buffer, ref position)),
            TypeMarker.Byte when typeof(T) == typeof(byte) => As<byte, T>(ReadByte(buffer, ref position)),
            TypeMarker.DateTime when typeof(T) == typeof(DateTime) => As<DateTime, T>(ReadDateTime(buffer, ref position)),
            TypeMarker.DateTimeOffset when typeof(T) == typeof(DateTimeOffset) => As<DateTimeOffset, T>(ReadDateTimeOffset(buffer, ref position)),
            TypeMarker.Decimal when typeof(T) == typeof(decimal) => As<decimal, T>(ReadDecimal(buffer, ref position)),
            TypeMarker.Double when typeof(T) == typeof(double) => As<double, T>(ReadDouble(buffer, ref position)),
            TypeMarker.Guid when typeof(T) == typeof(Guid) => As<Guid, T>(ReadGuid(buffer, ref position)),
            TypeMarker.Int16 when typeof(T) == typeof(short) => As<short, T>(ReadInt16(buffer, ref position)),
            TypeMarker.Int32 when typeof(T) == typeof(int) => As<int, T>(ReadInt32(buffer, ref position)),
            TypeMarker.Int64 when typeof(T) == typeof(long) => As<long, T>(ReadInt64(buffer, ref position)),
            TypeMarker.String when typeof(T) == typeof(string) => As<string, T>(ReadString(buffer, ref position)),
            _ => throw new InvalidOperationException($"Type mismatch or unsupported type"),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TTo As<TFrom, TTo>(TFrom value)
    {
        return Unsafe.As<TFrom, TTo>(ref value);
    }


    private static int WriteInt32(Span<byte> buffer, int value)
    {
        buffer[0] = (byte)TypeMarker.Int32;

        BinaryPrimitives.WriteInt32LittleEndian(buffer[1..], value);

        return 5;
    }

    private static int ReadInt32(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 4);

        var value = BinaryPrimitives.ReadInt32LittleEndian(buffer[position..]);
        position += 4;

        return value;
    }


    private static int WriteByte(Span<byte> buffer, byte value)
    {
        buffer[0] = (byte)TypeMarker.Byte;
        buffer[1] = value;

        return 2;
    }

    private static byte ReadByte(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 1);

        return buffer[position++];
    }


    private static int WriteInt16(Span<byte> buffer, short value)
    {
        buffer[0] = (byte)TypeMarker.Int16;

        BinaryPrimitives.WriteInt16LittleEndian(buffer[1..], value);

        return 3;
    }

    private static short ReadInt16(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 2);

        var value = BinaryPrimitives.ReadInt16LittleEndian(buffer[position..]);
        position += 2;

        return value;
    }


    private static int WriteInt64(Span<byte> buffer, long value)
    {
        buffer[0] = (byte)TypeMarker.Int64;

        BinaryPrimitives.WriteInt64LittleEndian(buffer[1..], value);

        return 9;
    }

    private static long ReadInt64(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 8);

        var value = BinaryPrimitives.ReadInt64LittleEndian(buffer[position..]);
        position += 8;

        return value;
    }


    private static int WriteDecimal(Span<byte> buffer, decimal value)
    {
        buffer[0] = (byte)TypeMarker.Decimal;

        // Decimal is composed of 4 32-bit integers (128 bits total)
        // This includes the significand and scale/sign information
        var bits = decimal.GetBits(value);

        // Serialize each 32-bit component in little-endian format
        BinaryPrimitives.WriteInt32LittleEndian(buffer[1..], bits[0]);   // Low bits
        BinaryPrimitives.WriteInt32LittleEndian(buffer[5..], bits[1]);   // Mid bits
        BinaryPrimitives.WriteInt32LittleEndian(buffer[9..], bits[2]);   // High bits
        BinaryPrimitives.WriteInt32LittleEndian(buffer[13..], bits[3]);  // Scale and sign

        return 17;
    }

    private static decimal ReadDecimal(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 16);

        Span<int> bits = stackalloc int[4];

        // Reconstruct the 4 32-bit components of the decimal value
        bits[0] = BinaryPrimitives.ReadInt32LittleEndian(buffer[position..]);        // Low bits
        bits[1] = BinaryPrimitives.ReadInt32LittleEndian(buffer[(position + 4)..]);  // Mid bits
        bits[2] = BinaryPrimitives.ReadInt32LittleEndian(buffer[(position + 8)..]);  // High bits
        bits[3] = BinaryPrimitives.ReadInt32LittleEndian(buffer[(position + 12)..]); // Scale and sign

        position += 16;

        return new decimal(bits);
    }


    private static int WriteDateTime(Span<byte> buffer, DateTime value)
    {
        buffer[0] = (byte)TypeMarker.DateTime;

        BinaryPrimitives.WriteInt64LittleEndian(buffer[1..], value.Ticks);

        // Store DateTime.Kind to preserve it during round-trip
        // 0 = Unspecified, 1 = Utc, 2 = Local
        buffer[9] = (byte)value.Kind;

        return 10;
    }

    private static DateTime ReadDateTime(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 9);

        var ticks = BinaryPrimitives.ReadInt64LittleEndian(buffer[position..]);
        position += 8;

        // Read DateTime.Kind to restore the original kind
        var kind = (DateTimeKind)buffer[position++];

        return new DateTime(ticks, kind);
    }


    private static int WriteDateTimeOffset(Span<byte> buffer, DateTimeOffset value)
    {
        buffer[0] = (byte)TypeMarker.DateTimeOffset;

        // Store local ticks and offset to preserve the original DateTimeOffset value.
        BinaryPrimitives.WriteInt64LittleEndian(buffer[1..], value.Ticks);

        // Store offset in minutes (2 bytes) to preserve the original time zone
        // Range: -14 hours to +14 hours (-840 to +840 minutes)
        BinaryPrimitives.WriteInt16LittleEndian(buffer[9..], (short)value.Offset.TotalMinutes);

        return 11;
    }

    private static DateTimeOffset ReadDateTimeOffset(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 10);

        var ticks = BinaryPrimitives.ReadInt64LittleEndian(buffer[position..]);
        position += 8;

        var offsetMinutes = BinaryPrimitives.ReadInt16LittleEndian(buffer[position..]);
        position += 2;

        return new DateTimeOffset(ticks, TimeSpan.FromMinutes(offsetMinutes));
    }


    private static int WriteGuid(Span<byte> buffer, Guid value)
    {
        buffer[0] = (byte)TypeMarker.Guid;
        value.TryWriteBytes(buffer[1..]);

        return 17;
    }

    private static Guid ReadGuid(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 16);

        var value = new Guid(buffer.Slice(position, 16));
        position += 16;

        return value;
    }


    private static int WriteDouble(Span<byte> buffer, double value)
    {
        buffer[0] = (byte)TypeMarker.Double;

        BinaryPrimitives.WriteDoubleLittleEndian(buffer[1..], value);

        return 9;
    }

    private static double ReadDouble(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 8);

        var value = BinaryPrimitives.ReadDoubleLittleEndian(buffer[position..]);
        position += 8;

        return value;
    }


    private static int WriteBoolean(Span<byte> buffer, bool value)
    {
        buffer[0] = (byte)TypeMarker.Boolean;
        buffer[1] = (byte)(value ? 1 : 0);

        return 2;
    }

    private static bool ReadBoolean(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 1);

        return buffer[position++] != 0;
    }


    private static int WriteString(Span<byte> buffer, string value)
    {
        value ??= string.Empty;

        buffer[0] = (byte)TypeMarker.String;

        // Convert string to UTF-8 bytes and write directly to buffer after length prefix
        var byteCount = Encoding.UTF8.GetBytes(value, buffer[5..]);

        // Write the byte count as a 4-byte prefix so we know how much to read back
        BinaryPrimitives.WriteInt32LittleEndian(buffer[1..], byteCount);

        return 5 + byteCount;
    }

    private static string ReadString(ReadOnlySpan<byte> buffer, ref int position)
    {
        EnsureAvailable(buffer, position, 4);

        // Read the 4-byte length prefix to determine how many UTF-8 bytes to decode
        var length = BinaryPrimitives.ReadInt32LittleEndian(buffer[position..]);
        position += 4;

        if (length < 0)
            throw new FormatException("Continuation token contains an invalid string length.");

        EnsureAvailable(buffer, position, length);

        // Decode the UTF-8 byte sequence back to a string
        var value = Encoding.UTF8.GetString(buffer.Slice(position, length));
        position += length;

        return value;
    }

    private static void EnsureAvailable(ReadOnlySpan<byte> buffer, int position, int count)
    {
        if ((uint)position > (uint)buffer.Length || count > buffer.Length - position)
            throw new FormatException("Continuation token is malformed or truncated.");
    }

    private static void EnsureFullyConsumed(ReadOnlySpan<byte> buffer, int position)
    {
        if (position != buffer.Length)
            throw new FormatException("Continuation token contains unexpected trailing data.");
    }

    #endregion

}
