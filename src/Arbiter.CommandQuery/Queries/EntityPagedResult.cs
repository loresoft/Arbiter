using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A paged result for an entity query.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public class EntityPagedResult<TReadModel>
{
    /// <summary>
    /// The total number of the results for the query.
    /// </summary>
    [JsonPropertyName("total")]
    public long Total { get; set; }

    /// <summary>
    /// The current page of data for the query.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<TReadModel>? Data { get; set; }
}
