using System.Buffers;
using System.Runtime.CompilerServices;

namespace Arbiter.Services;

/// <summary>
/// A high-performance, low-allocation mutable string builder using a Span-based buffer.
/// Uses stack or pooled memory depending on constructor. Designed for scenarios where minimizing allocations is critical.
/// </summary>
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
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueStringBuilder Append(char value)
    {
        ThrowIfDisposed();

        if (_position < _buffer.Length)
            _buffer[_position++] = value;
        else
            GrowAndAppend(value);

        return this;
    }

    /// <summary>
    /// Appends the specified string to the end of the builder.
    /// </summary>
    /// <param name="value">The string to append. If <see langword="null"/>, no action is taken.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueStringBuilder Append(string? value)
    {
        ThrowIfDisposed();

        if (value == null)
            return this;

        int required = _position + value.Length;
        if (required > _buffer.Length)
            Grow(required);

        value.AsSpan().CopyTo(_buffer[_position..]);
        _position += value.Length;

        return this;
    }

    /// <summary>
    /// Appends a span of characters to the end of the builder.
    /// </summary>
    /// <param name="value">The span of characters to append.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    public ValueStringBuilder Append(scoped ReadOnlySpan<char> value)
    {
        ThrowIfDisposed();

        int required = _position + value.Length;
        if (required > _buffer.Length)
            Grow(required);

        value.CopyTo(_buffer[_position..]);
        _position += value.Length;

        return this;
    }

    /// <summary>
    /// Appends a character to the builder multiple times.
    /// </summary>
    /// <param name="value">The character to append.</param>
    /// <param name="count">The number of times to append the character.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    public ValueStringBuilder Append(char value, int count)
    {
        ThrowIfDisposed();

        if (count <= 0)
            return this;

        int required = _position + count;
        if (required > _buffer.Length) Grow(required);

        for (int i = 0; i < count; i++)
            _buffer[_position++] = value;

        return this;
    }

    /// <summary>
    /// Appends the string representation of the specified value to the current instance.
    /// </summary>
    /// <remarks>
    /// This method attempts to format the value directly into the internal buffer. If the buffer
    /// does not have  enough space, it is resized, and the formatting is retried. The method ensures that the appended
    /// value  is properly added to the builder, growing the buffer as needed.
    /// </remarks>
    /// <typeparam name="T">The type of the value to append. Must implement <see cref="ISpanFormattable"/>.</typeparam>
    /// <param name="value">The value to append. The value is formatted using its <see cref="ISpanFormattable.TryFormat"/> implementation.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance with the appended value.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the value of type <typeparamref name="T"/> cannot be formatted into the buffer.</exception>
    public ValueStringBuilder Append<T>(T value) where T : ISpanFormattable
    {
        ThrowIfDisposed();

        // Try to format directly into the buffer
        int charsAvailable = _buffer.Length - _position;
        if (value.TryFormat(_buffer[_position..], out int charsWritten, format: null, provider: null))
        {
            _position += charsWritten;
            return this;
        }

        // If not enough space, grow and try again
        // Estimate: most types won't exceed 128 chars, but we double buffer for safety
        Grow(_position + 128);
        charsAvailable = _buffer.Length - _position;
        if (!value.TryFormat(_buffer.Slice(_position), out charsWritten, format: null, provider: null))
            throw new InvalidOperationException($"Failed to format value of type {typeof(T)}.");

        _position += charsWritten;
        return this;
    }

    /// <summary>
    /// Appends the platform-specific line terminator to the builder.
    /// </summary>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    public ValueStringBuilder AppendLine()
        => Append(Environment.NewLine.AsSpan());

    /// <summary>
    /// Appends a string followed by a line terminator to the builder.
    /// </summary>
    /// <param name="value">The string to append before the line terminator.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    public ValueStringBuilder AppendLine(string? value)
        => Append(value).AppendLine();

    /// <summary>
    /// Appends the string representation of the specified value, followed by a newline, to the current instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to append. Must implement <see cref="ISpanFormattable"/>.</typeparam>
    /// <param name="value">The value to append. Its string representation is formatted and appended to the current instance.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance with the appended value and a newline.</returns>
    public ValueStringBuilder AppendLine<T>(T value) where T : ISpanFormattable
        => Append(value).AppendLine();

    /// <summary>
    /// Appends a span of characters followed by a line terminator to the builder.
    /// </summary>
    /// <param name="value">The span of characters to append before the line terminator.</param>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    public ValueStringBuilder AppendLine(scoped ReadOnlySpan<char> value)
        => Append(value).AppendLine();

    /// <summary>
    /// Clears the builder's content, but retains the underlying buffer for reuse.
    /// </summary>
    /// <returns>The current <see cref="ValueStringBuilder"/> instance.</returns>
    public ValueStringBuilder Clear()
    {
        _position = 0;
        return this;
    }

    /// <summary>
    /// Converts the builder's content to a string and disposes the builder.
    /// The builder must not be used after this method is called.
    /// </summary>
    /// <returns>The string representation of the builder's content.</returns>
    public override string ToString()
    {
        ThrowIfDisposed();

        string result = _buffer[.._position].ToString();

        Dispose();

        return result;
    }

    /// <summary>
    /// Releases any pooled resources. The builder must not be used after this method is called.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_arrayFromPool != null)
        {
            ArrayPool<char>.Shared.Return(_arrayFromPool);
            _arrayFromPool = null;
        }

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
        int newCapacity = Math.Max(requiredCapacity, _buffer.Length * 2);
        char[] newArray = ArrayPool<char>.Shared.Rent(newCapacity);

        _buffer[.._position].CopyTo(newArray);

        if (_arrayFromPool != null)
            ArrayPool<char>.Shared.Return(_arrayFromPool);

        _buffer = newArray;
        _arrayFromPool = newArray;
    }
}
