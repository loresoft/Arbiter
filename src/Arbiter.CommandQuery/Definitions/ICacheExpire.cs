namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> for cache expiration and cache tagging
/// </summary>
public interface ICacheExpire
{
    /// <summary>
    /// Gets the cache key for this entity.
    /// </summary>
    /// <returns>The cache key for this entity</returns>
    string? GetCacheKey();

    /// <summary>
    /// Gets the cache tag for this entity.
    /// </summary>
    /// <returns>The cache tag for this entity</returns>
    string? GetCacheTag();

    /// <summary>
    /// Gets the cache tags for this entity.
    /// </summary>
    /// <returns>
    /// The cache tags for this entity. The default implementation returns the single tag from
    /// <see cref="GetCacheTag"/> as a one-item sequence, or an empty sequence when no tag is available.
    /// </returns>
    /// <remarks>
    /// Override this member to expire multiple cache tags for a single change. The default implementation
    /// preserves backward compatibility with <see cref="GetCacheTag"/>, so existing implementers do not
    /// need to change.
    /// </remarks>
    IEnumerable<string> GetCacheTags()
    {
        var tag = GetCacheTag();
        return string.IsNullOrEmpty(tag) ? [] : [tag];
    }
}
