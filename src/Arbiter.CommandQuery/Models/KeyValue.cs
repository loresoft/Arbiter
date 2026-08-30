using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// Represents a key and associated value.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value associated with the key.</typeparam>
[Equatable]
[MessagePackObject(true)]
public partial class KeyValue<TKey, TValue>
{
    /// <summary>
    /// Gets or sets the key.
    /// </summary>
    [JsonPropertyName("key")]
    public TKey Key { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value associated with the key.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TValue? Value { get; set; }

    /// <summary>
    /// Returns a string representation of the key-value pair.
    /// </summary>
    /// <returns>A string representation of the key-value pair.</returns>
    public override string ToString()
    {
        return $"KeyValue: Key = {Key}, Value = {Value}";
    }
}
