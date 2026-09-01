using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Arbiter.Services;

/// <summary>
/// A high-performance, low-allocation mutable string builder using a Span-based buffer.
/// Uses stack or pooled memory depending on constructor. Designed for scenarios where minimizing allocations is critical.
/// </summary>
/// <remarks>
/// <para>
/// Always call <see cref="Dispose"/> when finished, typically from a <c language="C#">finally</c> block, so any
/// rented buffer is returned to <see cref="ArrayPool{T}.Shared"/>.
/// </para>
/// <para>
/// This is a mutable <see langword="struct"/>; pass it by <see langword="ref"/> and never copy it,
/// as writes through a copy are not observed by the original and a stale copy may reference a
/// buffer that has already been returned to the pool.
/// </para>
/// </remarks>
public ref struct ValueStringBuilder
{
    /// <summary>
    /// Represents the default initial capacity for the builder.
    /// </summary>
    /// <remarks>
    /// This constant defines the default size, in characters, that a builder can hold when
    /// initialized without specifying a capacity.
    /// </remarks>
    public const int DefaultCapacity = 256;

    private Span<char> _buffer;          // Current character buffer
    private char[]? _arrayFromPool;      // Array rented from pool
    private int _position;               // Current write position
    private bool _disposed;              // Flag indicating if the builder has been disposed

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct using a user-supplied stack-allocated buffer.
    /// </summary>
    /// <param name="buffer">The stack-allocated buffer to use for the builder's storage.</param>
    public ValueStringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
        _arrayFromPool = null;
        _position = 0;
        _disposed = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct with a pooled buffer of the specified capacity.
    /// </summary>
    /// <param name="capacity">The minimum number of characters the builder can initially store.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="capacity"/> is not positive.</exception>
    public ValueStringBuilder(int capacity = DefaultCapacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

        _arrayFromPool = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = _arrayFromPool;
        _position = 0;
        _disposed = false;
    }

    /// <summary>
    /// Gets the length of the current written content in the builder.
    /// </summary>
    public readonly int Length => _position;

    /// <summary>
    /// Appends a single character to the end of the builder.
    /// </summary>
    /// <param name="value">The character to append.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value)
    {
        ThrowIfDisposed();

        if (_position < _buffer.Length)
            _buffer[_position++] = value;
        else
            GrowAndAppend(value);
    }

    /// <summary>
    /// Appends the specified string to the end of the builder.
    /// </summary>
    /// <param name="value">The string to append. If <see langword="null"/>, no action is taken.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? value)
    {
        ThrowIfDisposed();

        if (value == null)
            return;

        int required = _position + value.Length;
        if (required > _buffer.Length)
            Grow(required);

        value.AsSpan().CopyTo(_buffer[_position..]);
        _position += value.Length;
    }

    /// <summary>
    /// Appends a span of characters to the end of the builder.
    /// </summary>
    /// <param name="value">The span of characters to append.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void Append(scoped ReadOnlySpan<char> value)
    {
        ThrowIfDisposed();

        int required = _position + value.Length;
        if (required > _buffer.Length)
            Grow(required);

        value.CopyTo(_buffer[_position..]);
        _position += value.Length;
    }

    /// <summary>
    /// Appends a character to the builder multiple times.
    /// </summary>
    /// <param name="value">The character to append.</param>
    /// <param name="count">The number of times to append the character. If less than or equal to zero, no action is taken.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void Append(char value, int count)
    {
        ThrowIfDisposed();

        if (count <= 0)
            return;

        int required = _position + count;
        if (required > _buffer.Length) Grow(required);

        for (int i = 0; i < count; i++)
            _buffer[_position++] = value;
    }

    /// <summary>
    /// Inserts a single character at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the character.</param>
    /// <param name="value">The character to insert.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or greater than <see cref="Length"/>.</exception>
    public void Insert(int index, char value)
    {
        ThrowIfDisposed();

        if ((uint)index > (uint)_position)
            throw new ArgumentOutOfRangeException(nameof(index));

        int required = _position + 1;
        if (required > _buffer.Length)
            Grow(required);

        _buffer[index.._position].CopyTo(_buffer[(index + 1)..]);
        _buffer[index] = value;
        _position++;
    }

    /// <summary>
    /// Inserts the specified string at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which to insert the string.</param>
    /// <param name="value">The string to insert. If <see langword="null"/> or empty, no action is taken.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is negative or greater than <see cref="Length"/>.</exception>
    public void Insert(int index, string? value)
    {
        ThrowIfDisposed();

        if ((uint)index > (uint)_position)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (string.IsNullOrEmpty(value))
            return;

        int required = _position + value.Length;
        if (required > _buffer.Length)
            Grow(required);

        _buffer[index.._position].CopyTo(_buffer[(index + value.Length)..]);
        value.AsSpan().CopyTo(_buffer[index..]);
        _position += value.Length;
    }

    /// <summary>
    /// Appends the string representation of the specified value to the current instance.
    /// </summary>
    /// <remarks>
    /// This method formats the value directly into the internal buffer. If the buffer does not have
    /// enough space, it is resized and the formatting is retried, growing the buffer as needed.
    /// The value is formatted using the default format and the current culture.
    /// </remarks>
    /// <typeparam name="T">The type of the value to append. Must implement <see cref="ISpanFormattable"/>.</typeparam>
    /// <param name="value">The value to append. The value is formatted using its <see cref="ISpanFormattable.TryFormat"/> implementation.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void Append<T>(T value) where T : ISpanFormattable
    {
        ThrowIfDisposed();

        // Try to format directly into the buffer, growing until the value fits.
        int charsWritten;
        while (!value.TryFormat(_buffer[_position..], out charsWritten, format: null, provider: null))
            Grow(_buffer.Length + 1);

        _position += charsWritten;
    }

    /// <summary>
    /// Appends the platform-specific line terminator to the builder.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void AppendLine()
        => Append(Environment.NewLine.AsSpan());

    /// <summary>
    /// Appends a string followed by a line terminator to the builder.
    /// </summary>
    /// <param name="value">The string to append before the line terminator. If <see langword="null"/>, only the line terminator is appended.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void AppendLine(string? value)
    {
        Append(value);
        AppendLine();
    }

    /// <summary>
    /// Appends the string representation of the specified value, followed by a newline, to the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to append. Must implement <see cref="ISpanFormattable"/>.</typeparam>
    /// <param name="value">The value to append. Its string representation is formatted and appended to the current instance.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void AppendLine<T>(T value) where T : ISpanFormattable
    {
        Append(value);
        AppendLine();
    }

    /// <summary>
    /// Appends a span of characters followed by a line terminator to the builder.
    /// </summary>
    /// <param name="value">The span of characters to append before the line terminator.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void AppendLine(scoped ReadOnlySpan<char> value)
    {
        Append(value);
        AppendLine();
    }

    /// <summary>
    /// Clears the builder's content, but retains the underlying buffer for reuse.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public void Clear()
    {
        ThrowIfDisposed();

        _position = 0;
    }

    /// <summary>
    /// Gets a read-only view over the content written so far. The span is only valid until the
    /// next mutating call or <see cref="Dispose"/>.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> over the written content.</returns>
    public readonly ReadOnlySpan<char> AsSpan() => _buffer[.._position];

    /// <summary>
    /// Converts the builder's content to a string. The builder is left intact and remains usable.
    /// </summary>
    /// <returns>The string representation of the builder's content.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has been disposed.</exception>
    public override readonly string ToString()
    {
        ThrowIfDisposed();

        return _buffer[.._position].ToString();
    }

    /// <summary>
    /// Releases any pooled resources. The builder must not be used after this method is called.
    /// </summary>
    /// <remarks>
    /// Calling this method more than once is safe and subsequent calls are ignored.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_arrayFromPool != null)
        {
            ArrayPool<char>.Shared.Return(_arrayFromPool, clearArray: true);
            _arrayFromPool = null;
        }

        // Drop the buffer reference so a stale copy of this struct cannot read or write
        // memory that now belongs to another renter.
        _buffer = default;
        _position = 0;
        _disposed = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ValueStringBuilder));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(_position + 1);
        _buffer[_position++] = c;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int requiredCapacity)
    {
        // Same value as Array.MaxLength, which is not available on all target frameworks.
        const uint ArrayMaxLength = 0x7FFFFFC7;

        // Grow to at least the required size, preferring to double the buffer, without
        // letting the doubling overflow past the maximum array length.
        int newCapacity = (int)Math.Max(
            (uint)requiredCapacity,
            Math.Min((uint)_buffer.Length * 2, ArrayMaxLength));

        char[] newArray = ArrayPool<char>.Shared.Rent(newCapacity);

        _buffer[.._position].CopyTo(newArray);

        if (_arrayFromPool != null)
            ArrayPool<char>.Shared.Return(_arrayFromPool, clearArray: true);

        _buffer = newArray;
        _arrayFromPool = newArray;
    }
}
