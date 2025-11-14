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
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("asc")]
#endif
    Ascending = 0,
    /// <summary>
    /// The descending sort direction.
    /// </summary>
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("desc")]
#endif
    Descending = 1,
}
