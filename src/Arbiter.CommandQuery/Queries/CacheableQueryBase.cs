using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.Queries;

/// <summary>
/// Represents a base class for cacheable queries that use a specified <see cref="ClaimsPrincipal"/> for user context.
/// </summary>
/// <typeparam name="TResponse">The type of the response returned by the query.</typeparam>
/// <remarks>
/// This class provides support for cache key generation, cache tagging, and cache expiration policies.
/// It is intended for use in scenarios where query results can be cached and associated with a user principal.
/// </remarks>
public abstract record CacheableQueryBase<TResponse> : PrincipalQueryBase<TResponse>, ICacheResult
{
    private DateTimeOffset? _absoluteExpiration;
    private TimeSpan? _slidingExpiration;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheableQueryBase{TResponse}"/> class.
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user for whom the query is executed.</param>
    protected CacheableQueryBase(ClaimsPrincipal? principal) : base(principal)
    {
    }

    /// <summary>
    /// Gets the cache key for this query instance.
    /// </summary>
    /// <returns>A string representing the cache key.</returns>
    public abstract string GetCacheKey();

    /// <summary>
    /// Gets the cache tag for this query instance.
    /// </summary>
    /// <returns>A string representing the cache tag, or <c>null</c> if not set.</returns>
    public virtual string? GetCacheTag() => null;

    /// <summary>
    /// Determines whether this query is cacheable based on the expiration settings.
    /// </summary>
    /// <returns>
    ///   <see langword="true"/> if either absolute or sliding expiration is set; otherwise, <see langword="false"/>.
    /// </returns>
    public bool IsCacheable()
    {
        return _absoluteExpiration.HasValue
            || _slidingExpiration.HasValue;
    }

    /// <summary>
    /// Sets the absolute expiration for the cache entry associated with this query.
    /// </summary>
    /// <param name="absoluteExpiration">The absolute expiration date and time, or <c>null</c> to unset.</param>
    public void Cache(DateTimeOffset? absoluteExpiration)
    {
        _absoluteExpiration = absoluteExpiration;
    }

    /// <summary>
    /// Sets the sliding expiration for the cache entry associated with this query.
    /// </summary>
    /// <param name="expiration">The sliding expiration time span, or <c>null</c> to unset.</param>
    public void Cache(TimeSpan? expiration)
    {
        _slidingExpiration = expiration;
    }

    /// <summary>
    /// Gets the absolute expiration date and time for the cache entry.
    /// </summary>
    /// <returns>
    /// The absolute expiration as a <see cref="DateTimeOffset"/> if set; otherwise, <c>null</c>.
    /// </returns>
    DateTimeOffset? ICacheResult.AbsoluteExpiration()
    {
        return _absoluteExpiration;
    }

    /// <summary>
    /// Gets the sliding expiration time span for the cache entry.
    /// </summary>
    /// <returns>
    /// The sliding expiration as a <see cref="TimeSpan"/> if set; otherwise, <c>null</c>.
    /// </returns>
    TimeSpan? ICacheResult.SlidingExpiration()
    {
        return _slidingExpiration;
    }
}
