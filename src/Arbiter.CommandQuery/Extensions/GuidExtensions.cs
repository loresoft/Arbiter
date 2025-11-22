using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Extension methods for Guid to generate sequential UUIDs compatible with SQL Server.
/// </summary>
public static class GuidExtensions
{
    private static long _lastTicks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetMonotonicTicks()
    {
        // Hybrid approach: Use DateTime.UtcNow as base, but ensure monotonic ordering
        // This survives reboots while being resilient to small clock adjustments
        long currentTicks = DateTime.UtcNow.Ticks;

        while (true)
        {
            long lastTicks = Volatile.Read(ref _lastTicks);
            long newTicks;

            // Determine the new tick value
            if (currentTicks > lastTicks)
            {
                // Guard against large backward jumps (>1 second)
                if (lastTicks > 0 && currentTicks < lastTicks - TimeSpan.TicksPerSecond)
                    newTicks = lastTicks + 1;
                else
                    newTicks = currentTicks;
            }
            else
            {
                // Clock hasn't moved forward, increment from last value
                newTicks = lastTicks + 1;
            }

            // Try to atomically update _lastTicks
            long original = Interlocked.CompareExchange(ref _lastTicks, newTicks, lastTicks);

            // If we successfully updated, return the new value
            if (original == lastTicks)
                return newTicks;

            // Another thread updated _lastTicks, retry with new value
            // The loop will read the updated value and try again
        }
    }

    extension(Guid)
    {
        /// <summary>
        /// Generates a new sequential UUID v8 compatible with SQL Server's NewSequentialID pattern.
        /// The UUID is ordered by timestamp in the first 6 bytes for optimal B-tree insertion.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid NewSequentialId()
        {
            Span<byte> bytes = stackalloc byte[16];

            // Get monotonically increasing timestamp
            long ticks = GetMonotonicTicks();

            // First 8 bytes: Write timestamp as big-endian (we only use first 6 bytes, but writing 8 is faster)
            // This ensures sequential ordering in SQL Server
            BinaryPrimitives.WriteInt64BigEndian(bytes, ticks);

            // Generate cryptographically random bytes directly into remaining slots (bytes 6-15)
            RandomNumberGenerator.Fill(bytes[6..]);

            // Set version to 8 (custom UUID) - bits 48-51 (byte 6, upper nibble)
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x80);

            // Set variant to RFC 9562 (10xx) - bits 64-65 (byte 8, upper 2 bits)
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

            return new Guid(bytes);
        }
    }
}
