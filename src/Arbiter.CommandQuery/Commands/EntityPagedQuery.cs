using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Queries;
using Arbiter.Services;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a query for retrieving paged entities based on an <see cref="EntityQuery"/>.
/// The result of the query will be of type <see cref="EntityPagedResult{TReadModel}"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the query.</typeparam>
/// <remarks>
/// <para>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve entities
/// in a paginated format. The <see cref="EntityQuery"/> allows filtering, sorting, and pagination criteria to be specified.
/// </para>
/// <para>
/// This query supports caching through the <see cref="CacheableQueryBase{TResponse}"/> base class, allowing results
/// to be cached with configurable expiration policies to improve performance for frequently accessed data.
/// </para>
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityPagedQuery{TReadModel}"/>:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var entityQuery = new EntityQuery
/// {
///     Filter = new EntityFilter { Name = "Status", Operator = FilterOperators.Equal, Value = "Active" },
///     Sort = new List&lt;EntitySort&gt; { new EntitySort { Name = "Name", Direction = SortDirections.Ascending } },
///     Page = 1,
///     PageSize = 20
/// };
///
/// var query = new EntityPagedQuery&lt;ProductReadModel&gt;(principal, entityQuery);
///
/// // Optionally configure caching
/// query.Cache(TimeSpan.FromMinutes(5));
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
/// Console.WriteLine($"Total Results: {result?.Total}");
/// Console.WriteLine($"Page Size: {result?.Data?.Count}");
/// </code>
/// </example>
/// <seealso cref="EntityQuery"/>
/// <seealso cref="EntityPagedResult{TReadModel}"/>
/// <seealso cref="CacheableQueryBase{TResponse}"/>
[MessagePackObject]
public partial record EntityPagedQuery<TReadModel> : CacheableQueryBase<EntityPagedResult<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPagedQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="query">The <see cref="EntityQuery"/> defining the filter, sort, and pagination criteria for the query. If <see langword="null"/>, a new empty <see cref="EntityQuery"/> will be created.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows for named query modification strategies to be applied.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables the use of named query modification pipelines, allowing different
    /// filtering, security, or transformation strategies to be applied based on the context of the query execution.
    /// </para>
    /// <para>
    /// If <paramref name="query"/> is <see langword="null"/>, a default empty <see cref="EntityQuery"/> is initialized,
    /// which can be useful for retrieving all entities with default pagination settings.
    /// </para>
    /// </remarks>
    public EntityPagedQuery(ClaimsPrincipal? principal, EntityQuery? query, string? filterName = null)
        : base(principal)
    {
        Query = query ?? new EntityQuery();
        FilterName = filterName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPagedQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="query">The <see cref="EntityQuery"/> defining the filter, sort, and pagination criteria for the query. If <see langword="null"/>, a new empty <see cref="EntityQuery"/> will be created.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows different query modification strategies to be applied based on context.</param>
    /// <remarks>
    /// <para>
    /// If <paramref name="query"/> is <see langword="null"/>, a default empty <see cref="EntityQuery"/> is initialized,
    /// which can be useful for retrieving all entities with default pagination settings.
    /// </para>
    /// </remarks>
    [JsonConstructor]
    [SerializationConstructor]
    public EntityPagedQuery(EntityQuery? query, string? filterName = null)
        : this(principal: null, query, filterName)
    {
    }

    /// <summary>
    /// Gets the <see cref="EntityQuery"/> defining the filter, sort, and pagination criteria for the query.
    /// </summary>
    /// <value>
    /// An <see cref="EntityQuery"/> object containing the filtering, sorting, and pagination configuration.
    /// This property is never <see langword="null"/> as it is initialized in the constructor.
    /// </value>
    [Key(0)]
    [JsonPropertyName("query")]
    public EntityQuery Query { get; }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during query execution.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if no specific pipeline is specified.
    /// </value>
    /// <remarks>
    /// This property allows for named query modification strategies to be applied, enabling different filtering,
    /// security policies, or data transformations based on the execution context. The specific behavior depends
    /// on the registered query pipeline modifiers in the application.
    /// </remarks>
    [Key(1)]
    [JsonPropertyName("filterName")]
    public string? FilterName { get; }

    /// <summary>
    /// Generates a cache key for the query based on the <see cref="EntityQuery"/> hash code.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query, combining the entity type, result type, and query parameters.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The cache key is generated using the <see cref="CacheTagger"/> utility with the <see cref="CacheTagger.Buckets.Paged"/> bucket,
    /// ensuring that paged query results are properly isolated in the cache namespace.
    /// </para>
    /// <para>
    /// The key includes the hash code of the <see cref="Query"/> property, which is computed from pagination parameters
    /// such as page number and page size, ensuring that different pages are cached separately.
    /// </para>
    /// </remarks>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Paged, Query.GetHashCode());

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/> entity type.
    /// </summary>
    /// <returns>
    /// The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Cache tags enable cache invalidation strategies where all cached queries for a specific entity type
    /// can be invalidated when the underlying data changes. This is particularly useful for maintaining
    /// cache consistency in CQRS architectures.
    /// </para>
    /// <para>
    /// The tag is generated using the <see cref="CacheTagger"/> utility and is based on the
    /// <typeparamref name="TReadModel"/> type, allowing all paged queries for that entity type to be
    /// grouped and invalidated together when necessary.
    /// </para>
    /// </remarks>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
