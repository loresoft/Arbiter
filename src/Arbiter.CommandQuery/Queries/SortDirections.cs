using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents the sort directions for an entity.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SortDirections>))]
public enum SortDirections
{
    /// <summary>
    /// The ascending sort direction.
    /// </summary>
    [JsonStringEnumMemberName("asc")]
    Ascending = 0,
    /// <summary>
    /// The descending sort direction.
    /// </summary>
    [JsonStringEnumMemberName("desc")]
    Descending = 1,
}
