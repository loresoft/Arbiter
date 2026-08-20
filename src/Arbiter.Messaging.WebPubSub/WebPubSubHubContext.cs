using Azure.Messaging.WebPubSub;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Represents the resolved configuration for an Azure Web PubSub hub.
/// </summary>
/// <param name="ServiceClient">The service client used to communicate with the hub.</param>
/// <param name="Options">The configured Web PubSub options.</param>
/// <param name="HubName">The formatted hub name.</param>
/// <param name="Groups">The group names declared for the hub mapped to their formatted group names.</param>
public sealed record WebPubSubHubContext(
    WebPubSubServiceClient ServiceClient,
    WebPubSubOptions Options,
    string HubName,
    IReadOnlyDictionary<string, string> Groups
)
{
    /// <summary>
    /// Gets the formatted group name for a group declared for the hub.
    /// </summary>
    /// <param name="groupName">The group name declared for the hub.</param>
    /// <returns>The formatted group name.</returns>
    public string GetGroupName(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        return Groups[groupName];
    }
}
