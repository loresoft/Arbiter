namespace Arbiter.CommandQuery.Definitions;

/// <summary>
/// An <see langword="interface"/> for cache expiration and cache tagging
/// </summary>
public interface ICacheExpire
{
    /// <summary>
    /// Gets the cache tag for this entity.
    /// </summary>
    /// <returns>The cache tag for this entity</returns>
    string? GetCacheTag();
}
