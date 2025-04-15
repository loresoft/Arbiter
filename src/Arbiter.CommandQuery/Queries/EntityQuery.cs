using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// An entity query for selecting entities with a filter, sort and pagination.
/// </summary>
public class EntityQuery : EntitySelect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class.
    /// </summary>
    [JsonConstructor]
    public EntityQuery()
    {
        Page = 1;
        PageSize = 20;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class.
    /// </summary>
    /// <param name="query">The raw query expression</param>
    /// <param name="page">The page number for the query</param>
    /// <param name="pageSize">The size of the page for the query</param>
    /// <param name="sort">The sort expression</param>
    public EntityQuery(string? query, int page, int pageSize, string? sort)
        : base(query, sort)
    {
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class.
    /// </summary>
    /// <param name="filter">The filter to apply to the query</param>
    /// <param name="page">The page number for the query. The default page is 1</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20</param>
    public EntityQuery(EntityFilter? filter, int page = 1, int pageSize = 20)
        : this(filter, [], page, pageSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class.
    /// </summary>
    /// <param name="filter">The filter to apply to the query</param>
    /// <param name="sort">The sort expression for the query</param>
    /// <param name="page">The page number for the query. The default page is 1</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20</param>
    public EntityQuery(EntityFilter? filter, EntitySort? sort, int page = 1, int pageSize = 20)
        : this(filter, sort != null ? [sort] : null, page, pageSize)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityQuery"/> class.
    /// </summary>
    /// <param name="filter">The filter to apply to the query</param>
    /// <param name="sort">The list of sort expressions for the query</param>
    /// <param name="page">The page number for the query. The default page is 1</param>
    /// <param name="pageSize">The size of the page for the query. The default page size is 20</param>
    public EntityQuery(EntityFilter? filter, IEnumerable<EntitySort>? sort, int page = 1, int pageSize = 20)
        : base(filter, sort)
    {
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// The page number for the query.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// The size of the page for the query.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Page, PageSize);
    }
}
