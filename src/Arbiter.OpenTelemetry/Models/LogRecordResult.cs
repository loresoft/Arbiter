using System.Diagnostics;
using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.OpenTelemetry.Models;

/// <summary>
/// Represents a paged OpenTelemetry log record query result.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[MessagePackObject(true)]
public class LogRecordResult
{
    private string DebuggerDisplay => $"Data={Data?.Count ?? 0}, Descriptors={Descriptors?.Count ?? 0}";

    /// <summary>
    /// Gets or sets the log records in the result.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<LogRecord> Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the available attributes for the log records.
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
