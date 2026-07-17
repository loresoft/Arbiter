using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// Represents a single health check entry with status and diagnostic details.
/// </summary>
[MessagePackObject(true)]
public class HealthEntryModel
{
    /// <summary>
    /// Gets or sets the unique key that identifies the health check entry.
    /// </summary>
    /// <value>
    /// The health check entry key.
    /// </value>
    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    /// <summary>
    /// Gets or sets the human-readable description of the health check.
    /// </summary>
    /// <value>
    /// The description of the health check, or <see langword="null"/> when not provided.
    /// </value>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the duration of the health check execution.
    /// </summary>
    /// <value>
    /// The execution duration, or <see langword="null"/> when not provided.
    /// </value>
    [JsonPropertyName("duration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Gets or sets an informational message for the health check entry.
    /// </summary>
    /// <value>
    /// The informational message, or <see langword="null"/> when not provided.
    /// </value>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the exception details captured by the health check.
    /// </summary>
    /// <value>
    /// The exception details, or <see langword="null"/> when no exception occurred.
    /// </value>
    [JsonPropertyName("exception")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Exception { get; set; }

    /// <summary>
    /// Gets or sets the status of the health check entry.
    /// </summary>
    /// <value>
    /// The health status value, or <see langword="null"/> when not provided.
    /// </value>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the collection of tags associated with the health check entry.
    /// </summary>
    /// <value>
    /// A list of tags for the health check entry, or <see langword="null"/> when no tags are provided.
    /// </value>
    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Tags { get; set; }
}
