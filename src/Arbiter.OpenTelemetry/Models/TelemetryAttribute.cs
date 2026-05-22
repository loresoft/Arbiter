using System.Diagnostics;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents an OpenTelemetry attribute value and its optional type metadata.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Equatable, MessagePackObject(true)]
public partial class TelemetryAttribute
{
    private string DebuggerDisplay => $"{Name} {Type}";

    /// <summary>
    /// Gets or sets the value name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value content.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the optional value type.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Type { get; set; }
}
