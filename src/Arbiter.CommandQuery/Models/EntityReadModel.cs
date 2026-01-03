using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// A read model base <see langword="class"/>
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <seealso cref="EntityIdentifierModel{TKey}" />
/// <seealso cref="ITrackCreated" />
/// <seealso cref="ITrackUpdated" />
/// <seealso cref="ITrackConcurrency" />
[MessagePackObject]
public partial class EntityReadModel<TKey> : EntityIdentifierModel<TKey>, ITrackCreated, ITrackUpdated, ITrackConcurrency
{
    /// <inheritdoc />
    [Key(1)]
    [JsonPropertyName("created")]
    [JsonPropertyOrder(9990)]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    [Key(2)]
    [JsonPropertyName("createdBy")]
    [JsonPropertyOrder(9991)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    [Key(3)]
    [JsonPropertyName("updated")]
    [JsonPropertyOrder(9992)]
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    [Key(4)]
    [JsonPropertyName("updatedBy")]
    [JsonPropertyOrder(9993)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdatedBy { get; set; }

    /// <inheritdoc />
    [Key(5)]
    [JsonPropertyName("rowVersion")]
    [JsonPropertyOrder(9999)]
    public long RowVersion { get; set; }
}
