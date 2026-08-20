namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides a builder for declaring the Azure Web PubSub hubs, and the groups associated with them,
/// to register service clients for.
/// </summary>
public sealed class WebPubSubBuilder
{
    private readonly WebPubSubOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPubSubBuilder" /> class.
    /// </summary>
    /// <param name="options">The options instance the builder configures.</param>
    public WebPubSubBuilder(WebPubSubOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <summary>
    /// Adds a hub to register a service client for.
    /// </summary>
    /// <param name="hubName">The hub name.</param>
    /// <returns>The current <see cref="WebPubSubBuilder" /> instance for chaining.</returns>
    public WebPubSubBuilder AddHub(string hubName)
    {
        _options.AddHub(hubName);
        return this;
    }

    /// <summary>
    /// Adds a hub to register a service client for, along with the groups listeners join for that hub.
    /// </summary>
    /// <param name="hubName">The hub name.</param>
    /// <param name="groups">The group names associated with the hub.</param>
    /// <returns>The current <see cref="WebPubSubBuilder" /> instance for chaining.</returns>
    public WebPubSubBuilder AddHub(string hubName, params IEnumerable<string> groups)
    {
        _options.AddHub(hubName, groups);
        return this;
    }
}
