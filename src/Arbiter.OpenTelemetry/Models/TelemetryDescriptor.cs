using System.Diagnostics;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents an available telemetry attribute with a name and optional type.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Equatable, MessagePackObject(true)]
public partial class TelemetryDescriptor
{
    private string DebuggerDisplay => $"{Name} {Type}";

    /// <summary>
    /// Gets or sets the attribute name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional attribute type.
    /// </summary>
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Type { get; set; }
}
