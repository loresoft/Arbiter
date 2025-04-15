using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// An update model base <see langword="class"/>
/// </summary>
/// <seealso cref="ITrackUpdated" />
/// <seealso cref="ITrackConcurrency" />
public class EntityUpdateModel : ITrackUpdated, ITrackConcurrency
{
    /// <inheritdoc />
    [JsonPropertyName("updated")]
    [JsonPropertyOrder(9992)]
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    [JsonPropertyName("updatedBy")]
    [JsonPropertyOrder(9993)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdatedBy { get; set; }

    /// <inheritdoc />
    [JsonPropertyName("rowVersion")]
    [JsonPropertyOrder(9999)]
    public long RowVersion { get; set; }
}
