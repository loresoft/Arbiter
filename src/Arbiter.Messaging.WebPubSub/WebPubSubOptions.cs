using Azure.Core;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Represents configuration options for Azure Web PubSub clients, hubs, and groups.
/// </summary>
public sealed class WebPubSubOptions
{
    /// <summary>
    /// Gets or sets the dependency injection key used to register the Web PubSub clients.
    /// </summary>
    public object? ServiceKey { get; set; }

    /// <summary>
    /// Gets or sets a Web PubSub connection string, connection string name, or configuration key.
    /// When <see cref="Credential" /> is provided, this value is the service endpoint
    /// (for example, <c>https://my-service.webpubsub.azure.com</c>).
    /// </summary>
    public string NameOrConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="TokenCredential" /> used for identity-based authentication.
    /// When set, <see cref="NameOrConnectionString" /> is treated as the service endpoint.
    /// When <see langword="null" />, connection-string based authentication is used.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the suffix appended to hub names when formatting hub names.
    /// </summary>
    public string? HubSuffix { get; set; }

    /// <summary>
    /// Gets or sets the suffix appended to group names when formatting group names.
    /// </summary>
    /// <remarks>
    /// This suffix is independent of <see cref="HubSuffix" />. When <see langword="null" /> or blank,
    /// group names are used as-is.
    /// </remarks>
    public string? GroupSuffix { get; set; }


    /// <summary>
    /// Gets or sets the hub names to register service clients for.
    /// </summary>
    public IList<string> Hubs { get; set; } = [];

    /// <summary>
    /// Gets or sets group names grouped by hub name.
    /// </summary>
    /// <remarks>
    /// Groups are not provisioned; Azure Web PubSub creates them implicitly when a connection joins.
    /// The declared names are used to join groups when a listener starts.
    /// </remarks>
    public IDictionary<string, IList<string>> Groups { get; set; } = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Gets or sets the lifetime of generated client access tokens.
    /// </summary>
    /// <remarks>
    /// A new token is requested on every connection attempt, including reconnects, so this value only
    /// needs to exceed the time required to establish a connection.
    /// </remarks>
    public TimeSpan ClientAccessTokenExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets a value indicating whether listener clients automatically reconnect when a connection drops.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether listener clients automatically rejoin their groups after reconnecting.
    /// </summary>
    public bool AutoRejoinGroups { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether listener clients use the reliable JSON subprotocol.
    /// </summary>
    /// <remarks>
    /// The reliable protocol adds sequence acknowledgement and message recovery across brief reconnects.
    /// It does not provide durability; messages published while a client is disconnected are never delivered.
    /// </remarks>
    public bool UseReliableProtocol { get; set; } = true;


    /// <summary>
    /// Sets the suffix appended to hub names when formatting hub names.
    /// </summary>
    /// <param name="nameSuffix">The name suffix to append.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions WithHubSuffix(string? nameSuffix)
    {
        HubSuffix = nameSuffix;
        return this;
    }

    /// <summary>
    /// Sets the suffix appended to group names when formatting group names.
    /// </summary>
    /// <param name="groupSuffix">The group name suffix to append.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions WithGroupSuffix(string? groupSuffix)
    {
        GroupSuffix = groupSuffix;
        return this;
    }

    /// <summary>
    /// Sets the lifetime of generated client access tokens.
    /// </summary>
    /// <param name="expiration">The token lifetime. Must be greater than zero.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions WithClientAccessTokenExpiration(TimeSpan expiration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expiration, TimeSpan.Zero);

        ClientAccessTokenExpiration = expiration;
        return this;
    }

    /// <summary>
    /// Sets whether listener clients automatically reconnect when a connection drops.
    /// </summary>
    /// <param name="autoReconnect">A value indicating whether automatic reconnect is enabled.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions WithAutoReconnect(bool autoReconnect = true)
    {
        AutoReconnect = autoReconnect;
        return this;
    }

    /// <summary>
    /// Sets whether listener clients automatically rejoin their groups after reconnecting.
    /// </summary>
    /// <param name="autoRejoinGroups">A value indicating whether automatic group rejoin is enabled.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions WithAutoRejoinGroups(bool autoRejoinGroups = true)
    {
        AutoRejoinGroups = autoRejoinGroups;
        return this;
    }

    /// <summary>
    /// Sets whether listener clients use the reliable JSON subprotocol.
    /// </summary>
    /// <param name="useReliableProtocol">A value indicating whether the reliable protocol is used.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions WithReliableProtocol(bool useReliableProtocol = true)
    {
        UseReliableProtocol = useReliableProtocol;
        return this;
    }


    /// <summary>
    /// Adds a hub to register a service client for.
    /// </summary>
    /// <param name="hubName">The hub name.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions AddHub(string hubName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);

        Hubs.Add(hubName);
        return this;
    }

    /// <summary>
    /// Adds a hub to register a service client for, along with the groups listeners join for that hub.
    /// </summary>
    /// <param name="hubName">The hub name.</param>
    /// <param name="groups">The group names associated with the hub.</param>
    /// <returns>The current <see cref="WebPubSubOptions" /> instance for chaining.</returns>
    public WebPubSubOptions AddHub(string hubName, params IEnumerable<string> groups)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);
        ArgumentNullException.ThrowIfNull(groups);

        Hubs.Add(hubName);
        Groups[hubName] = [.. groups];

        return this;
    }


    /// <summary>
    /// Formats a hub name by appending the configured suffix when one is provided.
    /// </summary>
    /// <param name="baseName">The base hub name.</param>
    /// <returns>The formatted hub name.</returns>
    public string FormatHub(string baseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);

        return string.IsNullOrWhiteSpace(HubSuffix) ? baseName : $"{baseName}_{HubSuffix}";
    }

    /// <summary>
    /// Formats a group name by appending the configured group suffix when one is provided.
    /// </summary>
    /// <param name="baseName">The base group name.</param>
    /// <returns>The formatted group name.</returns>
    public string FormatGroup(string baseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);

        return string.IsNullOrWhiteSpace(GroupSuffix) ? baseName : $"{baseName}-{GroupSuffix}";
    }
}
