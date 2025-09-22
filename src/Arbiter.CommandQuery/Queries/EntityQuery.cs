using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Filters;

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
public class EntityQuery : EntitySelect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class with default pagination settings.
    /// </summary>
    [JsonConstructor]
    public EntityQuery()
    {
        Page = 1;
        PageSize = 20;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class with a raw query expression and pagination settings.
    /// </summary>
    /// <param name="query">The raw query expression.</param>
    /// <param name="page">The page number for the query.</param>
    /// <param name="pageSize">The size of the page for the query.</param>
    /// <param name="sort">The sort expression for the query.</param>
    public EntityQuery(string? query, int page, int pageSize, string? sort)
        : base(query, sort)
    {
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class with a filter and pagination settings.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <param name="page">The page number for the query. The default page is 1.</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20.</param>
    public EntityQuery(EntityFilter? filter, int page = 1, int pageSize = 20)
        : this(filter, Array.Empty<EntitySort>(), page, pageSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class with a filter, a single sort expression, and pagination settings.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <param name="sort">The sort expression for the query.</param>
    /// <param name="page">The page number for the query. The default page is 1.</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20.</param>
    public EntityQuery(EntityFilter? filter, EntitySort? sort, int page = 1, int pageSize = 20)
        : this(filter, sort != null ? new[] { sort } : null, page, pageSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class with a filter, multiple sort expressions, and pagination settings.
    /// </summary>
    /// <param name="filter">The filter to apply to the query.</param>
    /// <param name="sort">The list of sort expressions for the query.</param>
    /// <param name="page">The page number for the query. The default page is 1.</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20.</param>
    public EntityQuery(EntityFilter? filter, IEnumerable<EntitySort>? sort, int page = 1, int pageSize = 20)
        : base(filter, sort)
    {
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Gets or sets the page number for the query.
    /// </summary>
    /// <value>
    /// The page number for the query. The default value is 1.
    /// </value>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the size of the page for the query.
    /// </summary>
    /// <value>
    /// The size of the page for the query. The default value is 20.
    /// </value>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

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
}
