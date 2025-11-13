using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a query for retrieving a single entity identified by a specific key.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the query.</typeparam>
/// <remarks>
/// <para>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve a single entity
/// based on its unique identifier. It inherits from <see cref="CacheableQueryBase{TResponse}"/> to support caching,
/// which can significantly improve performance for frequently accessed entities.
/// </para>
/// <para>
/// The query supports optional named filter pipelines through the <see cref="FilterName"/> property, allowing different
/// query modification strategies to be applied based on the execution context, such as security policies or data transformations.
/// </para>
/// <para>
/// Cache keys are generated based on the entity identifier, ensuring that each unique entity is cached separately.
/// Cache tags enable efficient cache invalidation when the underlying entity data changes.
/// </para>
/// </remarks>
/// <example>
/// Example demonstrating a simple entity lookup by identifier:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var query = new EntityIdentifierQuery&lt;int, ProductReadModel&gt;(principal, 123);
///
/// // Optionally configure caching
/// query.Cache(TimeSpan.FromMinutes(10));
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
/// 
/// if (result != null)
/// {
///     Console.WriteLine($"Product Name: {result.Name}");
///     Console.WriteLine($"Price: {result.Price:C}");
/// }
/// </code>
/// </example>
/// <example>
/// Example using a named filter pipeline for administrative access:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }));
/// var query = new EntityIdentifierQuery&lt;int, ProductReadModel&gt;(
///     principal, 
///     456, 
///     filterName: "admin-view");
///
/// // This might include additional fields or bypass certain security filters
/// var result = await mediator.Send(query);
/// </code>
/// </example>
/// <seealso cref="CacheableQueryBase{TResponse}"/>
/// <seealso cref="EntityIdentifiersQuery{TKey, TReadModel}"/>
/// <seealso cref="EntityPagedQuery{TReadModel}"/>
public record EntityIdentifierQuery<TKey, TReadModel> : CacheableQueryBase<TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifierQuery{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query. Used for authorization and audit logging.</param>
    /// <param name="id">The identifier of the entity to retrieve. This value cannot be <see langword="null"/>.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows different query modification strategies to be applied based on context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="principal"/> parameter is used for authorization checks to ensure the user has permission
    /// to access the requested entity. If <see langword="null"/>, the operation is considered a system-level query.
    /// </para>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables named query pipeline scenarios, such as:
    /// <list type="bullet">
    /// <item><description>Administrative views that include additional fields or bypass certain filters</description></item>
    /// <item><description>Public views with restricted data visibility</description></item>
    /// <item><description>Audit views with enhanced logging and tracking</description></item>
    /// <item><description>Different security policies based on user roles or context</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public EntityIdentifierQuery(ClaimsPrincipal? principal, [NotNull] TKey id, string? filterName = null)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        FilterName = filterName;
    }

    /// <summary>
    /// Gets the identifier of the entity to retrieve.
    /// </summary>
    /// <value>
    /// The unique identifier of the entity. This value is guaranteed to be non-null after construction.
    /// </value>
    /// <remarks>
    /// This identifier is used to locate the specific entity instance and is also incorporated into the cache key
    /// to ensure that each entity is cached independently.
    /// </remarks>
    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during query execution.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if the default query pipeline should be used.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows for named query modification strategies to be applied during entity retrieval,
    /// enabling different filtering, security policies, or data transformations based on the execution context.
    /// </para>
    /// <para>
    /// The specific behavior depends on the registered query pipeline modifiers in the application.
    /// Common scenarios include applying different field projections, security filters, or data enrichment
    /// strategies based on the pipeline name.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the default query pipeline (if any) will be applied.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using a named pipeline for detailed view
    /// var query = new EntityIdentifierQuery&lt;int, ProductReadModel&gt;(
    ///     userPrincipal, 
    ///     productId, 
    ///     filterName: "detailed-view");
    /// </code>
    /// </example>
    [JsonPropertyName("filterName")]
    public string? FilterName { get; }

    /// <summary>
    /// Generates a cache key for the query based on the entity identifier.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query, combining the entity type and the specific identifier.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The cache key is generated using the <see cref="CacheTagger"/> utility with the <see cref="CacheTagger.Buckets.Identifier"/> bucket,
    /// ensuring that single entity queries are properly isolated in the cache namespace from other query types (such as paged queries).
    /// </para>
    /// <para>
    /// The key includes the <see cref="Id"/> value, which uniquely identifies the entity instance being queried.
    /// This ensures that different entities of the same type are cached separately.
    /// </para>
    /// <para>
    /// The cache key format allows for efficient lookup and invalidation of cached entity instances.
    /// </para>
    /// </remarks>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, TKey>(CacheTagger.Buckets.Identifier, Id);

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
    /// <typeparamref name="TReadModel"/> type, allowing all queries for that entity type (including
    /// identifier queries, paged queries, etc.) to be grouped and invalidated together when necessary.
    /// </para>
    /// <para>
    /// When an entity is created, updated, or deleted, the associated cache tag can be used to invalidate
    /// all related cached queries, ensuring that subsequent queries return fresh data.
    /// </para>
    /// </remarks>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
