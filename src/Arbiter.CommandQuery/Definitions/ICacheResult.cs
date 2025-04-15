namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> for cache expiration and cache tagging
/// </summary>
public interface ICacheResult
{
    /// <summary>
    /// Determines whether this entity is cacheable.
    /// </summary>
    /// <returns>
    ///   <see langword="true"/> if this entity is cacheable; otherwise, <see langword="false"/>.
    /// </returns>
    bool IsCacheable();


    /// <summary>
    /// Gets the cache key for this entity.
    /// </summary>
    /// <returns>The cache key for this entity</returns>
    string GetCacheKey();

    /// <summary>
    /// Gets the cache tag for this entity.
    /// </summary>
    /// <returns>The cache tag for this entity</returns>
    string? GetCacheTag();


    /// <summary>
    /// Gets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
    /// </summary>
    /// <returns>The sliding expiration <see cref="TimeSpan"/> if set; otherwise <see langword="null"/>.</returns>
    TimeSpan? SlidingExpiration();

    /// <summary>
    /// Gets an absolute expiration date for the cache entry.
    /// </summary>
    /// <returns>The absolute expiration <see cref="DateTimeOffset"/> if set; otherwise <see langword="null"/>.</returns>
    DateTimeOffset? AbsoluteExpiration();
}
