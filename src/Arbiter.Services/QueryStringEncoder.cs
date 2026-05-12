using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Arbiter.Services;

/// <summary>
/// A service to encode and decode values for query strings.
/// </summary>
public static class QueryStringEncoder
{
    private const string JsonSerializerOptionsRequiresDynamicCode = "JSON serialization with JsonSerializerOptions might require runtime code generation. Use the JsonTypeInfo<T> overload for Native AOT applications.";
    private const string JsonSerializerOptionsRequiresUnreferencedCode = "JSON serialization with JsonSerializerOptions might require types that cannot be statically analyzed. Use the JsonTypeInfo<T> overload for trimmed applications.";

    /// <summary>
    /// Encodes a value to a query string format.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The value to encode</param>
    /// <param name="options">The JSON options to use for serialization</param>
    /// <returns>A query string encode value</returns>
    [RequiresDynamicCode(JsonSerializerOptionsRequiresDynamicCode)]
    [RequiresUnreferencedCode(JsonSerializerOptionsRequiresUnreferencedCode)]
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
    /// Encodes a value to a query string format using source-generated JSON metadata.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="value">The value to encode</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata to use for serialization</param>
    /// <returns>A query string encode value</returns>
    public static string? Encode<T>(T value, JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        if (value is null)
            return null;

        using var outputStream = new MemoryStream();
        using var compressionStream = new BrotliStream(outputStream, CompressionLevel.Optimal);
        using var jsonWriter = new Utf8JsonWriter(compressionStream);

        JsonSerializer.Serialize(jsonWriter, value, jsonTypeInfo);

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
    [RequiresDynamicCode(JsonSerializerOptionsRequiresDynamicCode)]
    [RequiresUnreferencedCode(JsonSerializerOptionsRequiresUnreferencedCode)]
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

    /// <summary>
    /// Decodes a query string value to its original format using source-generated JSON metadata.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="encodedQueryString">The encoded query string format</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata to use for serialization</param>
    /// <returns>The instance decoded from the query string format</returns>
    public static T? Decode<T>(string? encodedQueryString, JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        if (string.IsNullOrWhiteSpace(encodedQueryString))
            return default;

        var jsonBytes = Base64Url.DecodeFromChars(encodedQueryString);

        using var inputStream = new MemoryStream(jsonBytes);
        using var compressionStream = new BrotliStream(inputStream, CompressionMode.Decompress);

        return JsonSerializer.Deserialize(compressionStream, jsonTypeInfo);
    }
}
