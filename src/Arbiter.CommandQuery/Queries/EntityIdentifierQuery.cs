using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A query for a single identifier.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TReadModel">The type of the read model</typeparam>
public record EntityIdentifierQuery<TKey, TReadModel> : CacheableQueryBase<TReadModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifierQuery{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">the <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="id">The identifier for this query.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="id"/> is null</exception>
    public EntityIdentifierQuery(ClaimsPrincipal? principal, [NotNull] TKey id)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }

    /// <summary>
    /// Gets the identifier for this query.
    /// </summary>
    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }


    /// <inheritdoc/>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, TKey>(CacheTagger.Buckets.Identifier, Id);

    /// <inheritdoc/>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
