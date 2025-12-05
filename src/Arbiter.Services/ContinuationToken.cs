using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Arbiter.Services;

/// <summary>
/// Represents a continuation token for pagination that combines an identifier and an optional timestamp.
/// </summary>
/// <typeparam name="T">The type of the identifier. Must be an unmanaged type.</typeparam>
/// <remarks>
/// <para>
/// The <see cref="ContinuationToken{T}"/> provides a stateless pagination mechanism by encoding an identifier
/// and optional timestamp into a compact, URL-safe Base64 string. This approach is more efficient than traditional
/// page-based pagination for large datasets and avoids consistency issues when data changes between requests.
/// </para>
/// <para>
/// The token uses little-endian encoding for cross-platform consistency and supports common unmanaged types including:
/// <list type="bullet">
/// <item><description>Numeric types: <see cref="int"/>, <see cref="long"/>, <see cref="short"/>, <see cref="byte"/>, and unsigned variants</description></item>
/// <item><description>Floating-point types: <see cref="float"/>, <see cref="double"/></description></item>
/// <item><description><see cref="Guid"/> for unique identifiers</description></item>
/// <item><description>Other unmanaged value types</description></item>
/// </list>
/// </para>
/// <para>
/// The optional timestamp component stores UTC ticks and can be used for time-based pagination or audit tracking.
/// When serialized, the token is encoded as a Base64URL string that is safe for use in URLs and HTTP headers.
/// </para>
/// </remarks>
/// <example>
/// <para><strong>Basic Usage with Entity Framework Core</strong></para>
/// <para>
/// The following example demonstrates keyset pagination using continuation tokens.
/// This is more efficient than SKIP/TAKE because it uses indexed WHERE clauses:
/// </para>
/// <code>
/// var query = dbContext.Products
///     .AsNoTracking()
///     .Where(p => p.IsActive);
///
/// // Parse continuation token to get the last seen ID
/// if (ContinuationToken&lt;int&gt;.TryParse(continuationToken, out var token))
/// {
///     // Fetch items after the last seen ID (keyset pagination)
///     query = query.Where(p => p.Id &gt; token.Id);
/// }
///
/// // Take one more than requested to check if there are more items
/// var pageSize = 20;
/// var items = await query
///     .OrderBy(p => p.Id)
///     .Take(pageSize + 1)
///     .ToListAsync();
///
/// // Check if there are more items and create continuation token
/// string? nextToken = null;
/// if (items.Count &gt; pageSize)
/// {
///     items.RemoveAt(items.Count - 1);
///     var lastItem = items[^1];
///     nextToken = new ContinuationToken&lt;int&gt;(lastItem.Id).ToString();
/// }
/// </code>
///
/// <para><strong>Time-Based Pagination</strong></para>
/// <para>
/// Using timestamp with ID as a tie-breaker for ordering by date:
/// </para>
/// <code>
/// var query = dbContext.Events.AsNoTracking();
///
/// // Parse token to get last seen timestamp and ID
/// if (ContinuationToken&lt;int&gt;.TryParse(continuationToken, out var token))
/// {
///     // Keyset pagination with composite key (timestamp + ID)
///     query = query.Where(e =>
///         e.CreatedDate &gt; token.Timestamp ||
///         (e.CreatedDate == token.Timestamp &amp;&amp; e.Id &gt; token.Id));
/// }
///
/// // Take one more than requested to check if there are more items
/// var pageSize = 50;
/// var events = await query
///     .OrderBy(e => e.CreatedDate)
///     .ThenBy(e => e.Id)
///     .Take(pageSize + 1)
///     .ToListAsync();
///
/// // Check if there are more items and create continuation token
/// string? nextToken = null;
/// if (events.Count &gt; pageSize)
/// {
///     events.RemoveAt(events.Count - 1);
///     var last = events[^1];
///     nextToken = new ContinuationToken&lt;int&gt;(last.Id, last.CreatedDate).ToString();
/// }
/// </code>
/// </example>
[DebuggerDisplay("Id = {Id}, Timestamp = {Timestamp}")]
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ContinuationToken<T>
    where T : unmanaged
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContinuationToken{T}"/> struct.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    /// <param name="timestamp">The optional timestamp associated with the token.</param>
    public ContinuationToken(T id, DateTimeOffset? timestamp = null)
    {
        Id = id;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the identifier value.
    /// </summary>
    public T Id { get; }

    /// <summary>
    /// Gets the optional timestamp associated with the token.
    /// </summary>
    public DateTimeOffset? Timestamp { get; }


    /// <summary>
    /// Converts the continuation token to a Base64URL-encoded string representation.
    /// </summary>
    /// <returns>A Base64URL-encoded string containing the identifier and optional date.</returns>
    public override readonly string ToString()
    {
        int idSize = Unsafe.SizeOf<T>();

        // Allocate: idSize for Id + 1 for hasDate flag + (optional: 8 for ticks)
        int totalSize = Timestamp.HasValue ? idSize + 9 : idSize + 1;
        Span<byte> buffer = stackalloc byte[totalSize];

        // Write Id with explicit little-endian encoding
        WriteValueLittleEndian(Id, buffer);

        // Write hasDate flag
        buffer[idSize] = Timestamp.HasValue ? (byte)1 : (byte)0;

        // Write Date ticks if present
        if (Timestamp.HasValue)
            BinaryPrimitives.WriteInt64LittleEndian(buffer[(idSize + 1)..], Timestamp.Value.UtcTicks);

        // Convert to Base64URL
        int encodedLength = Base64Url.GetEncodedLength(totalSize);
        Span<byte> encoded = stackalloc byte[encodedLength];
        Base64Url.EncodeToUtf8(buffer, encoded, out _, out _);

        return System.Text.Encoding.UTF8.GetString(encoded);
    }


    /// <summary>
    /// Attempts to parse a Base64URL-encoded string into a continuation token.
    /// </summary>
    /// <param name="token">The Base64URL-encoded token string to parse.</param>
    /// <param name="result">When this method returns, contains the parsed continuation token if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if the token was successfully parsed; otherwise, <c>false</c>.</returns>
    [SuppressMessage("Design", "MA0018:Do not declare static members on generic types", Justification = "Safe")]
    public static bool TryParse(string? token, out ContinuationToken<T> result)
    {
        result = default;

        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            int idSize = Unsafe.SizeOf<T>();

            // Decode from Base64URL
            var tokenByteCount = System.Text.Encoding.UTF8.GetByteCount(token);
            Span<byte> encodedBytes = stackalloc byte[tokenByteCount];
            System.Text.Encoding.UTF8.GetBytes(token, encodedBytes);

            int maxDecodedLength = Base64Url.GetMaxDecodedLength(encodedBytes.Length);
            Span<byte> buffer = stackalloc byte[maxDecodedLength];

            if (Base64Url.DecodeFromUtf8(encodedBytes, buffer, out _, out int bytesWritten) != OperationStatus.Done)
                return false;

            buffer = buffer[..bytesWritten];

            // Validate minimum size (idSize + 1 for hasDate flag)
            if (buffer.Length < idSize + 1)
                return false;

            // Read hasDate flag first to determine expected size
            bool hasDate = buffer[idSize] == 1;

            // Calculate expected size and validate exact match
            int expectedSize = hasDate ? idSize + 9 : idSize + 1;
            if (buffer.Length != expectedSize)
                return false;

            // Read Id with explicit little-endian decoding
            T id = ReadValueLittleEndian<T>(buffer);

            DateTimeOffset? date = null;
            if (hasDate)
            {
                long ticks = BinaryPrimitives.ReadInt64LittleEndian(buffer[(idSize + 1)..]);
                date = new DateTimeOffset(ticks, TimeSpan.Zero);
            }

            result = new ContinuationToken<T>(id, date);
            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Writes a value to a buffer using little-endian encoding.
    /// </summary>
    /// <typeparam name="TValue">The type of value to write. Must be an unmanaged type.</typeparam>
    /// <param name="value">The value to write.</param>
    /// <param name="buffer">The destination buffer.</param>
    private static void WriteValueLittleEndian<TValue>(TValue value, Span<byte> buffer)
        where TValue : unmanaged
    {
        // Handle common numeric types with explicit endianness
        if (typeof(TValue) == typeof(short))
            BinaryPrimitives.WriteInt16LittleEndian(buffer, Unsafe.As<TValue, short>(ref value));
        else if (typeof(TValue) == typeof(ushort))
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, Unsafe.As<TValue, ushort>(ref value));
        else if (typeof(TValue) == typeof(int))
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Unsafe.As<TValue, int>(ref value));
        else if (typeof(TValue) == typeof(uint))
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, Unsafe.As<TValue, uint>(ref value));
        else if (typeof(TValue) == typeof(long))
            BinaryPrimitives.WriteInt64LittleEndian(buffer, Unsafe.As<TValue, long>(ref value));
        else if (typeof(TValue) == typeof(ulong))
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, Unsafe.As<TValue, ulong>(ref value));
        else if (typeof(TValue) == typeof(float))
            BinaryPrimitives.WriteSingleLittleEndian(buffer, Unsafe.As<TValue, float>(ref value));
        else if (typeof(TValue) == typeof(double))
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, Unsafe.As<TValue, double>(ref value));
        else if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
            // Single byte types don't need endianness conversion
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        else if (typeof(TValue) == typeof(Guid))
            Unsafe.As<TValue, Guid>(ref value).TryWriteBytes(buffer);
        else
            // For other unmanaged types, use unaligned write
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
    }

    /// <summary>
    /// Reads a value from a buffer using little-endian encoding.
    /// </summary>
    /// <typeparam name="TValue">The type of value to read. Must be an unmanaged type.</typeparam>
    /// <param name="buffer">The source buffer.</param>
    /// <returns>The value read from the buffer.</returns>
    private static TValue ReadValueLittleEndian<TValue>(Span<byte> buffer)
        where TValue : unmanaged
    {
        // Handle common numeric types with explicit endianness
        if (typeof(TValue) == typeof(short))
        {
            short value = BinaryPrimitives.ReadInt16LittleEndian(buffer);
            return Unsafe.As<short, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(ushort))
        {
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(buffer);
            return Unsafe.As<ushort, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(int))
        {
            int value = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return Unsafe.As<int, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(uint))
        {
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
            return Unsafe.As<uint, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(long))
        {
            long value = BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return Unsafe.As<long, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(ulong))
        {
            ulong value = BinaryPrimitives.ReadUInt64LittleEndian(buffer);
            return Unsafe.As<ulong, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(float))
        {
            float value = BinaryPrimitives.ReadSingleLittleEndian(buffer);
            return Unsafe.As<float, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(double))
        {
            double value = BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            return Unsafe.As<double, TValue>(ref value);
        }
        else if (typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(sbyte))
        {
            // Single byte types don't need endianness conversion
            return Unsafe.ReadUnaligned<TValue>(ref MemoryMarshal.GetReference(buffer));
        }
        else if (typeof(TValue) == typeof(Guid))
        {
            Guid value = new(buffer[..16]);
            return Unsafe.As<Guid, TValue>(ref value);
        }
        else
        {
            // For other unmanaged types, use unaligned read
            // Note: This may still have endianness issues for complex structs
            return Unsafe.ReadUnaligned<TValue>(ref MemoryMarshal.GetReference(buffer));
        }
    }
}
