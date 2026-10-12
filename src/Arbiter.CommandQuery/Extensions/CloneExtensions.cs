using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Arbiter.CommandQuery.Extensions;

/// <summary>
/// Extension methods for creating deep clones of objects via serialization.
/// </summary>
public static class CloneExtensions
{
    /// <summary>
    /// Creates a deep clone of <paramref name="value"/> using System.Text.Json serialization.
    /// </summary>
    /// <typeparam name="T">The type of the value to clone.</typeparam>
    /// <param name="value">The value to clone.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>A deep clone of the value, or <see langword="null"/> when the value is <see langword="null"/>.</returns>
    [RequiresUnreferencedCode("JSON serialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo.")]
    [RequiresDynamicCode("JSON serialization might require runtime code generation. Use the overload that takes a JsonTypeInfo.")]
    [return: NotNullIfNotNull(nameof(value))]
    public static T? JsonClone<T>(this T? value, JsonSerializerOptions? options = null)
    {
        if (value is null)
            return default;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, options);
        return JsonSerializer.Deserialize<T>(bytes, options)!;
    }

    /// <summary>
    /// Creates a deep clone of <paramref name="value"/> using System.Text.Json serialization with the specified type metadata.
    /// </summary>
    /// <typeparam name="T">The type of the value to clone.</typeparam>
    /// <param name="value">The value to clone.</param>
    /// <param name="jsonTypeInfo">The JSON type metadata used for serialization.</param>
    /// <returns>A deep clone of the value, or <see langword="null"/> when the value is <see langword="null"/>.</returns>
    [return: NotNullIfNotNull(nameof(value))]
    public static T? JsonClone<T>(this T? value, JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        if (value is null)
            return default;

        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo);
        return JsonSerializer.Deserialize(bytes, jsonTypeInfo)!;
    }
}
