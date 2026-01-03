using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// An update model base <see langword="class"/>
/// </summary>
/// <seealso cref="ITrackUpdated" />
/// <seealso cref="ITrackConcurrency" />
[MessagePackObject]
public partial class EntityUpdateModel : ITrackUpdated, ITrackConcurrency
{
    /// <inheritdoc />
    [Key(0)]
    [JsonPropertyName("updated")]
    [JsonPropertyOrder(9992)]
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    [Key(1)]
    [JsonPropertyName("updatedBy")]
    [JsonPropertyOrder(9993)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdatedBy { get; set; }

    /// <inheritdoc />
    [Key(2)]
    [JsonPropertyName("rowVersion")]
    [JsonPropertyOrder(9999)]
    public long RowVersion { get; set; }
}
