using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Arbiter.Services;

/// <summary>
/// Provides utilities for prefixing and extracting a UTF-8 encoded type name from binary payloads.
/// </summary>
/// <remarks>
/// Data format:
/// - First 4 bytes: big-endian 32-bit integer representing the byte length of the UTF-8 encoded type name.
/// - Next <c>length</c> bytes: UTF-8 encoded type name.
/// - Remaining bytes: payload.
///
/// The class uses <see cref="ArrayPool{T}"/> for temporary buffers where applicable and allocates a new array
/// when returning combined buffers. Methods are stateless and thread-safe.
///
/// Usage:
/// This helper is intended to be used by serializers to include type information in the buffer so that
/// receivers can determine the type prior to deserializing the payload.
/// </remarks>
public static class TypeBuffer
{
    /// <summary>
    /// Prefixes a UTF-8 encoded type name to the provided payload.
    /// </summary>
    /// <param name="typeName">The type name to prefix. This value is UTF-8 encoded.</param>
    /// <param name="payload">The payload to append after the type name. Not copied until placed in the resulting buffer.</param>
    /// <returns>
    /// A new byte array containing: the 4-byte big-endian length of the type name, the UTF-8 bytes of the type name,
    /// followed by the payload bytes.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="typeName"/> is null or empty.</exception>
    /// <remarks>
    /// The first 4 bytes represent the length of the type name encoded as a big-endian 32-bit integer. The type name
    /// is encoded using UTF-8. The returned buffer is newly allocated to contain both the type name and payload.
    /// </remarks>
    public static ReadOnlyMemory<byte> Prefix(string typeName, ReadOnlySpan<byte> payload)
    {
        if (string.IsNullOrEmpty(typeName))
            throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));

        // Compute exact byte count for UTF-8 encoding
        var typeNameByteCount = Encoding.UTF8.GetByteCount(typeName);

        // Allocate final buffer: 4 bytes for length + type name + payload
        var buffer = new byte[4 + typeNameByteCount + payload.Length];
        var bufferSpan = buffer.AsSpan();

        // Write type name length (4 bytes, big-endian)
        BinaryPrimitives.WriteInt32BigEndian(bufferSpan, typeNameByteCount);

        // Encode type name directly into the buffer
        var written = Encoding.UTF8.GetBytes(typeName, bufferSpan.Slice(4, typeNameByteCount));

        // Write payload
        payload.CopyTo(bufferSpan[(4 + written)..]);

        return buffer;
    }

    /// <summary>
    /// Extracts the type name and payload from a buffer produced by <see cref="Prefix"/>.
    /// </summary>
    /// <param name="prefixedBuffer">The buffer containing the 4-byte big-endian length, the UTF-8 type name, and the payload.</param>
    /// <returns>
    /// A tuple containing the extracted type name and the payload as <see cref="ReadOnlyMemory{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the buffer is too short to contain type information or when the buffer is corrupted.
    /// </exception>
    /// <remarks>
    /// The type name length is read as a big-endian 32-bit integer from the first 4 bytes. The type name is decoded
    /// using UTF-8. The payload is copied into a new buffer to produce a <see cref="ReadOnlyMemory{T}"/> value.
    /// </remarks>
    public static (string TypeName, ReadOnlyMemory<byte> Payload) Extract(ReadOnlySpan<byte> prefixedBuffer)
    {
        if (prefixedBuffer.Length < 4)
            throw new ArgumentException("Buffer is too short to contain type information", nameof(prefixedBuffer));

        // Read type name length (4 bytes, big-endian)
        var typeNameLength = BinaryPrimitives.ReadInt32BigEndian(prefixedBuffer);

        if (prefixedBuffer.Length < 4 + typeNameLength)
            throw new ArgumentException("Buffer is corrupted: type name length exceeds buffer size", nameof(prefixedBuffer));

        // Read type name
        var typeNameBytes = prefixedBuffer.Slice(4, typeNameLength);
        var typeName = Encoding.UTF8.GetString(typeNameBytes);

        // Extract payload - return as ReadOnlyMemory to avoid copying
        var payloadStart = 4 + typeNameLength;
        var payloadLength = prefixedBuffer.Length - payloadStart;

        // We need to copy to return ReadOnlyMemory since we're working with a span
        var payloadCopy = new byte[payloadLength];
        prefixedBuffer[payloadStart..].CopyTo(payloadCopy);

        return (typeName, payloadCopy);
    }

    /// <summary>
    /// Reads the type name from a buffer produced by <see cref="Prefix"/> without extracting the payload.
    /// </summary>
    /// <param name="prefixedBuffer">The buffer containing the 4-byte big-endian length and the UTF-8 type name.</param>
    /// <returns>The type name extracted from the buffer.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the buffer is too short to contain type information or when the buffer is corrupted.
    /// </exception>
    /// <remarks>
    /// This method reads only the type name length and decodes the UTF-8 type name. It does not read or copy the payload.
    /// </remarks>
    public static string Peek(ReadOnlySpan<byte> prefixedBuffer)
    {
        if (prefixedBuffer.Length < 4)
            throw new ArgumentException("Buffer is too short to contain type information", nameof(prefixedBuffer));

        // Read type name length
        var typeNameLength = BinaryPrimitives.ReadInt32BigEndian(prefixedBuffer);

        if (prefixedBuffer.Length < 4 + typeNameLength)
            throw new ArgumentException("Buffer is corrupted: type name length exceeds buffer size", nameof(prefixedBuffer));

        // Read type name
        var typeNameBytes = prefixedBuffer.Slice(4, typeNameLength);
        return Encoding.UTF8.GetString(typeNameBytes);
    }
}
