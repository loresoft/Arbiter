using System.Buffers;
using System.Runtime.CompilerServices;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// A high-performance, low-allocation mutable string builder using a Span-based buffer.
/// Uses stack or pooled memory depending on constructor.
/// </summary>
public ref struct ValueStringBuilder
{
    private Span<char> _buffer;          // Current character buffer
    private char[]? _arrayFromPool;      // Array rented from pool
    private int _position;               // Current write position
    private bool _disposed;              // Flag indicating if the builder has been disposed

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct with a user-supplied stack-allocated buffer.
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
    public ValueStringBuilder(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

        _arrayFromPool = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = _arrayFromPool;
        _position = 0;
        _disposed = false;
    }

    /// <summary>
    /// Gets the length of the current written content.
    /// </summary>
    public int Length
    {
        get
        {
            ThrowIfDisposed();
            return _position;
        }
    }

    /// <summary>
    /// Appends a single character to the builder.
    /// </summary>
    /// <param name="value">The character to append.</param>
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
    /// Appends a string to the builder.
    /// </summary>
    /// <param name="value">The string to append. If <see langword="null"/>, no action is taken.</param>
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
    /// Appends a span of characters to the builder.
    /// </summary>
    /// <param name="value">The span of characters to append.</param>
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
    /// <param name="count">The number of times to append the character.</param>
    public void Append(char value, int count)
    {
        ThrowIfDisposed();

        if (count <= 0) return;

        int required = _position + count;
        if (required > _buffer.Length) Grow(required);

        for (int i = 0; i < count; i++)
            _buffer[_position++] = value;
    }

    /// <summary>
    /// Appends the platform-specific line terminator to the builder.
    /// </summary>
    public void AppendLine()
    {
        Append(Environment.NewLine.AsSpan());
    }

    /// <summary>
    /// Appends a string followed by a line terminator to the builder.
    /// </summary>
    /// <param name="value">The string to append before the line terminator.</param>
    public void AppendLine(string? value)
    {
        Append(value);
        AppendLine();
    }

    /// <summary>
    /// Appends a span of characters followed by a line terminator to the builder.
    /// </summary>
    /// <param name="value">The span of characters to append before the line terminator.</param>
    public void AppendLine(scoped ReadOnlySpan<char> value)
    {
        Append(value);
        AppendLine();
    }

    /// <summary>
    /// Clears the builder's content, but retains the underlying buffer for reuse.
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        _position = 0;
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

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the builder has already been disposed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ValueStringBuilder));
    }

    /// <summary>
    /// Ensures enough capacity in the buffer and appends a character.
    /// </summary>
    /// <param name="c">The character to append.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(_position + 1);
        _buffer[_position++] = c;
    }

    /// <summary>
    /// Grows the buffer to fit the required capacity.
    /// </summary>
    /// <param name="requiredCapacity">The minimum required capacity for the buffer.</param>
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
