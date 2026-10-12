using System.Buffers.Binary;
using System.Numerics;

namespace Arbiter.Services;

/// <summary>
/// Fully managed SHA3-512 (FIPS 202) implementation used when the platform
/// does not provide SHA3-512 (e.g. browser WebAssembly).
/// </summary>
internal static class Sha3512
{
    public const int HashSizeInBytes = 64;

    // rate = 1600 - 2 * 512 bits = 576 bits
    private const int RateInBytes = 72;
    private const int RateInLanes = RateInBytes / 8;

    private static ReadOnlySpan<ulong> RoundConstants =>
    [
        0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808AUL, 0x8000000080008000UL,
        0x000000000000808BUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
        0x000000000000008AUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000AUL,
        0x000000008000808BUL, 0x800000000000008BUL, 0x8000000000008089UL, 0x8000000000008003UL,
        0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800AUL, 0x800000008000000AUL,
        0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL,
    ];

    private static ReadOnlySpan<int> Rotations =>
    [
        1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 2, 14,
        27, 41, 56, 8, 25, 43, 62, 18, 39, 61, 20, 44,
    ];

    private static ReadOnlySpan<int> PiLanes =>
    [
        10, 7, 11, 17, 18, 3, 5, 16, 8, 21, 24, 4,
        15, 23, 19, 13, 12, 2, 20, 14, 22, 9, 6, 1,
    ];

    public static void HashData(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        if (destination.Length < HashSizeInBytes)
            throw new ArgumentException("Destination is too short.", nameof(destination));

        Span<ulong> state = stackalloc ulong[25];
        state.Clear();

        while (source.Length >= RateInBytes)
        {
            Absorb(state, source[..RateInBytes]);
            source = source[RateInBytes..];
        }

        // final block with SHA3 domain padding (0x06 ... 0x80)
        Span<byte> block = stackalloc byte[RateInBytes];
        block.Clear();
        source.CopyTo(block);
        block[source.Length] ^= 0x06;
        block[RateInBytes - 1] ^= 0x80;
        Absorb(state, block);

        for (int i = 0; i < HashSizeInBytes / 8; i++)
            BinaryPrimitives.WriteUInt64LittleEndian(destination[(i * 8)..], state[i]);
    }

    private static void Absorb(Span<ulong> state, ReadOnlySpan<byte> block)
    {
        for (int i = 0; i < RateInLanes; i++)
            state[i] ^= BinaryPrimitives.ReadUInt64LittleEndian(block[(i * 8)..]);

        Permute(state);
    }

    private static void Permute(Span<ulong> state)
    {
        Span<ulong> c = stackalloc ulong[5];

        for (int round = 0; round < 24; round++)
        {
            // theta
            for (int x = 0; x < 5; x++)
                c[x] = state[x] ^ state[x + 5] ^ state[x + 10] ^ state[x + 15] ^ state[x + 20];

            for (int x = 0; x < 5; x++)
            {
                ulong d = c[(x + 4) % 5] ^ BitOperations.RotateLeft(c[(x + 1) % 5], 1);
                for (int y = 0; y < 25; y += 5)
                    state[y + x] ^= d;
            }

            // rho and pi
            ulong current = state[1];
            for (int i = 0; i < 24; i++)
            {
                int lane = PiLanes[i];
                ulong temp = state[lane];
                state[lane] = BitOperations.RotateLeft(current, Rotations[i]);
                current = temp;
            }

            // chi
            for (int y = 0; y < 25; y += 5)
            {
                for (int x = 0; x < 5; x++)
                    c[x] = state[y + x];

                for (int x = 0; x < 5; x++)
                    state[y + x] = c[x] ^ (~c[(x + 1) % 5] & c[(x + 2) % 5]);
            }

            // iota
            state[0] ^= RoundConstants[round];
        }
    }
}
