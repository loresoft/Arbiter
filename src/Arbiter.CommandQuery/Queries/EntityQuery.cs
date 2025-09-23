using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for selecting entities with filtering, sorting, and pagination capabilities.
/// </summary>
/// <remarks>
/// This class is typically used to define the criteria for querying entities, including filters, sorting, and pagination options.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityQuery"/> class:
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
/// var query = new EntityQuery(filter, sort, page: 1, pageSize: 20);
/// Console.WriteLine($"Page: {query.Page}, PageSize: {query.PageSize}");
/// </code>
/// </example>
public class EntityQuery
{
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
    /// Gets or sets the page number for the query.
    /// </summary>
    /// <value>
    /// The page number for the query. The default value is 1.
    /// </value>
    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the size of the page for the query.
    /// </summary>
    /// <value>
    /// The size of the page for the query. The default value is 20.
    /// </value>
    [JsonPropertyName("pageSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets the continuation token for retrieving the next page of results.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; }

    /// <summary>
    /// Computes the hash code for the current <see cref="EntityQuery"/> instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="EntityQuery"/> instance.
    /// </returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Page, PageSize);
    }


    public EntityQuery AddSort(string? sort)
    {
        return AddSort(EntitySort.Parse(sort));
    }

    public EntityQuery AddSort(EntitySort? sort)
    {
        if (sort == null)
            return this;

        Sort ??= [];
        Sort.Add(sort);

        return this;
    }
}
