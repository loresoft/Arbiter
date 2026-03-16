using System.Security.Cryptography;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for converting <see cref="Guid"/> values
/// between standard .NET byte ordering and SQL Server byte ordering.
/// </summary>
public static class GuidExtensions
{
    extension(Guid)
    {
        /// <summary>
        /// Creates a new SQL Server-compatible sequential GUID using UUID version 7.
        /// </summary>
        /// <param name="timestamp">
        /// The timestamp to embed in the UUID. Defaults to <see cref="DateTimeOffset.UtcNow"/> when <see langword="null"/>.
        /// </param>
        /// <returns>A new <see cref="Guid"/> reordered for optimal SQL Server index performance.</returns>
        public static Guid NewSqlGuid(DateTimeOffset? timestamp = null)
        {
            timestamp ??= DateTimeOffset.UtcNow;

#if NET9_0_OR_GREATER
            return Guid.CreateVersion7(timestamp.Value).ToSqlGuid();
#else
            return CreateVersion7(timestamp.Value).ToSqlGuid();
#endif
        }

#if !NET9_0_OR_GREATER
        /// <summary>
        /// Creates a new UUID version 7 using the current UTC time as the timestamp.
        /// </summary>
        /// <returns>A new time-ordered <see cref="Guid"/> conforming to RFC 9562 UUID version 7.</returns>
        public static Guid CreateVersion7()
            => CreateVersion7(DateTimeOffset.UtcNow);

        /// <summary>
        /// Creates a new UUID version 7 using the specified <paramref name="timestamp"/>.
        /// </summary>
        /// <param name="timestamp">The timestamp to embed as the 48-bit Unix millisecond epoch in the UUID.</param>
        /// <returns>A new time-ordered <see cref="Guid"/> conforming to RFC 9562 UUID version 7.</returns>
        public static Guid CreateVersion7(DateTimeOffset timestamp)
        {
            var unixMilliseconds = timestamp.ToUnixTimeMilliseconds();

            var timeBytes = BitConverter.GetBytes(unixMilliseconds);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            var uuidBytes = new byte[16];
            timeBytes[2..8].CopyTo(uuidBytes, 0);

            var randomBytes = uuidBytes.AsSpan().Slice(6);

            RandomNumberGenerator.Fill(randomBytes);

            uuidBytes[6] &= 0x0F;
            uuidBytes[6] += 0x70;

            return new(uuidBytes, bigEndian: true);
        }
#endif

    }

    extension(Guid id)
    {
        /// <summary>
        /// Attempts to extract the UTC timestamp embedded in a UUIDv7 <see cref="Guid"/>.
        /// Both standard .NET byte ordering and SQL Server byte-ordered representations are handled.
        /// </summary>
        /// <returns>
        /// The <see cref="DateTimeOffset"/> (UTC) extracted from the UUID timestamp, or <see langword="null"/>
        /// if the <see cref="Guid"/> does not contain a version 7 timestamp.
        /// </returns>
        public DateTimeOffset? ToTimestamp()
        {
            Span<byte> bytes = stackalloc byte[16];
            id.TryWriteBytes(bytes);

            // Standard .NET UUIDv7: version nibble is the high nibble of bytes[7] (Data3 high byte)
            if ((bytes[7] >> 4) == 7)
                return ExtractTimestamp(bytes);

            // Could be a SQL GUID — normalize byte ordering and retry
            Span<byte> normalized = stackalloc byte[16];
            bytes.WriteFromSqlByteOrder(normalized);

            if ((normalized[7] >> 4) == 7)
                return ExtractTimestamp(normalized);

            return null;
        }

        /// <summary>
        /// Converts a <see cref="Guid"/> to a SQL Server-compatible byte ordering
        /// for optimal sequential index performance.
        /// </summary>
        /// <returns>A new <see cref="Guid"/> with bytes reordered for SQL Server.</returns>
        public Guid ToSqlGuid()
        {
            Span<byte> src = stackalloc byte[16];
            id.TryWriteBytes(src);

            Span<byte> dst = stackalloc byte[16];
            src.WriteToSqlByteOrder(dst);

            return new(dst);
        }

        /// <summary>
        /// Converts a SQL Server byte-ordered <see cref="Guid"/> back to standard .NET byte ordering.
        /// </summary>
        /// <returns>A new <see cref="Guid"/> with bytes restored to standard .NET order.</returns>
        public Guid FromSqlGuid()
        {
            Span<byte> src = stackalloc byte[16];
            id.TryWriteBytes(src);

            Span<byte> dst = stackalloc byte[16];
            src.WriteFromSqlByteOrder(dst);

            return new(dst);
        }

    }

    // Data1 (LE int32) carries UUID bits 0–31 of the 48-bit timestamp; Data2 (LE int16) carries bits 32–47.
    // Reversing the little-endian field storage reconstructs the original big-endian Unix millisecond value.
    private static DateTimeOffset ExtractTimestamp(ReadOnlySpan<byte> bytes)
    {
        var unixMs = ((long)bytes[3] << 40)
                   | ((long)bytes[2] << 32)
                   | ((long)bytes[1] << 24)
                   | ((long)bytes[0] << 16)
                   | ((long)bytes[5] <<  8)
                   |        bytes[4];

        return DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
    }
}
