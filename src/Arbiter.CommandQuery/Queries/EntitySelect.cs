using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for selecting entities with optional filtering and sorting criteria.
/// </summary>
/// <remarks>
/// This class is typically used to define the selection criteria for querying entities, including filters, sorting, and raw query expressions.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntitySelect"/> class:
/// <code>
/// var filter = new EntityFilter
/// {
///     Name = "Status",
///     Operator = "eq",
///     Value = "Active"
/// };
///
/// var sort = new EntitySort
/// {
///     Name = "Name",
///     Direction = "asc"
/// };
///
/// var entitySelect = new EntitySelect(filter, sort);
/// </code>
/// </example>
public class EntitySelect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class with no filtering or sorting criteria.
    /// </summary>
    [JsonConstructor]
    public EntitySelect()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class with a raw query expression and a sort expression.
    /// </summary>
    /// <param name="query">The raw query expression.</param>
    /// <param name="sort">The sort expression in the format "PropertyName:Direction".</param>
    public EntitySelect(string? query, string? sort)
    {
        Query = query;

        var entitySort = EntitySort.Parse(sort);
        if (entitySort == null)
            return;

        Sort = new List<EntitySort> { entitySort };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class with a filter.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    public EntitySelect(EntityFilter? filter)
    {
        Filter = filter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class with a filter and a single sort expression.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <param name="sort">The sort expression for the query.</param>
    public EntitySelect(EntityFilter? filter, EntitySort? sort)
    {
        Filter = filter;

        if (sort != null)
            Sort = [sort];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitySelect"/> class with a filter and a list of sort expressions.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <param name="sort">The list of sort expressions for the query.</param>
    public EntitySelect(EntityFilter? filter, IEnumerable<EntitySort>? sort)
    {
        Filter = filter;
        Sort = sort?.ToList();
    }

    /// <summary>
    /// Gets or sets the raw query expression to search for entities.
    /// </summary>
    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the list of sort expressions to apply to the query.
    /// </summary>
    [JsonPropertyName("sort")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<EntitySort>? Sort { get; set; }

    /// <summary>
    /// Gets or sets the filter to apply to the query.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EntityFilter? Filter { get; set; }

    /// <summary>
    /// Computes the hash code for the current <see cref="EntitySelect"/> instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="EntitySelect"/> instance.
    /// </returns>
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
