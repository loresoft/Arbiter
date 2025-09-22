using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// Provides a high-performance builder for constructing and manipulating Uniform Resource Locators (URLs).
/// Supports a fluent API for setting URL components and appending path/query segments efficiently.
/// </summary>
/// <remarks>
/// <para>
/// <b>Thread Safety:</b> This type is not thread-safe and should not be shared between threads.
/// </para>
/// <para>
/// <b>Disposal:</b> After calling <see cref="ToString"/>, this instance is disposed and must not be used again.
/// Any further method calls will throw <see cref="ObjectDisposedException"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var builder = new UrlBuilder()
///     .Scheme("https")
///     .Host("api.example.com")
///     .Port(443)
///     .AppendPath("v1")
///     .AppendPath("users")
///     .AppendQuery("active", "true")
///     .AppendQuery("role", "admin");
///
/// string url = builder.ToString();
/// // Result: "https://api.example.com:443/v1/users?active=true&amp;role=admin"
/// </code>
/// </example>
public ref struct UrlBuilder
{
    private ReadOnlySpan<char> _scheme;
    private ReadOnlySpan<char> _host;
    private ReadOnlySpan<char> _port;
    private ReadOnlySpan<char> _username;
    private ReadOnlySpan<char> _password;
    private ReadOnlySpan<char> _fragment;

    // Path segments buffer
    private char[]? _pathSegmentsBuffer;
    private int _pathSegmentsLength;

    // Query string buffer
    private char[]? _queryStringBuffer;
    private int _queryStringLength;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlBuilder"/> struct, ready for URL construction.
    /// </summary>
    public UrlBuilder()
    {
        _pathSegmentsBuffer = null;
        _pathSegmentsLength = 0;
        _queryStringBuffer = null;
        _queryStringLength = 0;
        _disposed = false;
    }

    /// <summary>
    /// Sets the scheme (protocol) for the URL, such as "http" or "https".
    /// </summary>
    /// <param name="scheme">The scheme to use for the URL (e.g., "http", "https").</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Scheme(ReadOnlySpan<char> scheme)
    {
        _scheme = scheme;
        return this;
    }

    /// <summary>
    /// Sets the user name for user information in the URL authority component.
    /// </summary>
    /// <param name="userName">The user name to include in the URL.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder UserName(ReadOnlySpan<char> userName)
    {
        _username = userName;
        return this;
    }

    /// <summary>
    /// Sets the password for user information in the URL authority component.
    /// </summary>
    /// <param name="password">The password to include in the URL.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Password(ReadOnlySpan<char> password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Sets the host (domain or IP address) for the URL.
    /// </summary>
    /// <param name="host">The host name or IP address.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Host(ReadOnlySpan<char> host)
    {
        _host = host;
        return this;
    }

    /// <summary>
    /// Sets the port for the URL using a character span.
    /// </summary>
    /// <param name="port">The port as a character span (e.g., "443").</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Port(ReadOnlySpan<char> port)
    {
        _port = port;
        return this;
    }

    /// <summary>
    /// Sets the port for the URL using an integer value.
    /// </summary>
    /// <param name="port">The port number (e.g., 443).</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Port(int port)
    {
        _port = port.ToString(CultureInfo.InvariantCulture).AsSpan();
        return this;
    }

    /// <summary>
    /// Sets the fragment for the URL (the portion after the '#' symbol).
    /// </summary>
    /// <param name="fragment">The fragment to append to the URL (without the '#').</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Fragment(ReadOnlySpan<char> fragment)
    {
        _fragment = fragment;
        return this;
    }

    /// <summary>
    /// Appends a path segment to the URL, escaping as needed.
    /// </summary>
    /// <param name="path">The path segment to append. Must not be empty.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendPath(scoped ReadOnlySpan<char> path)
    {
        ThrowIfDisposed();

        if (path.IsEmpty)
            return this;

        // Calculate encoded length for the path segment
        int encodedLen = GetEncodedLength(path);
        bool needsSlash = _pathSegmentsLength > 0 && _pathSegmentsBuffer![_pathSegmentsLength - 1] != '/';
        int requiredExtra = encodedLen + (needsSlash ? 1 : 0);

        EnsureCapacity(ref _pathSegmentsBuffer, _pathSegmentsLength, requiredExtra);

        if (needsSlash)
            _pathSegmentsBuffer![_pathSegmentsLength++] = '/';

        // Encode the path segment in place
        UrlEncode(path, _pathSegmentsBuffer.AsSpan(_pathSegmentsLength, encodedLen));
        _pathSegmentsLength += encodedLen;

        return this;
    }

    /// <summary>
    /// Appends a path segment to the URL, converting the value to a string and escaping as needed.
    /// Optionally, a predicate can be provided to determine if the value should be appended.
    /// </summary>
    /// <typeparam name="TValue">The type of the path segment value.</typeparam>
    /// <param name="path">The path segment to append. If <see langword="null"/> or empty, nothing is appended.</param>
    /// <param name="condition">
    /// An optional predicate that determines whether the path should be appended.
    /// If <see langword="null"/>, the path is always appended.
    /// </param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendPath<TValue>(TValue? path, Func<TValue?, bool>? condition = null)
    {
        ThrowIfDisposed();

        if (path is null)
            return this;

        if (condition != null && !condition(path))
            return this;

        var str = path.ToString();
        if (string.IsNullOrEmpty(str))
            return this;

        return AppendPath(str.AsSpan());
    }

    /// <summary>
    /// Appends a path segment to the URL, escaping as needed.
    /// Optionally, a predicate can be provided to determine if the value should be appended.
    /// </summary>
    /// <param name="path">The path segment to append.</param>
    /// <param name="condition">
    /// An optional predicate that determines whether the path should be appended.
    /// If <see langword="null"/>, the path is always appended.
    /// </param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendPath(string? path, Func<string?, bool>? condition = null)
    {
        ThrowIfDisposed();

        if (path is null)
            return this;

        if (condition != null && !condition(path))
            return this;

        return AppendPath(path.AsSpan());
    }

    /// <summary>
    /// Appends a path segment to the URL if the specified boolean condition is true.
    /// </summary>
    /// <param name="path">The path segment to append.</param>
    /// <param name="condition">If <see langword="true"/>, the path is appended; otherwise, it is not.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendPath(string? path, bool condition)
    {
        ThrowIfDisposed();

        if (path is null || !condition)
            return this;

        return AppendPath(path.AsSpan());
    }

    /// <summary>
    /// Appends multiple path segments to the URL. Each segment is escaped as needed.
    /// </summary>
    /// <param name="paths">A collection of path segments to append. <see langword="null"/> or empty segments are ignored.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendPaths(params IEnumerable<string>? paths)
    {
        ThrowIfDisposed();

        if (paths is null)
            return this;

        foreach (var path in paths)
        {
            if (!string.IsNullOrEmpty(path))
                AppendPath(path.AsSpan());
        }

        return this;
    }

    /// <summary>
    /// Appends a query string parameter to the URL, escaping both name and value.
    /// </summary>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The query parameter value.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendQuery(scoped ReadOnlySpan<char> name, scoped ReadOnlySpan<char> value)
    {
        ThrowIfDisposed();

        if (name.IsEmpty)
            return this;

        int encodedNameLen = GetEncodedLength(name);
        int encodedValueLen = GetEncodedLength(value);

        int extra = encodedNameLen + 1 + encodedValueLen; // name=value
        bool needsAmp = _queryStringLength > 0;
        if (needsAmp)
            extra++; // for '&'

        EnsureCapacity(ref _queryStringBuffer, _queryStringLength, extra);

        int pos = _queryStringLength;
        if (needsAmp)
            _queryStringBuffer![pos++] = '&';

        // Encode name in place
        UrlEncode(name, _queryStringBuffer.AsSpan(pos, encodedNameLen));
        pos += encodedNameLen;

        _queryStringBuffer![pos++] = '=';

        // Encode value in place
        UrlEncode(value, _queryStringBuffer.AsSpan(pos, encodedValueLen));
        pos += encodedValueLen;

        _queryStringLength = pos;
        return this;
    }

    /// <summary>
    /// Appends a query string parameter to the URL, converting the value to a string and escaping as needed.
    /// Optionally, a predicate can be provided to determine if the value should be appended.
    /// </summary>
    /// <typeparam name="TValue">The type of the query value.</typeparam>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The query parameter value. If <see langword="null"/>, nothing is appended.</param>
    /// <param name="condition">Optional predicate to determine if the value should be appended.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendQuery<TValue>(string name, TValue? value, Func<TValue?, bool>? condition = null)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(name) || value is null)
            return this;

        if (condition != null && !condition(value))
            return this;

        return AppendQuery(name.AsSpan(), value.ToString().AsSpan());
    }

    /// <summary>
    /// Appends a query string parameter to the URL if the provided condition is met.
    /// </summary>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The query parameter value.</param>
    /// <param name="condition">
    /// An optional predicate that determines whether the query should be appended.
    /// If <see langword="null"/>, the query is always appended.
    /// </param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendQuery(string name, string? value, Func<string?, bool>? condition = null)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(name) || (condition != null && !condition(value)))
            return this;

        return AppendQuery(name.AsSpan(), value.AsSpan());
    }

    /// <summary>
    /// Appends a query string parameter to the URL if the specified boolean condition is true.
    /// </summary>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The query parameter value.</param>
    /// <param name="condition">If <see langword="true"/>, the query is appended; otherwise, it is not.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendQuery(string name, string? value, bool condition)
    {
        ThrowIfDisposed();

        if (string.IsNullOrEmpty(name) || !condition || value is null)
            return this;

        return AppendQuery(name.AsSpan(), value.AsSpan());
    }

    /// <summary>
    /// Appends multiple query string parameters to the URL. Each key-value pair is escaped as needed.
    /// </summary>
    /// <param name="queryParams">A collection of query string key-value pairs. <see langword="null"/> or empty keys/values are ignored.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    public UrlBuilder AppendQueries(params IEnumerable<KeyValuePair<string, string?>>? queryParams)
    {
        ThrowIfDisposed();

        if (queryParams is null)
            return this;

        foreach (var kvp in queryParams)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
                AppendQuery(kvp.Key.AsSpan(), kvp.Value.AsSpan());
        }

        return this;
    }

    /// <summary>
    /// Returns the fully constructed URL as a string, including all components and segments.
    /// After calling this method, the builder is disposed and must not be used again.
    /// </summary>
    /// <returns>The complete URL as a string.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Simple string building logic")]
    public override string ToString()
    {
        ThrowIfDisposed();

        // a reasonable initial capacity
        var sb = new ValueStringBuilder(128);

        // scheme://
        if (!_scheme.IsEmpty)
        {
            sb.Append(_scheme);
            sb.Append("://");
        }

        // [username[:password]@]
        if (!_username.IsEmpty)
        {
            sb.Append(_username);
            if (!_password.IsEmpty)
            {
                sb.Append(':');
                sb.Append(_password);
            }
            sb.Append('@');
        }

        // host
        if (!_host.IsEmpty)
            sb.Append(_host);

        // :port
        if (!_port.IsEmpty)
        {
            sb.Append(':');
            sb.Append(_port);
        }

        // /path
        if (_pathSegmentsBuffer != null && _pathSegmentsLength > 0)
        {
            // Ensure path starts with '/'
            if (_pathSegmentsBuffer[0] != '/')
                sb.Append('/');

            sb.Append(_pathSegmentsBuffer.AsSpan(0, _pathSegmentsLength));
        }

        // ?query
        if (_queryStringBuffer != null && _queryStringLength > 0)
        {
            sb.Append('?');
            sb.Append(_queryStringBuffer.AsSpan(0, _queryStringLength));
        }

        // #fragment
        if (!_fragment.IsEmpty)
        {
            sb.Append('#');
            sb.Append(_fragment);
        }

        var url = sb.ToString();

        Dispose(); // Clean up buffers

        return url;
    }

    /// <summary>
    /// Releases all resources used by this <see cref="UrlBuilder"/> instance.
    /// After disposal, further use of this instance will throw an <see cref="ObjectDisposedException"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_disposed)
            return;

        if (_pathSegmentsBuffer != null)
        {
            ArrayPool<char>.Shared.Return(_pathSegmentsBuffer);
            _pathSegmentsBuffer = null;
            _pathSegmentsLength = 0;
        }

        if (_queryStringBuffer != null)
        {
            ArrayPool<char>.Shared.Return(_queryStringBuffer);
            _queryStringBuffer = null;
            _queryStringLength = 0;
        }

        _disposed = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UrlBuilder));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void EnsureCapacity(ref char[]? buffer, int currentLength, int requiredExtra)
    {
        int requiredLength = currentLength + requiredExtra;

        if (buffer == null)
        {
            int initialSize = Math.Max(128, requiredLength);
            buffer = ArrayPool<char>.Shared.Rent(initialSize);
        }
        else if (buffer.Length < requiredLength)
        {
            int newSize = Math.Max(buffer.Length * 2, requiredLength);
            var newBuffer = ArrayPool<char>.Shared.Rent(newSize);
            buffer.AsSpan(0, currentLength).CopyTo(newBuffer);

            ArrayPool<char>.Shared.Return(buffer);
            buffer = newBuffer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int UrlEncode(ReadOnlySpan<char> input, Span<char> output)
    {
        int written = 0;
        Span<byte> utf8Buffer = stackalloc byte[4];

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (IsUnreserved(c))
            {
                output[written++] = c;
            }
            else if (c <= 0x7F)
            {
                // ASCII, percent-encode as single byte
                output[written++] = '%';
                output[written++] = GetHex((c >> 4) & 0xF);
                output[written++] = GetHex(c & 0xF);
            }
            else
            {
                // Non-ASCII: encode as UTF-8 bytes and percent-encode each byte
                int byteCount = System.Text.Encoding.UTF8.GetBytes(input.Slice(i, 1), utf8Buffer);
                for (int b = 0; b < byteCount; b++)
                {
                    output[written++] = '%';
                    output[written++] = GetHex((utf8Buffer[b] >> 4) & 0xF);
                    output[written++] = GetHex(utf8Buffer[b] & 0xF);
                }
            }
        }
        return written;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetEncodedLength(ReadOnlySpan<char> input)
    {
        int len = 0;
        Span<byte> utf8Buffer = stackalloc byte[4];

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (IsUnreserved(c))
            {
                len += 1;
            }
            else if (c <= 0x7F)
            {
                len += 3; // %XX
            }
            else
            {
                // Non-ASCII: count UTF-8 bytes and add 3 per byte
                int byteCount = System.Text.Encoding.UTF8.GetBytes(input.Slice(i, 1), utf8Buffer);
                len += 3 * byteCount;
            }
        }
        return len;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsUnreserved(char c)
    {
        // RFC 3986 unreserved: ALPHA / DIGIT / "-" / "." / "_" / "~"
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c >= '0' && c <= '9') ||
               c == '-' || c == '_' || c == '.' || c == '~';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char GetHex(int value)
    {
        return (char)(value < 10 ? ('0' + value) : ('A' + (value - 10)));
    }
}
