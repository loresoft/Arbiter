using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a query for retrieving a single entity identified by a specific key.
/// The result of the query will be of type <typeparamref name="TReadModel"/>.
/// </summary>
/// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
/// <typeparam name="TReadModel">The type of the read model returned as the result of the query.</typeparam>
/// <remarks>
/// This query is typically used in a CQRS (Command Query Responsibility Segregation) pattern to retrieve a single entity
/// based on its unique identifier. It supports caching to optimize repeated queries for the same entity.
/// </remarks>
/// <example>
/// The following example demonstrates how to use the <see cref="EntityIdentifierQuery{TKey, TReadModel}"/>:
/// <code>
/// var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "JohnDoe") }));
/// var query = new EntityIdentifierQuery&lt;int, ProductReadModel&gt;(principal, 123);
///
/// // Send the query to the mediator instance
/// var result = await mediator.Send(query);
/// Console.WriteLine($"Product Name: {result?.Name}");
/// </code>
/// </example>
public record EntityIdentifierQuery<TKey, TReadModel> : CacheableQueryBase<TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifierQuery{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
    public EntityIdentifierQuery(ClaimsPrincipal? principal, [NotNull] TKey id)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }

    /// <summary>
    /// Gets the identifier of the entity to retrieve.
    /// </summary>
    /// <value>
    /// The identifier of the entity to retrieve.
    /// </value>
    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }

    /// <summary>
    /// Generates a cache key for the query based on the identifier.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query.
    /// </returns>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, TKey>(CacheTagger.Buckets.Identifier, Id);

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/>.
    /// </summary>
    /// <returns>
    /// The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.
    /// </returns>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
