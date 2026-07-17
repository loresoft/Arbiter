using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// Represents a machine-readable error payload for HTTP API responses, based on RFC 7807.
/// </summary>
[MessagePackObject(true)]
public class ProblemDetails
{
    /// <summary>
    /// Gets the media type for a problem details JSON response.
    /// </summary>
    /// <value>
    /// The <c>application/problem+json</c> media type.
    /// </value>
    public const string ContentType = "application/problem+json";

    /// <summary>
    /// Gets or sets a URI reference that identifies the problem type.
    /// </summary>
    /// <value>
    /// A URI for the problem type, or <see langword="null"/> when not provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-5)]
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets a short, human-readable summary of the problem type.
    /// </summary>
    /// <value>
    /// The problem title, or <see langword="null"/> when not provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-4)]
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code generated for this occurrence of the problem.
    /// </summary>
    /// <value>
    /// The HTTP status code, or <see langword="null"/> when not provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-3)]
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>
    /// Gets or sets a human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    /// <value>
    /// The problem detail message, or <see langword="null"/> when not provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-2)]
    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    /// <summary>
    /// Gets or sets a URI reference that identifies the specific occurrence of the problem.
    /// </summary>
    /// <value>
    /// The problem instance URI, or <see langword="null"/> when not provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyOrder(-1)]
    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    /// <summary>
    /// Gets or sets exception details associated with the problem.
    /// </summary>
    /// <value>
    /// The exception details, or <see langword="null"/> when no exception details are provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("exception")]
    public string? Exception { get; set; }

    /// <summary>
    /// Gets or sets the validation errors associated with this instance of problem details.
    /// </summary>
    /// <value>
    /// A dictionary of validation errors keyed by field name, or <see langword="null"/> when not provided.
    /// </value>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("errors")]
    public IDictionary<string, string[]>? Errors { get; set; }
}
