using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// An entity query for selecting entities with a filter and sort.
/// </summary>
public class EntitySelect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class.
    /// </summary>
    [JsonConstructor]
    public EntitySelect()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class.
    /// </summary>
    /// <param name="query">The raw query expression</param>
    /// <param name="sort">The sort expression</param>
    public EntitySelect(string? query, string? sort)
    {
        Query = query;

        var entitySort = EntitySort.Parse(sort);
        if (entitySort == null)
            return;

        Sort = new List<EntitySort> { entitySort };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class.
    /// </summary>
    /// <param name="filter">The filter to apply to the query</param>
    public EntitySelect(EntityFilter? filter)
    {
        Filter = filter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class.
    /// </summary>
    /// <param name="filter">The filter to apply to the query</param>
    /// <param name="sort">The sort expression for the query</param>
    public EntitySelect(EntityFilter? filter, EntitySort? sort)
    {
        Filter = filter;

        if (sort != null)
            Sort = [sort];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class.
    /// </summary>
    /// <param name="filter">The filter to apply to the query</param>
    /// <param name="sort">The list of sort expressions for the query</param>
    public EntitySelect(EntityFilter? filter, IEnumerable<EntitySort>? sort)
    {
        Filter = filter;
        Sort = sort?.ToList();
    }

    /// <summary>
    /// The raw query to search for entities.
    /// </summary>
    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Query { get; set; }

    /// <summary>
    /// The list of sort expressions to apply to the query.
    /// </summary>
    [JsonPropertyName("sort")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<EntitySort>? Sort { get; set; }

    /// <summary>
    /// The filter to apply to the query.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EntityFilter? Filter { get; set; }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Query);

        if (Filter != null)
            hash.Add(Filter.GetHashCode());

        if (Sort == null)
            return hash.ToHashCode();

        foreach (var s in Sort)
            hash.Add(s.GetHashCode());

        return hash.ToHashCode();
    }
}
