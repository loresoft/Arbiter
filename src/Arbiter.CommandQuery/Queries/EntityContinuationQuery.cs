using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Services;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A query for paging entities by continuation token based on an <see cref="EntitySelect"/>.
/// </summary>
/// <typeparam name="TReadModel">The type of the read model.</typeparam>
public record EntityContinuationQuery<TReadModel> : CacheableQueryBase<EntityContinuationResult<TReadModel>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityContinuationQuery{TReadModel}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    /// <param name="query">The <see cref="EntitySelect"/> for this query</param>
    /// <param name="pageSize">The page size for this query</param>
    /// <param name="continuationToken">The continuation token for this query</param>
    public EntityContinuationQuery(ClaimsPrincipal? principal, EntitySelect? query, int pageSize = 10, string? continuationToken = null)
        : base(principal)
    {
        Query = query ?? new EntitySelect();
        PageSize = pageSize;
        ContinuationToken = continuationToken;
    }

    /// <summary>
    /// The <see cref="EntitySelect"/> for this query.
    /// </summary>
    [JsonPropertyName("query")]
    public EntitySelect Query { get; }

    /// <summary>
    /// The page size for this query.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; }

    /// <summary>
    /// The continuation token for this query.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; }

    /// <inheritdoc/>
    public override string GetCacheKey()
        => CacheTagger.GetKey<TReadModel, int>
        (
            bucket: CacheTagger.Buckets.Continuation,
            value: HashCode.Combine(Query.GetHashCode(), PageSize, ContinuationToken)
        );

    /// <inheritdoc/>
    public override string? GetCacheTag()
        => CacheTagger.GetTag<TReadModel>();
}
