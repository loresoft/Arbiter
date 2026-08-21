using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// Represents a configuration entry with its resolved value and source provider.
/// </summary>
[MessagePackObject(true)]
public class ConfigurationValue
{
    /// <summary>
    /// Gets or sets the configuration key path.
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the resolved configuration value.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the configuration provider that supplied the value.
    /// </summary>
    [JsonPropertyName("provider")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Provider { get; set; }
}
