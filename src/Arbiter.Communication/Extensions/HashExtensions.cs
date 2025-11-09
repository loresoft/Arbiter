using System.Buffers;
using System.IO.Hashing;
using System.Text;

namespace Arbiter.Communication.Extensions;

/// <summary>
/// Provides extension methods for computing hash values.
/// </summary>
public static class HashExtensions
{
    /// <summary>
    /// Computes the XXHash3 hash of the input string and returns it as a hexadecimal string.
    /// </summary>
    /// <param name="input">The string to hash.</param>
    /// <returns>A hexadecimal string representation of the XXHash3 hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <remarks>
    /// Uses stack allocation for strings that encode to 256 bytes or less.
    /// For larger strings, uses ArrayPool to rent a buffer for better memory efficiency.
    /// </remarks>
    public static string ToXXHash3(this string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        int maxByteCount = Encoding.UTF8.GetMaxByteCount(input.Length);

        byte[]? rentedArray = null;
        Span<byte> buffer = maxByteCount <= 256
            ? stackalloc byte[maxByteCount]
            : (rentedArray = ArrayPool<byte>.Shared.Rent(maxByteCount)).AsSpan(0, maxByteCount);

        try
        {
            int bytesWritten = Encoding.UTF8.GetBytes(input, buffer);

            Span<byte> hash = stackalloc byte[8];
            XxHash3.TryHash(buffer[..bytesWritten], hash, out _);

            return Convert.ToHexString(hash);
        }
        finally
        {
            if (rentedArray != null)
                ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }

}
