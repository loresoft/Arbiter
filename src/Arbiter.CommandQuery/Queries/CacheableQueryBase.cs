// Ignore Spelling: Cacheable

using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// A base cacheable query type using the specified <see cref="ClaimsPrincipal"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract record CacheableQueryBase<TResponse> : PrincipalQueryBase<TResponse>, ICacheResult
{
    private DateTimeOffset? _absoluteExpiration;
    private TimeSpan? _slidingExpiration;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheableQueryBase{TResponse}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> this query is run for</param>
    protected CacheableQueryBase(ClaimsPrincipal? principal) : base(principal)
    {
    }


    /// <inheritdoc/>
    public abstract string GetCacheKey();

    /// <inheritdoc/>
    public virtual string? GetCacheTag() => null;

    /// <inheritdoc/>
    public bool IsCacheable()
    {
        return _absoluteExpiration.HasValue
            || _slidingExpiration.HasValue;
    }

    /// <summary>
    /// Sets the cache expiration for this entity.
    /// </summary>
    /// <param name="absoluteExpiration">The absolute expiration</param>
    public void Cache(DateTimeOffset? absoluteExpiration)
    {
        _absoluteExpiration = absoluteExpiration;
    }

    /// <summary>
    /// Sets the cache expiration for this entity.
    /// </summary>
    /// <param name="expiration">The expiration time span</param>
    public void Cache(TimeSpan? expiration)
    {
        _slidingExpiration = expiration;
    }


    /// <inheritdoc/>
    DateTimeOffset? ICacheResult.AbsoluteExpiration()
    {
        return _absoluteExpiration;
    }

    /// <inheritdoc/>
    TimeSpan? ICacheResult.SlidingExpiration()
    {
        return _slidingExpiration;
    }
}
