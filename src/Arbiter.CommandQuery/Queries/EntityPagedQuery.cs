using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A query for paging entities based on an <see cref="EntityQuery"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public record EntityPagedQuery<TReadModel> : CacheableQueryBase<EntityPagedResult<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityPagedQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="query">The <see cref="EntityQuery"/> for this query</param>
    public EntityPagedQuery(ClaimsPrincipal? principal, EntityQuery? query)
        : base(principal)
    {
        Query = query ?? new EntityQuery();
    }

    /// <summary>
    /// The <see cref="EntityQuery"/> for this query.
    /// </summary>
    [JsonPropertyName("query")]
    public EntityQuery Query { get; }


    /// <inheritdoc/>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Paged, Query.GetHashCode());

    /// <inheritdoc/>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
