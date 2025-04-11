using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

public record EntityPagedQuery<TReadModel> : CacheableQueryBase<EntityPagedResult<TReadModel>>
{
    public EntityPagedQuery(ClaimsPrincipal? principal, EntityQuery? query)
        : base(principal)
    {
        Query = query ?? new EntityQuery();
    }

    [JsonPropertyName("query")]
    public EntityQuery Query { get; }


    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>(CacheTagger.Buckets.Paged, Query.GetHashCode());

    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
