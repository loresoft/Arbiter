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
        using var compressionStream = new BrotliStream(outputStream, CompressionLevel.Optimal);
        using var jsonWriter = new Utf8JsonWriter(compressionStream);

        JsonSerializer.Serialize(jsonWriter, value, options);

        var jsonBytes = outputStream.ToArray();

        return Base64Url.EncodeToString(jsonBytes);
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

        var jsonBytes = Base64Url.DecodeFromChars(encodedQueryString);

        options ??= new JsonSerializerOptions(JsonSerializerDefaults.Web);

        using var inputStream = new MemoryStream(jsonBytes);
        using var compressionStream = new BrotliStream(inputStream, CompressionMode.Decompress);

        return JsonSerializer.Deserialize<T>(compressionStream, options);
    }
}
