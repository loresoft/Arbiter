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
/// This query uses a <see cref="Guid"/> based alternate key to retrieve entities, independent of the primary key.
/// The query supports caching with configurable expiration policies for improved performance.
/// </remarks>
public record EntityKeyQuery<TReadModel> : CacheableQueryBase<TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityKeyQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user executing the query.</param>
    /// <param name="key">The globally unique alternate key of the entity to retrieve.</param>
    public EntityKeyQuery(ClaimsPrincipal? principal, Guid key)
        : base(principal)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the globally unique alternate key of the entity to retrieve.
    /// </summary>
    /// <value>
    /// A <see cref="Guid"/> representing the globally unique alternate key of the entity to retrieve.
    /// </value>
    [NotNull]
    [JsonPropertyName("key")]
    public Guid Key { get; }

    /// <summary>
    /// Generates a cache key for the query based on the alternate key.
    /// </summary>
    /// <returns>
    /// A string representing the cache key for the query, derived from the <typeparamref name="TReadModel"/> type and the <see cref="Key"/>.
    /// </returns>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, Guid>(CacheTagger.Buckets.Key, Key);

    /// <summary>
    /// Gets the cache tag associated with the <typeparamref name="TReadModel"/> type.
    /// </summary>
    /// <returns>
    /// The cache tag for the <typeparamref name="TReadModel"/>, or <see langword="null"/> if no tag is available.
    /// </returns>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
