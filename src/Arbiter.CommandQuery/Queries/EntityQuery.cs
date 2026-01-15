using System.Text.Json.Serialization;

using MessagePack;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for selecting entities with filtering, sorting, and pagination capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This class is typically used to define the criteria for querying entities, including filters, sorting, and pagination options.
/// It supports both simple property-based queries and complex filtering scenarios through the <see cref="EntityFilter"/> system.
/// </para>
/// <para>
/// The query supports multiple pagination strategies:
/// <list type="bullet">
/// <item><description>Page-based pagination using <see cref="Page"/> and <see cref="PageSize"/> properties</description></item>
/// <item><description>Continuation token-based pagination using <see cref="ContinuationToken"/> for stateless paging</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityQuery"/> class with basic filtering and sorting:
/// <code>
/// var filter = new EntityFilter
/// {
///     Name = "Status",
///     Operator = FilterOperators.Equal,
///     Value = "Active"
/// };
///
/// var sort = new EntitySort
/// {
///     Name = "Name",
///     Direction = SortDirections.Ascending
/// };
///
/// var query = new EntityQuery();
/// query.Filter = filter;
/// query.Page = 1;
/// query.PageSize = 20;
/// query.AddSort(sort);
///
/// Console.WriteLine($"Page: {query.Page}, PageSize: {query.PageSize}");
/// </code>
///
/// Example of using the fluent API for adding sorts:
/// <code>
/// var query = new EntityQuery()
///     .AddSort("Name:asc")
///     .AddSort("CreatedDate:desc");
/// </code>
/// </example>
[MessagePackObject(true)]
public class EntityQuery
{
    /// <summary>
    /// Gets or sets the raw query expression to search for entities.
    /// </summary>
    /// <value>
    /// A string containing the raw query expression, or <see langword="null"/> if no raw query is specified.
    /// This can be used for full-text search or custom query expressions depending on the underlying data provider.
    /// </value>
    [JsonPropertyName("query")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the list of sort expressions to apply to the query.
    /// </summary>
    /// <value>
    /// A collection of <see cref="EntitySort"/> objects defining the sort order, or <see langword="null"/> if no sorting is specified.
    /// Multiple sort expressions are applied in the order they appear in the collection.
    /// </value>
    /// <remarks>
    /// Sort expressions are applied in sequence, allowing for multi-level sorting (e.g., sort by category, then by name within each category).
    /// Use the <see cref="AddSort(EntitySort)"/> or <see cref="AddSort(string)"/> methods to add sort expressions fluently.
    /// </remarks>
    [JsonPropertyName("sort")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<EntitySort>? Sort { get; set; }

    /// <summary>
    /// Gets or sets the filter to apply to the query.
    /// </summary>
    /// <value>
    /// An <see cref="EntityFilter"/> object defining the filtering criteria, or <see langword="null"/> if no filtering is specified.
    /// </value>
    /// <remarks>
    /// The filter can be a simple property-based filter or a complex group filter containing multiple nested conditions
    /// combined with logical operators (AND/OR). Use <see cref="EntityFilter.IsGroup"/> to determine if the filter contains nested filters.
    /// </remarks>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EntityFilter? Filter { get; set; }

    /// <summary>
    /// Gets or sets the page number for the query.
    /// </summary>
    /// <value>
    /// The page number for the query, or <see langword="null"/> if page-based pagination is not used.
    /// The default value is 1 when pagination is enabled. Page numbers are 1-based.
    /// </value>
    /// <remarks>
    /// This property is used in conjunction with <see cref="PageSize"/> for traditional page-based pagination.
    /// When both <see cref="Page"/> and <see cref="PageSize"/> are specified, the query will return the specified page of results.
    /// </remarks>
    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the size of the page for the query.
    /// </summary>
    /// <value>
    /// The number of entities to return per page, or <see langword="null"/> if page-based pagination is not used.
    /// The default value is 20 when pagination is enabled.
    /// </value>
    /// <remarks>
    /// This property controls the maximum number of entities returned in a single query execution.
    /// It is used in conjunction with <see cref="Page"/> for page-based pagination or independently to limit result set size.
    /// </remarks>
    [JsonPropertyName("pageSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets the continuation token for retrieving the next page of results.
    /// </summary>
    /// <value>
    /// A string token that can be used to retrieve the next page of results in a stateless pagination scenario,
    /// or <see langword="null"/> if continuation token-based pagination is not supported or not available.
    /// </value>
    /// <remarks>
    /// <para>
    /// Continuation tokens provide an alternative to page-based pagination that is more efficient for large datasets
    /// and doesn't suffer from consistency issues when data is added or removed between page requests.
    /// </para>
    /// <para>
    /// The token is typically opaque and should not be parsed or modified by client code. It should be obtained
    /// from a previous query result and passed to subsequent queries to retrieve the next set of results.
    /// </para>
    /// </remarks>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Computes the hash code for the current <see cref="EntityQuery"/> instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="EntityQuery"/> instance, suitable for use in hashing algorithms and data structures like hash tables.
    /// </returns>
    /// <remarks>
    /// The hash code is computed based on the <see cref="Page"/> and <see cref="PageSize"/> properties.
    /// Note that this implementation does not include all properties in the hash code calculation.
    /// </remarks>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Query);
        hash.Add(Page);
        hash.Add(PageSize);
        hash.Add(ContinuationToken);
        hash.Add(Filter?.GetHashCode() ?? 0);

        if (Sort == null)
            return hash.ToHashCode();

        for (var i = 0; i < Sort.Count; i++)
            hash.Add(Sort[i].GetHashCode());

        return hash.ToHashCode();
    }

    /// <summary>
    /// Adds a sort expression to the query by parsing a string representation.
    /// </summary>
    /// <param name="sort">
    /// A string representation of the sort expression in the format "PropertyName Direction" (e.g., "Name asc", "Date desc"),
    /// or <see langword="null"/> to skip adding a sort expression.
    /// </param>
    /// <returns>
    /// The current <see cref="EntityQuery"/> instance to enable method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides a convenient way to add sort expressions using string notation.
    /// The string is parsed using <see cref="EntitySort.Parse(string)"/> method.
    /// </para>
    /// <para>
    /// If the <paramref name="sort"/> parameter is <see langword="null"/> or cannot be parsed, no sort expression is added
    /// and the method returns without modification.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var query = new EntityQuery()
    ///     .AddSort("Name asc")
    ///     .AddSort("CreatedDate desc");
    /// </code>
    /// </example>
    public EntityQuery AddSort(string? sort)
    {
        return AddSort(EntitySort.Parse(sort));
    }

    /// <summary>
    /// Adds a sort expression to the query.
    /// </summary>
    /// <param name="sort">
    /// An <see cref="EntitySort"/> object representing the sort expression to add,
    /// or <see langword="null"/> to skip adding a sort expression.
    /// </param>
    /// <returns>
    /// The current <see cref="EntityQuery"/> instance to enable method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the <see cref="Sort"/> collection is <see langword="null"/>, it will be initialized as an empty list before adding the sort expression.
    /// Multiple sort expressions can be added and will be applied in the order they were added.
    /// </para>
    /// <para>
    /// If the <paramref name="sort"/> parameter is <see langword="null"/>, no sort expression is added
    /// and the method returns without modification.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var nameSort = new EntitySort { Name = "Name", Direction = SortDirections.Ascending };
    /// var dateSort = new EntitySort { Name = "CreatedDate", Direction = SortDirections.Descending };
    ///
    /// var query = new EntityQuery()
    ///     .AddSort(nameSort)
    ///     .AddSort(dateSort);
    /// </code>
    /// </example>
    public EntityQuery AddSort(EntitySort? sort)
    {
        if (sort == null)
            return this;

        Sort ??= [];
        Sort.Add(sort);

        return this;
    }
}
