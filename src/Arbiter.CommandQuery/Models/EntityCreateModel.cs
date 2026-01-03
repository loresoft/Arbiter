using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Definitions;

using MessagePack;

namespace Arbiter.CommandQuery.Models;

/// <summary>
/// A create model base <see langword="class"/>
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <seealso cref="EntityIdentifierModel{TKey}" />
/// <seealso cref="ITrackCreated" />
/// <seealso cref="ITrackUpdated" />
[MessagePackObject]
public partial class EntityCreateModel<TKey> : EntityIdentifierModel<TKey>, ITrackCreated, ITrackUpdated
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
}
