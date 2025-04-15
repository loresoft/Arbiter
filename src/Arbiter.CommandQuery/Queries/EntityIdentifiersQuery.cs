using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A query for a list of identifiers.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TReadModel">The type of the read model</typeparam>
public record EntityIdentifiersQuery<TKey, TReadModel> : CacheableQueryBase<IReadOnlyCollection<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIdentifiersQuery{TKey, TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">the <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="ids">The list of identifiers for this query.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="ids"/> is null</exception>
    public EntityIdentifiersQuery(ClaimsPrincipal? principal, [NotNull] IReadOnlyCollection<TKey> ids)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(ids);

        Ids = ids;
    }

    /// <summary>
    /// Gets the list of identifiers for this query.
    /// </summary>
    /// <value>
    /// The list of identifiers for this query.
    /// </value>
    [NotNull]
    [JsonPropertyName("ids")]
    public IReadOnlyCollection<TKey> Ids { get; }

    /// <inheritdoc/>
    public override string GetCacheKey()
    {
        var hash = new HashCode();

        foreach (var id in Ids)
            hash.Add(id);

        return CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Identifiers, hash.ToHashCode());
    }

    /// <inheritdoc/>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
