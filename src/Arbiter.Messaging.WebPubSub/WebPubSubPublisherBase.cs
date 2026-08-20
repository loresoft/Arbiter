using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides a base class for publishing messages to the Azure Web PubSub groups declared for a hub by
/// <see cref="WebPubSubExtensions.AddWebPubSub(Microsoft.Extensions.DependencyInjection.IServiceCollection, object, string, System.Action{WebPubSubBuilder}, System.Action{WebPubSubOptionsBuilder})" />.
/// </summary>
/// <remarks>
/// Derive from this class to publish strongly-typed payloads to hub groups configured through
/// <see cref="WebPubSubHubContext" />.
/// <para>
/// Group names passed to publishing methods are logical names declared for the hub and are resolved to the
/// formatted runtime group names before dispatch.
/// </para>
/// <para>
/// This type provides helper overloads for JSON serialization using either source-generated metadata
/// (<see cref="JsonTypeInfo{T}"/>) or <see cref="JsonSerializerOptions" />.
/// </para>
/// </remarks>
public abstract class WebPubSubPublisherBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebPubSubPublisherBase"/> class.
    /// </summary>
    /// <param name="context">The hub context that contains the formatted hub name, group mappings, and service client.</param>
    protected WebPubSubPublisherBase(WebPubSubHubContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        HubContext = context;
    }

    /// <summary>
    /// Gets the formatted hub name.
    /// </summary>
    protected string HubName => HubContext.HubName;

    /// <summary>
    /// Gets the group names declared for the hub mapped to their formatted group names.
    /// </summary>
    protected IReadOnlyDictionary<string, string> Groups => HubContext.Groups;

    /// <summary>
    /// Gets the hub context containing the hub metadata, group mappings, and service client used for publishing.
    /// </summary>
    protected WebPubSubHubContext HubContext { get; }

    /// <summary>
    /// Gets the formatted group name for a group declared for the hub.
    /// </summary>
    /// <param name="groupName">The group name declared for the hub.</param>
    /// <returns>The formatted group name.</returns>
    protected string GetGroupName(string groupName) => HubContext.GetGroupName(groupName);

    /// <summary>
    /// Serializes and publishes a message to the formatted group mapped from a declared hub group name.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="groupName">The declared hub group name to map to a formatted group name.</param>
    /// <param name="message">The message payload to publish.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON metadata that describes how to serialize <paramref name="message"/>.</param>
    /// <param name="cancellationToken">A token used to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    public Task SendToGroupAsJsonAsync<TMessage>(
        string groupName,
        TMessage message,
        JsonTypeInfo<TMessage> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        string group = GetGroupName(groupName);
        return HubContext.ServiceClient.SendToGroupAsJsonAsync(
            group,
            message,
            jsonTypeInfo,
            cancellationToken);
    }

    /// <summary>
    /// Serializes and publishes a message to the formatted group mapped from a declared hub group name.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <param name="groupName">The declared hub group name to map to a formatted group name.</param>
    /// <param name="message">The message payload to publish.</param>
    /// <param name="jsonOptions">The JSON serializer options used to serialize <paramref name="message"/>.</param>
    /// <param name="cancellationToken">A token used to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    public Task SendToGroupAsJsonAsync<TMessage>(
        string groupName,
        TMessage message,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);
        ArgumentNullException.ThrowIfNull(message);

        var group = GetGroupName(groupName);
        return HubContext.ServiceClient.SendToGroupAsJsonAsync(
            group,
            message,
            jsonOptions,
            cancellationToken);
    }

}
