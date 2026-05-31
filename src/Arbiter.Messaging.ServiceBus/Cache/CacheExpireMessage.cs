using System.Text.Json.Serialization;

namespace Arbiter.Messaging.ServiceBus.Cache;

/// <summary>
/// A message describing a cache entry to expire across applications subscribed to a Service Bus topic.
/// </summary>
/// <remarks>
/// The message is published when a command expires the local cache and is received by other applications so they can
/// remove the matching key and tags from their own <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>.
/// </remarks>
public sealed record CacheExpireMessage
{
    /// <summary>
    /// Gets the cache key to remove, or <see langword="null"/> when no specific key should be removed.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>
    /// Gets the cache tags to remove. Empty when no tags should be removed.
    /// </summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the identifier of the application instance that published this message.
    /// </summary>
    /// <remarks>
    /// Used by receivers to skip expiration for messages they originally published, since the publishing
    /// application has already expired its own local cache.
    /// </remarks>
    [JsonPropertyName("sourceId")]
    public string? SourceId { get; init; }
}
