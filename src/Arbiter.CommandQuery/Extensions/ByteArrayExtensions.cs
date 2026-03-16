namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Provides extension methods for reordering raw <see cref="byte"/> span data
/// between standard .NET <see cref="Guid"/> byte ordering and SQL Server byte ordering.
/// </summary>
public static class ByteArrayExtensions
{
    extension(ReadOnlySpan<byte> bytes)
    {
        /// <summary>
        /// Writes the bytes of a .NET <see cref="Guid"/> into <paramref name="destination"/>
        /// reordered for SQL Server byte ordering.
        /// </summary>
        /// <param name="destination">The destination span that receives the reordered bytes. Must be at least 16 bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> or the source span is shorter than 16 bytes.</exception>
        public void WriteToSqlByteOrder(Span<byte> destination)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bytes.Length, 16, nameof(bytes));
            ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, 16, nameof(destination));

            // Data4[4..7] (rand_b high bytes) → dest[0..3]: least significant in SQL Server sort order
            destination[0] = bytes[12];
            destination[1] = bytes[13];
            destination[2] = bytes[14];
            destination[3] = bytes[15];

            // Data4[2..3] (rand_b) → dest[4..5]
            destination[4] = bytes[10];
            destination[5] = bytes[11];

            // Data4[0..1] (rand_b low bytes) → dest[6..7]
            destination[6] = bytes[8];
            destination[7] = bytes[9];

            // Data3 (version + rand_a) → dest[8..9]: swap .NET little-endian to big-endian
            destination[8] = bytes[7];
            destination[9] = bytes[6];

            // Data1 (unix_ts_ms high 32 bits) → dest[10..13]: reverse .NET little-endian to big-endian; most significant in SQL Server sort order
            destination[10] = bytes[3];
            destination[11] = bytes[2];
            destination[12] = bytes[1];
            destination[13] = bytes[0];

            // Data2 (unix_ts_ms bits 32–47) → dest[14..15]: reverse .NET little-endian to big-endian
            destination[14] = bytes[5];
            destination[15] = bytes[4];
        }

        /// <summary>
        /// Writes the bytes of a SQL Server byte-ordered <see cref="Guid"/> into <paramref name="destination"/>
        /// restored to standard .NET byte ordering.
        /// </summary>
        /// <param name="destination">The destination span that receives the reordered bytes. Must be at least 16 bytes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> or the source span is shorter than 16 bytes.</exception>
        public void WriteFromSqlByteOrder(Span<byte> destination)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(bytes.Length, 16, nameof(bytes));
            ArgumentOutOfRangeException.ThrowIfLessThan(destination.Length, 16, nameof(destination));

            // dest[0..3] (Data1, unix_ts_ms high 32 bits): reverse SQL big-endian back to .NET little-endian
            destination[0] = bytes[13];
            destination[1] = bytes[12];
            destination[2] = bytes[11];
            destination[3] = bytes[10];

            // dest[4..5] (Data2, unix_ts_ms bits 32–47): reverse SQL big-endian back to .NET little-endian
            destination[4] = bytes[15];
            destination[5] = bytes[14];

            // dest[6..7] (Data3, version + rand_a): swap SQL big-endian back to .NET little-endian
            destination[6] = bytes[9];
            destination[7] = bytes[8];

            // dest[8..9] (Data4[0..1], rand_b low bytes)
            destination[8] = bytes[6];
            destination[9] = bytes[7];

            // dest[10..11] (Data4[2..3], rand_b)
            destination[10] = bytes[4];
            destination[11] = bytes[5];

            // dest[12..15] (Data4[4..7], rand_b high bytes): least significant in SQL Server sort order
            destination[12] = bytes[0];
            destination[13] = bytes[1];
            destination[14] = bytes[2];
            destination[15] = bytes[3];
        }
    }
}
