using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

public record EntityIdentifierQuery<TKey, TReadModel> : CacheableQueryBase<TReadModel>
{

    public EntityIdentifierQuery(ClaimsPrincipal? principal, [NotNull] TKey id)
        : base(principal)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
    }

    [NotNull]
    [JsonPropertyName("id")]
    public TKey Id { get; }


    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, TKey>(CacheTagger.Buckets.Identifier, Id);

    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
