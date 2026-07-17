using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// Represents an aggregated health report that contains overall status and individual health check entries.
/// </summary>
[MessagePackObject(true)]
public class HealthReportModel
{
    /// <summary>
    /// Gets or sets the overall status of the health report.
    /// </summary>
    /// <value>
    /// The overall health status value.
    /// </value>
    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    /// <summary>
    /// Gets or sets the total duration of all executed health checks.
    /// </summary>
    /// <value>
    /// The total health check execution duration, or <see langword="null"/> when not provided.
    /// </value>
    [JsonPropertyName("totalDuration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimeSpan? TotalDuration { get; set; }

    /// <summary>
    /// Gets or sets the collection of health check entries included in the report.
    /// </summary>
    /// <value>
    /// A list of individual health check entries.
    /// </value>
    [JsonPropertyName("entries")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<HealthEntryModel>? Entries { get; set; }
}
