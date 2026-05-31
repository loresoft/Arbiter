namespace Arbiter.Messaging.ServiceBus.Cache;

/// <summary>
/// Options that control distributed cache expiration over Azure Service Bus.
/// </summary>
public sealed class CacheExpireOptions
{
    /// <summary>
    /// Gets or sets the name of the Service Bus topic used to publish and receive cache expiration messages.
    /// </summary>
    public required string TopicName { get; set; }

    /// <summary>
    /// Gets or sets the identifier of this application instance.
    /// </summary>
    /// <remarks>
    /// Stamped on outgoing messages and compared on incoming messages so an application does not redundantly
    /// expire cache entries it already expired locally. Defaults to a unique value per process.
    /// </remarks>
    public string SourceId { get; set; } = Guid.NewGuid().ToString("N");
}
