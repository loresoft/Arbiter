using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.Services;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a query for retrieving multiple entities identified by a list of keys.
/// </summary>
/// <typeparam name="TKey">The type of the keys used to identify the entities.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned for each entity in the result collection.</typeparam>
/// <remarks>
/// <para>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve multiple entities
/// based on their unique identifiers in a single operation. It inherits from <see cref="CacheableQueryBase{TResponse}"/>
/// to support caching, which can significantly improve performance when the same set of entities is frequently accessed together.
/// </para>
/// <para>
/// The query returns an <see cref="IReadOnlyList{T}"/> of <typeparamref name="TReadModel"/> instances, maintaining the
/// order and allowing for efficient batch retrieval of entities. This is particularly useful for scenarios such as:
/// <list type="bullet">
/// <item><description>Loading multiple related entities in a single database query</description></item>
/// <item><description>Retrieving selected items from a user's shopping cart or favorites</description></item>
/// <item><description>Fetching a batch of records for display or processing</description></item>
/// <item><description>Loading entities based on a pre-filtered list of identifiers</description></item>
/// </list>
/// </para>
/// <para>
/// The query supports optional named filter pipelines through the <see cref="FilterName"/> property, allowing different
/// query modification strategies to be applied based on the execution context.
/// </para>
/// <para>
/// Cache keys are generated based on the hash of all entity identifiers, ensuring that the same set of identifiers
/// (regardless of query parameters) produces the same cache key for efficient cache retrieval.
/// </para>
/// </remarks>
/// <example>
/// Example demonstrating retrieval of multiple entities by their identifiers:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var ids = new List&lt;int&gt; { 1, 2, 3, 5, 8 };
/// var query = new EntityIdentifiersQuery&lt;int, ProductReadModel&gt;(principal, ids);
///
/// // Optionally configure caching
/// query.Cache(TimeSpan.FromMinutes(15));
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
///
/// Console.WriteLine($"Retrieved {result?.Count} out of {ids.Count} products.");
/// foreach (var product in result ?? Enumerable.Empty&lt;ProductReadModel&gt;())
/// {
///     Console.WriteLine($"- {product.Name}: {product.Price:C}");
/// }
/// </code>
/// </example>
/// <example>
/// Example using a named filter pipeline for administrative batch access:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));
/// var orderIds = new List&lt;Guid&gt; { guid1, guid2, guid3 };
/// var query = new EntityIdentifiersQuery&lt;Guid, OrderReadModel&gt;(
///     principal,
///     orderIds,
///     filterName: "admin-batch");
///
/// // This might include archived or deleted orders that would normally be filtered out
/// var orders = await mediator.Send(query);
/// </code>
/// </example>
/// <seealso cref="CacheableQueryBase{TResponse}"/>
/// <seealso cref="EntityIdentifierQuery{TKey, TReadModel}"/>
/// <seealso cref="EntityPagedQuery{TReadModel}"/>
[MessagePackObject]
public partial record EntityIdentifiersQuery<TKey, TReadModel> : CacheableQueryBase<IReadOnlyList<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersQuery{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query. Used for authorization and audit logging.</param>
    /// <param name="ids">The list of identifiers for the entities to retrieve. This collection cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows different query modification strategies to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="principal"/> parameter is used for authorization checks to ensure the user has permission
    /// to access the requested entities. If <see langword="null"/>, the operation is considered a system-level query.
    /// </para>
    /// <para>
    /// The <paramref name="ids"/> collection can be empty, in which case an empty result set will be returned.
    /// The order of identifiers in the collection may affect the order of results, depending on the query handler implementation.
    /// </para>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables named query pipeline scenarios, such as:
    /// <list type="bullet">
    /// <item><description>Administrative batch operations that include additional fields or bypass certain filters</description></item>
    /// <item><description>Public batch views with restricted data visibility</description></item>
    /// <item><description>Bulk operations with enhanced logging and tracking</description></item>
    /// <item><description>Different security policies based on user roles or batch operation context</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public EntityIdentifiersQuery(ClaimsPrincipal? principal, [NotNull] IReadOnlyList<TKey> ids, string? filterName = null)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(ids);

        Ids = ids;
        FilterName = filterName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersQuery{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="ids">The list of identifiers for the entities to retrieve. This collection cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows different query modification strategies to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is <see langword="null"/>.</exception>
    [JsonConstructor]
    [SerializationConstructor]
    public EntityIdentifiersQuery([NotNull] IReadOnlyList<TKey> ids, string? filterName = null)
        : this(principal: null, ids, filterName)
    {
    }

    /// <summary>
    /// Gets the list of identifiers for the entities to retrieve.
    /// </summary>
    /// <value>
    /// A read-only list of unique identifiers. This value is guaranteed to be non-null after construction.
    /// </value>
    /// <remarks>
    /// <para>
    /// This collection contains the identifiers of all entities that should be retrieved in the batch operation.
    /// The collection is read-only to prevent modification after the query is constructed.
    /// </para>
    /// <para>
    /// An empty collection is valid and will result in an empty result set. The order of identifiers in this
    /// collection may influence the order of results returned, depending on the handler implementation.
    /// </para>
    /// <para>
    /// All identifiers in the collection are incorporated into the cache key generation to ensure that
    /// different sets of identifiers produce different cache entries.
    /// </para>
    /// </remarks>
    [Key(0)]
    [NotNull]
    [JsonPropertyName("ids")]
    public IReadOnlyList<TKey> Ids { get; }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during query execution.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if the default query pipeline should be used.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows for named query modification strategies to be applied during batch entity retrieval,
    /// enabling different filtering, security policies, or data transformations based on the execution context.
    /// </para>
    /// <para>
    /// The specific behavior depends on the registered query pipeline modifiers in the application.
    /// Common scenarios include applying different field projections, security filters, or data enrichment
    /// strategies based on the pipeline name for batch operations.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the default query pipeline (if any) will be applied to the batch retrieval.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using a named pipeline for bulk administrative access
    /// var query = new EntityIdentifiersQuery&lt;int, ProductReadModel&gt;(
    ///     adminPrincipal,
    ///     productIds,
    ///     filterName: "bulk-admin");
    /// </code>
    /// </example>
    [Key(1)]
    [JsonPropertyName("filterName")]
    public string? FilterName { get; }

    /// <summary>
    /// Generates a cache key for the query based on the hash of all entity identifiers.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query, combining the entity type and the hash of all identifiers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The cache key is generated using the <see cref="CacheTagger"/> utility with the <see cref="CacheTagger.Buckets.Identifiers"/> bucket,
    /// ensuring that batch queries are properly isolated in the cache namespace from other query types (such as single identifier queries or paged queries).
    /// </para>
    /// <para>
    /// The key includes a hash code computed from all identifiers in the <see cref="Ids"/> collection using <see cref="HashCode"/>.
    /// This ensures that:
    /// <list type="bullet">
    /// <item><description>The same set of identifiers always produces the same cache key</description></item>
    /// <item><description>Different sets of identifiers produce different cache keys</description></item>
    /// <item><description>The order of identifiers affects the cache key (different orders = different cache entries)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// This approach provides efficient cache key generation even for large collections of identifiers while
    /// maintaining uniqueness for different identifier sets.
    /// </para>
    /// </remarks>
    public override string GetCacheKey()
    {
        var hash = new HashCode();

        foreach (var id in Ids)
            hash.Add(id);

        return CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Identifiers, hash.ToHashCode());
    }

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
    /// cache consistency in CQRS architectures when dealing with batch operations.
    /// </para>
    /// <para>
    /// The tag is generated using the <see cref="CacheTagger"/> utility and is based on the
    /// <typeparamref name="TReadModel"/> type, allowing all queries for that entity type (including
    /// single identifier queries, batch queries, paged queries, etc.) to be grouped and invalidated together when necessary.
    /// </para>
    /// <para>
    /// When any entity of type <typeparamref name="TReadModel"/> is created, updated, or deleted, the associated
    /// cache tag can be used to invalidate all related cached queries, including batch queries, ensuring that
    /// subsequent queries return fresh data.
    /// </para>
    /// </remarks>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
