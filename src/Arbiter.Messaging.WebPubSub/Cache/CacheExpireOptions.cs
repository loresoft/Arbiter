namespace Arbiter.Messaging.WebPubSub.Cache;

/// <summary>
/// Options that control distributed cache expiration over Azure Web PubSub.
/// </summary>
public sealed class CacheExpireOptions
{
    /// <summary>
    /// Gets or sets the name used to resolve the registered Web PubSub clients and options.
    /// </summary>
    public object? ServiceKey { get; set; }

    /// <summary>
    /// Gets or sets the hub name used to publish and receive cache expiration messages.
    /// </summary>
    /// <remarks>
    /// This is the declared hub name used as a stable service key; the actual hub name is computed
    /// from <see cref="WebPubSubOptions.HubSuffix" />.
    /// </remarks>
    public required string HubName { get; set; }

    /// <summary>
    /// Gets or sets the group name used to publish and receive cache expiration messages.
    /// </summary>
    /// <remarks>
    /// This is the declared group name; the actual group name is computed from
    /// <see cref="WebPubSubOptions.GroupSuffix" />.
    /// </remarks>
    public required string GroupName { get; set; }

    /// <summary>
    /// Gets or sets the identifier of this application instance.
    /// </summary>
    /// <remarks>
    /// Stamped on outgoing messages and compared on incoming messages so an application does not redundantly
    /// expire cache entries it already expired locally. Defaults to a unique value per process.
    /// </remarks>
    public string SourceId { get; set; } = Guid.NewGuid().ToString("N");
}
