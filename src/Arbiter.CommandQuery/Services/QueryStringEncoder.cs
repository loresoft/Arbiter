using System.Buffers;
using System.Buffers.Text;
using System.IO.Compression;
using System.Text.Json;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// A service to encode and decode values for query strings.
/// </summary>
public static class QueryStringEncoder
{
    /// <summary>
    /// Encodes a value to a query string format.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The value to encode</param>
    /// <param name="options">The JSON options to use for serialization</param>
    /// <returns>A query string encode value</returns>
    public static string? Encode<T>(T value, JsonSerializerOptions? options = null)
    {
        if (value is null)
            return null;

        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        using var outputStream = new MemoryStream();
        using var compressionStream = new GZipStream(outputStream, CompressionMode.Compress);
        using var jsonWriter = new Utf8JsonWriter(compressionStream);

        JsonSerializer.Serialize(jsonWriter, value, options);

        var jsonBytes = outputStream.ToArray();

        return Base64UrlEncode(jsonBytes);
    }

    /// <summary>
    /// Decodes a query string value to its original format.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="encodedQueryString">The encoded query string format</param>
    /// <param name="options">The JSON options to use for serialization</param>
    /// <returns>The instance decoded from the query string format</returns>
    public static T? Decode<T>(string? encodedQueryString, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(encodedQueryString))
            return default;

        var jsonBytes = Base64UrlDecode(encodedQueryString);

        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        using var inputStream = new MemoryStream(jsonBytes);
        using var compressionStream = new GZipStream(inputStream, CompressionMode.Decompress);

        return JsonSerializer.Deserialize<T>(compressionStream, options);
    }


    /// <summary>
    /// Encodes the specified binary data into a Base64 URL-safe string.
    /// </summary>
    /// <remarks>
    /// The resulting string is URL-safe, meaning it replaces characters that are not valid in URLs
    /// (such as '+' and '/') with '-' and '_', and removes padding characters ('=') typically found in standard Base64
    /// encoding. This method is optimized for performance and uses stack allocation for small inputs.
    /// </remarks>
    /// <param name="input">The binary data to encode as a <see cref="ReadOnlySpan{T}"/> of bytes.</param>
    /// <returns>
    /// A Base64 URL-safe string representation of the input data. Returns an empty string if <paramref name="input"/>is empty.
    /// </returns>
    public static string Base64UrlEncode(ReadOnlySpan<byte> input)
    {
#if NET9_0_OR_GREATER
        return Base64Url.EncodeToString(input);
#else
        const int StackAllocThreshold = 128;

        if (input.IsEmpty)
            return string.Empty;

        int bufferSize = GetArraySizeRequiredToEncode(input.Length);

        char[]? bufferToReturnToPool = null;

        Span<char> buffer = bufferSize <= StackAllocThreshold
            ? stackalloc char[StackAllocThreshold]
            : bufferToReturnToPool = ArrayPool<char>.Shared.Rent(bufferSize);

        var numBase64Chars = Base64UrlEncode(input, buffer);
        var base64Url = new string(buffer.Slice(0, numBase64Chars));

        if (bufferToReturnToPool != null)
            ArrayPool<char>.Shared.Return(bufferToReturnToPool);

        return base64Url;
#endif
    }

    /// <summary>
    /// Decodes a Base64 URL-encoded string into its original byte array representation.
    /// </summary>
    /// <remarks>
    /// This method automatically handles padding and replaces URL-safe Base64 characters ('-' and '_')
    /// with their standard Base64 equivalents ('+' and '/'). Ensure the input string is properly formatted  as
    /// Base64 URL-encoded before calling this method.
    /// </remarks>
    /// <param name="input">The Base64 URL-encoded string to decode. This string must use the URL-safe Base64 encoding scheme, where '-'
    /// replaces '+' and '_' replaces '/'.</param>
    /// <returns>A byte array containing the decoded data. Returns an empty array if <paramref name="input"/> is an empty string.</returns>
    public static byte[] Base64UrlDecode(string input)
    {
#if NET9_0_OR_GREATER
        return Base64Url.DecodeFromChars(input);
#else
        ArgumentNullException.ThrowIfNull(input);

        var count = input.Length;
        if (count == 0)
            return [];

        var offset = 0;
        var buffer = new char[GetArraySizeRequiredToDecode(count)];
        var bufferOffset = 0;

        var paddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);
        var arraySizeRequired = checked(count + paddingCharsToAdd);

        // Copy input into buffer, fixing up '-' -> '+' and '_' -> '/'.
        var i = bufferOffset;
        for (var j = offset; i - bufferOffset < count; i++, j++)
        {
            var ch = input[j];
            if (ch == '-')
                buffer[i] = '+';
            else if (ch == '_')
                buffer[i] = '/';
            else
                buffer[i] = ch;
        }

        // Add the padding characters back.
        for (; paddingCharsToAdd > 0; i++, paddingCharsToAdd--)
            buffer[i] = '=';

        return Convert.FromBase64CharArray(buffer, bufferOffset, arraySizeRequired);
#endif
    }

#if !NET9_0_OR_GREATER
    private static int Base64UrlEncode(ReadOnlySpan<byte> input, Span<char> output)
    {
        if (input.IsEmpty)
            return 0;

        Convert.TryToBase64Chars(input, output, out int charsWritten);

        // Fix up '+' -> '-' and '/' -> '_'. Drop padding characters.
        for (var i = 0; i < charsWritten; i++)
        {
            var ch = output[i];
            if (ch == '+')
                output[i] = '-';
            else if (ch == '/')
                output[i] = '_';
            else if (ch == '=')
                return i; // We've reached a padding character; truncate the remainder.
        }

        return charsWritten;
    }

    private static int GetArraySizeRequiredToEncode(int count)
    {
        var numWholeOrPartialInputBlocks = checked(count + 2) / 3;
        return checked(numWholeOrPartialInputBlocks * 4);
    }


    private static int GetArraySizeRequiredToDecode(int count)
    {
        if (count == 0)
            return 0;

        var numPaddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);

        return checked(count + numPaddingCharsToAdd);
    }

    private static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
    {
        return (inputLength % 4) switch
        {
            0 => 0,
            2 => 2,
            3 => 1,
            _ => throw new FormatException($"Malformed input: {inputLength} is an invalid input length."),
        };
    }
#endif
}
