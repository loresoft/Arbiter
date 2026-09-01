using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Arbiter.Services;

/// <summary>
/// Provides a low-allocation builder for constructing and manipulating Uniform Resource Locators (URLs).
/// Supports a fluent API for setting URL components and appending path/query segments efficiently.
/// </summary>
/// <remarks>
/// <para>
/// <b>Thread Safety:</b> This type is not thread-safe and should not be shared between threads.
/// </para>
/// <para>
/// <b>Lifetime:</b> This type holds no unmanaged or pooled resources and requires no cleanup.
/// <see cref="ToString"/> does not mutate the builder, so a builder can be built more than once and
/// extended between calls.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var url = new UrlBuilder()
///     .Scheme("https")
///     .Host("api.example.com")
///     .Port(443)
///     .AppendSegment("v1")
///     .AppendSegment("users")
///     .AppendQuery("active", "true")
///     .AppendQuery("role", "admin")
///     .ToString();
///
/// // Result: "https://api.example.com:443/v1/users?active=true&amp;role=admin"
/// </code>
/// </example>
public sealed class UrlBuilder
{
    // typical path and query segments are short; the buffers grow by doubling when needed
    private const int DefaultSegmentCapacity = 32;
    private const int DefaultBuildCapacity = 128;

    private string? _scheme;
    private string? _host;
    private string? _portText;
    private int _portNumber = -1;
    private string? _username;
    private string? _password;
    private string? _fragment;

    // Path segments buffer, percent-encoded, without leading or trailing separator
    private char[]? _pathBuffer;
    private int _pathLength;

    // Query string buffer, percent-encoded, without the leading '?'
    private char[]? _queryBuffer;
    private int _queryLength;

    #region Create Methods

    /// <summary>
    /// Creates a new <see cref="UrlBuilder"/> starting with the specified path, where '/' acts as a
    /// segment separator, for example <c>api/user</c>.
    /// </summary>
    /// <remarks>
    /// The value is appended without escaping, so the caller is responsible for percent-encoding it.
    /// Use <see cref="FromSegment(string?)"/> for a single segment that may contain characters
    /// requiring escaping.
    /// </remarks>
    /// <param name="path">The path to start the URL with. If empty, nothing is appended.</param>
    /// <returns>A new <see cref="UrlBuilder"/> instance for chaining.</returns>
    public static UrlBuilder FromPath(scoped ReadOnlySpan<char> path)
        => new UrlBuilder().AppendPath(path);

    /// <summary>
    /// Creates a new <see cref="UrlBuilder"/> starting with the specified path, where '/' acts as a
    /// segment separator, for example <c>api/user</c>.
    /// </summary>
    /// <remarks>
    /// The value is appended without escaping, so the caller is responsible for percent-encoding it.
    /// Use <see cref="FromSegment(string?)"/> for a single segment that may contain characters
    /// requiring escaping.
    /// </remarks>
    /// <param name="path">The path to start the URL with. If <see langword="null"/> or empty, nothing is appended.</param>
    /// <returns>A new <see cref="UrlBuilder"/> instance for chaining.</returns>
    public static UrlBuilder FromPath(string? path)
        => new UrlBuilder().AppendPath(path);

    /// <summary>
    /// Creates a new <see cref="UrlBuilder"/> starting with a single path segment, escaping as needed.
    /// </summary>
    /// <param name="segment">The path segment to start the URL with. If <see langword="null"/> or empty, nothing is appended.</param>
    /// <returns>A new <see cref="UrlBuilder"/> instance for chaining.</returns>
    public static UrlBuilder FromSegment(string? segment)
        => new UrlBuilder().AppendSegment(segment);

    /// <summary>
    /// Creates a new <see cref="UrlBuilder"/> starting with the specified path segments, joined by '/'.
    /// Each segment is escaped as needed.
    /// </summary>
    /// <param name="segments">The path segments to start the URL with. <see langword="null"/> or empty segments are ignored.</param>
    /// <returns>A new <see cref="UrlBuilder"/> instance for chaining.</returns>
    public static UrlBuilder FromSegments(params IEnumerable<string>? segments)
        => new UrlBuilder().AppendSegments(segments);

    #endregion

    /// <summary>
    /// Sets the scheme (protocol) for the URL, such as "http" or "https".
    /// </summary>
    /// <param name="scheme">The scheme to use for the URL (e.g., "http", "https").</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Scheme(string? scheme)
    {
        _scheme = scheme;
        return this;
    }

    /// <summary>
    /// Sets the user name for user information in the URL authority component.
    /// </summary>
    /// <param name="userName">The user name to include in the URL.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder UserName(string? userName)
    {
        _username = userName;
        return this;
    }

    /// <summary>
    /// Sets the password for user information in the URL authority component.
    /// </summary>
    /// <param name="password">The password to include in the URL.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Password(string? password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Sets the host (domain or IP address) for the URL.
    /// </summary>
    /// <param name="host">The host name or IP address.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Host(string? host)
    {
        _host = host;
        return this;
    }

    /// <summary>
    /// Sets the port for the URL using its textual representation.
    /// </summary>
    /// <param name="port">The port as text (e.g., "443").</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Port(string? port)
    {
        _portText = port;
        _portNumber = -1;
        return this;
    }

    /// <summary>
    /// Sets the port for the URL using an integer value.
    /// </summary>
    /// <param name="port">The port number (e.g., 443).</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="port"/> is negative.</exception>
    public UrlBuilder Port(int port)
    {
        if (port < 0)
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must not be negative.");

        _portNumber = port;
        _portText = null;
        return this;
    }

    /// <summary>
    /// Sets the fragment for the URL (the portion after the '#' symbol).
    /// </summary>
    /// <param name="fragment">The fragment to append to the URL (without the '#').</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder Fragment(string? fragment)
    {
        _fragment = fragment;
        return this;
    }

    /// <summary>
    /// Appends a single path segment to the URL, escaping as needed. Any '/' in the value is
    /// escaped as <c>%2F</c> and does not act as a separator.
    /// </summary>
    /// <param name="segment">The path segment to append. If empty, nothing is appended.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendSegment(scoped ReadOnlySpan<char> segment)
    {
        if (segment.IsEmpty)
            return this;

        // Separators are added between segments; the encoded segment can never contain '/'
        if (_pathLength > 0)
            AppendChar(ref _pathBuffer, ref _pathLength, '/');

        AppendEscaped(ref _pathBuffer, ref _pathLength, segment);

        return this;
    }

    /// <summary>
    /// Appends a single path segment to the URL, converting the value to a string and escaping as needed.
    /// Optionally, a predicate can be provided to determine if the value should be appended.
    /// </summary>
    /// <typeparam name="TValue">The type of the path segment value.</typeparam>
    /// <param name="segment">The path segment to append. If <see langword="null"/> or empty, nothing is appended.</param>
    /// <param name="condition">
    /// An optional predicate that determines whether the segment should be appended.
    /// If <see langword="null"/>, the segment is always appended.
    /// </param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendSegment<TValue>(TValue? segment, Func<TValue?, bool>? condition = null)
    {
        if (segment is null)
            return this;

        if (condition != null && !condition(segment))
            return this;

        var text = ToStringInvariant(segment);
        if (string.IsNullOrEmpty(text))
            return this;

        return AppendSegment(text.AsSpan());
    }

    /// <summary>
    /// Appends a single path segment to the URL, escaping as needed.
    /// Optionally, a predicate can be provided to determine if the value should be appended.
    /// </summary>
    /// <param name="segment">The path segment to append.</param>
    /// <param name="condition">
    /// An optional predicate that determines whether the segment should be appended.
    /// If <see langword="null"/>, the segment is always appended.
    /// </param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendSegment(string? segment, Func<string?, bool>? condition = null)
    {
        if (segment is null)
            return this;

        if (condition != null && !condition(segment))
            return this;

        return AppendSegment(segment.AsSpan());
    }

    /// <summary>
    /// Appends a single path segment to the URL if the specified boolean condition is true.
    /// </summary>
    /// <param name="segment">The path segment to append.</param>
    /// <param name="condition">If <see langword="true"/>, the segment is appended; otherwise, it is not.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendSegment(string? segment, bool condition)
    {
        if (segment is null || !condition)
            return this;

        return AppendSegment(segment.AsSpan());
    }

    /// <summary>
    /// Appends multiple path segments to the URL, joined by '/'. Each segment is escaped as needed.
    /// </summary>
    /// <param name="segments">A collection of path segments to append. <see langword="null"/> or empty segments are ignored.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendSegments(params IEnumerable<string>? segments)
    {
        if (segments is null)
            return this;

        foreach (var segment in segments)
        {
            if (!string.IsNullOrEmpty(segment))
                AppendSegment(segment.AsSpan());
        }

        return this;
    }

    /// <summary>
    /// Appends a path to the URL, where '/' acts as a segment separator, for example <c>api/user</c>.
    /// Leading and trailing '/' characters are trimmed; interior separators are preserved as-is.
    /// </summary>
    /// <remarks>
    /// The value is appended without escaping, so the caller is responsible for percent-encoding it.
    /// Use <see cref="AppendSegment(ReadOnlySpan{char})"/> for a single segment that may contain
    /// characters requiring escaping.
    /// </remarks>
    /// <param name="path">The path to append. If empty, nothing is appended.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendPath(scoped ReadOnlySpan<char> path)
    {
        path = path.Trim('/');
        if (path.IsEmpty)
            return this;

        if (_pathLength > 0)
            AppendChar(ref _pathBuffer, ref _pathLength, '/');

        EnsureCapacity(ref _pathBuffer, _pathLength, path.Length);

        path.CopyTo(_pathBuffer.AsSpan(_pathLength));
        _pathLength += path.Length;

        return this;
    }

    /// <summary>
    /// Appends a path to the URL, where '/' acts as a segment separator, for example <c>api/user</c>.
    /// Leading and trailing '/' characters are trimmed; interior separators are preserved as-is.
    /// </summary>
    /// <remarks>
    /// The value is appended without escaping, so the caller is responsible for percent-encoding it.
    /// Use <see cref="AppendSegment(string?, Func{string?, bool}?)"/> for a single segment that may
    /// contain characters requiring escaping.
    /// </remarks>
    /// <param name="path">The path to append. If <see langword="null"/> or empty, nothing is appended.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return this;

        return AppendPath(path.AsSpan());
    }

    /// <summary>
    /// Appends a query string parameter to the URL, escaping both name and value.
    /// </summary>
    /// <param name="name">The query parameter name. If empty, nothing is appended.</param>
    /// <param name="value">The query parameter value.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendQuery(scoped ReadOnlySpan<char> name, scoped ReadOnlySpan<char> value)
    {
        if (name.IsEmpty)
            return this;

        if (_queryLength > 0)
            AppendChar(ref _queryBuffer, ref _queryLength, '&');

        AppendEscaped(ref _queryBuffer, ref _queryLength, name);
        AppendChar(ref _queryBuffer, ref _queryLength, '=');
        AppendEscaped(ref _queryBuffer, ref _queryLength, value);

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
    public UrlBuilder AppendQuery<TValue>(string name, TValue? value, Func<TValue?, bool>? condition = null)
    {
        if (string.IsNullOrEmpty(name) || value is null)
            return this;

        if (condition != null && !condition(value))
            return this;

        return AppendQuery(name.AsSpan(), ToStringInvariant(value).AsSpan());
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
    public UrlBuilder AppendQuery(string name, string? value, Func<string?, bool>? condition = null)
    {
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
    public UrlBuilder AppendQuery(string name, string? value, bool condition)
    {
        if (string.IsNullOrEmpty(name) || !condition)
            return this;

        return AppendQuery(name.AsSpan(), value.AsSpan());
    }

    /// <summary>
    /// Appends multiple query string parameters to the URL. Each key-value pair is escaped as needed.
    /// </summary>
    /// <param name="queryParams">A collection of query string key-value pairs. <see langword="null"/> or empty keys/values are ignored.</param>
    /// <returns>This <see cref="UrlBuilder"/> instance for chaining.</returns>
    public UrlBuilder AppendQueries(params IEnumerable<KeyValuePair<string, string?>>? queryParams)
    {
        if (queryParams is null)
            return this;

        foreach (var pair in queryParams)
        {
            if (!string.IsNullOrEmpty(pair.Key) && pair.Value != null)
                AppendQuery(pair.Key.AsSpan(), pair.Value.AsSpan());
        }

        return this;
    }

    /// <summary>
    /// Returns the fully constructed URL as a string.
    /// </summary>
    /// <returns>The complete URL as a string.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Simple string building logic")]
    public override string ToString()
    {
        using var builder = new ValueStringBuilder(DefaultBuildCapacity);

        // scheme://
        if (!string.IsNullOrEmpty(_scheme))
        {
            builder.Append(_scheme);
            builder.Append("://");
        }

        // [username[:password]@]
        if (!string.IsNullOrEmpty(_username))
        {
            builder.Append(_username);
            if (!string.IsNullOrEmpty(_password))
            {
                builder.Append(':');
                builder.Append(_password);
            }
            builder.Append('@');
        }

        // host
        if (!string.IsNullOrEmpty(_host))
        {
            var isUnbracketedIpv6 = _host[0] != '[' && Uri.CheckHostName(_host) == UriHostNameType.IPv6;
            if (isUnbracketedIpv6)
                builder.Append('[');

            builder.Append(_host);

            if (isUnbracketedIpv6)
                builder.Append(']');
        }

        // :port
        if (!string.IsNullOrEmpty(_portText))
        {
            builder.Append(':');
            builder.Append(_portText);
        }
        else if (_portNumber >= 0)
        {
            builder.Append(':');
            builder.Append(_portNumber);
        }

        // /path
        if (_pathLength > 0)
        {
            builder.Append('/');
            builder.Append(_pathBuffer.AsSpan(0, _pathLength));
        }

        // ?query
        if (_queryLength > 0)
        {
            builder.Append('?');
            builder.Append(_queryBuffer.AsSpan(0, _queryLength));
        }

        // #fragment
        if (!string.IsNullOrEmpty(_fragment))
        {
            builder.Append('#');
            builder.Append(_fragment);
        }

        return builder.ToString();
    }

    private static string? ToStringInvariant<TValue>(TValue value)
        => value is IFormattable formattable
            ? formattable.ToString(format: null, CultureInfo.InvariantCulture)
            : value?.ToString();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void EnsureCapacity([NotNull] ref char[]? buffer, int currentLength, int requiredExtra)
    {
        var requiredLength = currentLength + requiredExtra;

        if (buffer == null)
        {
            buffer = new char[Math.Max(DefaultSegmentCapacity, requiredLength)];
        }
        else if (buffer.Length < requiredLength)
        {
            var newBuffer = new char[Math.Max(buffer.Length * 2, requiredLength)];
            buffer.AsSpan(0, currentLength).CopyTo(newBuffer);

            buffer = newBuffer;
        }
    }

    private static void AppendChar([NotNull] ref char[]? buffer, ref int length, char value)
    {
        EnsureCapacity(ref buffer, length, 1);
        buffer[length++] = value;
    }

    private static void AppendEscaped([NotNull] ref char[]? buffer, ref int length, scoped ReadOnlySpan<char> value)
    {
        // Worst case is a 3 byte UTF-8 scalar per char, each byte escaped as "%XX".
        var maximum = value.Length * 9;
        var required = value.Length;

        // Loop until we can successfully escape the string into the buffer, growing as needed.
        while (true)
        {
            EnsureCapacity(ref buffer, length, required);

            // Try to escape the string into the buffer. If it fails, increase the required size and try again.
            if (TryEscapeDataString(value, buffer.AsSpan(length), out var charsWritten))
            {
                length += charsWritten;
                return;
            }

            required = Math.Min(required * 3, maximum);
        }
    }

#if NET9_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryEscapeDataString(scoped ReadOnlySpan<char> input, Span<char> destination, out int charsWritten)
        => Uri.TryEscapeDataString(input, destination, out charsWritten);
#else
    /// <summary>
    /// Polyfill for <c>Uri.TryEscapeDataString</c>, which is only available on .NET 9 or greater.
    /// </summary>
    private static bool TryEscapeDataString(scoped ReadOnlySpan<char> input, Span<char> destination, out int charsWritten)
    {
        var written = 0;
        Span<byte> utf8Buffer = stackalloc byte[4];

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (IsUnreserved(c))
            {
                if (written + 1 > destination.Length)
                {
                    charsWritten = 0;
                    return false;
                }

                destination[written++] = c;
            }
            else if (c <= 0x7F)
            {
                // ASCII, percent-encode as single byte
                if (written + 3 > destination.Length)
                {
                    charsWritten = 0;
                    return false;
                }

                destination[written++] = '%';
                destination[written++] = GetHex((c >> 4) & 0xF);
                destination[written++] = GetHex(c & 0xF);
            }
            else
            {
                // Non-ASCII: encode the whole scalar value, a surrogate pair must not be split
                var charCount = GetScalarLength(input, i);
                var byteCount = Encoding.UTF8.GetBytes(input.Slice(i, charCount), utf8Buffer);

                if (written + (3 * byteCount) > destination.Length)
                {
                    charsWritten = 0;
                    return false;
                }

                for (var b = 0; b < byteCount; b++)
                {
                    destination[written++] = '%';
                    destination[written++] = GetHex((utf8Buffer[b] >> 4) & 0xF);
                    destination[written++] = GetHex(utf8Buffer[b] & 0xF);
                }

                i += charCount - 1;
            }
        }

        charsWritten = written;
        return true;
    }

    /// <summary>
    /// Gets the number of chars making up the scalar value at <paramref name="index"/>, either a
    /// surrogate pair or a single char. Unpaired surrogates are treated as a single char and encode
    /// as the Unicode replacement character, matching <see cref="Encoding.UTF8"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetScalarLength(scoped ReadOnlySpan<char> input, int index)
    {
        return char.IsHighSurrogate(input[index])
            && index + 1 < input.Length
            && char.IsLowSurrogate(input[index + 1])
                ? 2
                : 1;
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
#endif
}
