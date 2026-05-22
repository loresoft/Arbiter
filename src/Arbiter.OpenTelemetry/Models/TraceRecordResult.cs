using System.Diagnostics;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents a paged OpenTelemetry trace record query result.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[MessagePackObject(true)]
public class TraceRecordResult
{
    private string DebuggerDisplay => $"Data={Data?.Count ?? 0}, Descriptors={Descriptors?.Count ?? 0}";

    /// <summary>
    /// Gets or sets the trace records in the result.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<TraceRecord> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the available attributes for the trace records.
    /// </summary>
    [JsonPropertyName("attributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<TelemetryDescriptor> Descriptors { get; set; } = [];

    /// <summary>
    /// Gets or sets the token used to continue reading subsequent result pages.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }
}
