using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a cacheable query that retrieves an entity by its globally unique alternate key.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model to retrieve.</typeparam>
/// <remarks>
/// <para>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve a single entity
/// based on a globally unique alternate key (<see cref="Guid"/>), independent of the primary key. This is particularly
/// useful in distributed systems or when exposing entities through public APIs where primary keys should not be exposed.
/// </para>
/// <para>
/// The query inherits from <see cref="CacheableQueryBase{TResponse}"/> to support caching with configurable expiration policies,
/// which can significantly improve performance for frequently accessed entities.
/// </para>
/// <para>
/// Alternate keys provide several advantages:
/// <list type="bullet">
/// <item><description>Stable identifiers that don't change even if the entity is migrated or the primary key changes</description></item>
/// <item><description>Globally unique identifiers suitable for distributed systems and API exposure</description></item>
/// <item><description>Better security by hiding internal database primary key values</description></item>
/// <item><description>Improved interoperability when integrating with external systems</description></item>
/// </list>
/// </para>
/// <para>
/// The query supports optional named filter pipelines through the <see cref="FilterName"/> property, allowing different
/// query modification strategies to be applied based on the execution context.
/// </para>
/// </remarks>
/// <example>
/// Example demonstrating retrieval of an entity by its alternate key:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var alternateKey = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
/// var query = new EntityKeyQuery&lt;ProductReadModel&gt;(principal, alternateKey);
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
///     Console.WriteLine($"Product Key: {result.Key}");
/// }
/// </code>
/// </example>
/// <example>
/// Example using a named filter pipeline for public API access:
/// <code>
/// var publicKey = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
/// var query = new EntityKeyQuery&lt;ProductReadModel&gt;(
///     null, // Anonymous access
///     publicKey,
///     filterName: "public-api");
///
/// // This might exclude sensitive fields or apply rate limiting
/// var result = await mediator.Send(query);
/// </code>
/// </example>
/// <seealso cref="CacheableQueryBase{TResponse}"/>
/// <seealso cref="EntityIdentifierQuery{TKey, TReadModel}"/>
/// <seealso cref="EntityPagedQuery{TReadModel}"/>
public record EntityKeyQuery<TReadModel> : CacheableQueryBase<TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityKeyQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query. Used for authorization and audit logging. Can be <see langword="null"/> for anonymous access scenarios.</param>
    /// <param name="key">The globally unique alternate key (<see cref="Guid"/>) of the entity to retrieve.</param>
    /// <param name="filterName">Optional name of a specific filter pipeline to apply during query execution. This allows different query modification strategies to be applied based on context.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="principal"/> parameter is used for authorization checks to ensure the user has permission
    /// to access the requested entity by its alternate key. If <see langword="null"/>, the operation is considered
    /// either a system-level query or an anonymous access scenario (e.g., public API endpoints).
    /// </para>
    /// <para>
    /// The <paramref name="key"/> parameter is a globally unique identifier that serves as an alternate way to
    /// identify the entity, independent of the primary key. This is commonly used in scenarios where:
    /// <list type="bullet">
    /// <item><description>The entity needs a stable, public identifier for external API consumption</description></item>
    /// <item><description>The primary key should remain hidden for security reasons</description></item>
    /// <item><description>The entity participates in distributed systems requiring globally unique identifiers</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The <paramref name="filterName"/> parameter enables named query pipeline scenarios, such as:
    /// <list type="bullet">
    /// <item><description>Public API views with limited field visibility and restricted data access</description></item>
    /// <item><description>Partner integration views with specific data transformations</description></item>
    /// <item><description>Administrative views that include additional metadata or system fields</description></item>
    /// <item><description>Different security policies based on access context (internal vs. external)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public EntityKeyQuery(ClaimsPrincipal? principal, Guid key, string? filterName = null)
        : base(principal)
    {
        Key = key;
        FilterName = filterName;
    }

    /// <summary>
    /// Gets the globally unique alternate key of the entity to retrieve.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> representing the globally unique alternate key of the entity. This value is guaranteed to be set after construction.
    /// </value>
    /// <remarks>
    /// <para>
    /// This alternate key is independent of the entity's primary key and provides a stable, globally unique identifier
    /// that can be safely exposed in public APIs and used across distributed systems.
    /// </para>
    /// <para>
    /// The key is incorporated into the cache key generation to ensure that each unique entity is cached separately.
    /// </para>
    /// </remarks>
    [NotNull]
    [JsonPropertyName("key")]
    public Guid Key { get; }

    /// <summary>
    /// Gets the optional name of a specific filter pipeline to apply during query execution.
    /// </summary>
    /// <value>
    /// A string representing the name of the filter pipeline, or <see langword="null"/> if the default query pipeline should be used.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property allows for named query modification strategies to be applied during entity retrieval by alternate key,
    /// enabling different filtering, security policies, or data transformations based on the execution context.
    /// </para>
    /// <para>
    /// The specific behavior depends on the registered query pipeline modifiers in the application.
    /// Common scenarios include applying different field projections, security filters, or data enrichment
    /// strategies based on the pipeline name, particularly useful for public API vs. internal access scenarios.
    /// </para>
    /// <para>
    /// When <see langword="null"/>, the default query pipeline (if any) will be applied.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Using a named pipeline for public API access with restricted fields
    /// var query = new EntityKeyQuery&lt;ProductReadModel&gt;(
    ///     publicPrincipal,
    ///     productKey,
    ///     filterName: "public-api");
    /// </code>
    /// </example>
    [JsonPropertyName("filterName")]
    public string? FilterName { get; }


    /// <summary>
    /// Generates a cache key for the query based on the alternate key.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query, derived from the <typeparamref name="TReadModel"/> type and the <see cref="Key"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The cache key is generated using the <see cref="CacheTagger"/> utility with the <see cref="CacheTagger.Buckets.Key"/> bucket,
    /// ensuring that alternate key-based queries are properly isolated in the cache namespace from other query types
    /// (such as primary key-based queries or paged queries).
    /// </para>
    /// <para>
    /// The key includes the <see cref="Key"/> value (the alternate GUID), which uniquely identifies the entity instance being queried.
    /// This ensures that different entities of the same type are cached separately, even when accessed via their alternate keys.
    /// </para>
    /// <para>
    /// The cache key format allows for efficient lookup and invalidation of cached entity instances retrieved by alternate key.
    /// </para>
    /// </remarks>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, Guid>(CacheTagger.Buckets.Key, Key);

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
    /// primary key queries, alternate key queries, paged queries, etc.) to be grouped and invalidated together when necessary.
    /// </para>
    /// <para>
    /// When an entity is created, updated, or deleted, the associated cache tag can be used to invalidate
    /// all related cached queries, including those retrieved by alternate key, ensuring that subsequent queries return fresh data.
    /// </para>
    /// </remarks>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
