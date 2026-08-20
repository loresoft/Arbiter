using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub.Cache;

/// <summary>
/// Publishes cache expiration messages to an Azure Web PubSub group.
/// </summary>
/// <remarks>
/// This publisher emits distributed invalidation messages so other application instances can evict matching
/// entries from their local <c>HybridCache</c>.
/// <para>
/// The target group is configured by <see cref="CacheExpireOptions.GroupName" /> and resolved through
/// <see cref="WebPubSubHubContext"/> group mappings before publish.
/// </para>
/// <para>
/// Published payloads typically include a cache key and optional tags, along with a source identifier used by
/// receivers to ignore messages originating from the same instance.
/// </para>
/// </remarks>
public sealed partial class CacheExpirePublisher : WebPubSubPublisherBase
{
    private readonly CacheExpireOptions _options;
    private readonly ILogger<CacheExpirePublisher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheExpirePublisher"/> class.
    /// </summary>
    /// <param name="context">The hub context that contains the formatted hub name, group mappings, and service client.</param>
    /// <param name="options">The cache expiration options that provide the target group name.</param>
    /// <param name="logger">The logger used to write publishing messages.</param>
    public CacheExpirePublisher(
        WebPubSubHubContext context,
        CacheExpireOptions options,
        ILogger<CacheExpirePublisher> logger)
        : base(context)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Publishes a cache expiration message to the configured Web PubSub group.
    /// </summary>
    /// <param name="message">The cache expiration payload containing the key and optional tags to invalidate.</param>
    /// <param name="cancellationToken">A token used to cancel the publish operation.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    public Task PublishAsync(CacheExpireMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var groupName = GetGroupName(_options.GroupName);

        LogPublishingCacheExpiration(
            _logger,
            message.Key,
            message.Tags?.Count ?? 0,
            HubName,
            groupName);

        return SendToGroupAsJsonAsync(
            _options.GroupName,
            message,
            CacheExpireSerializerContext.Default.CacheExpireMessage,
            cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Publishing cache expiration for key '{Key}' and {TagCount} tag(s) to Web PubSub hub '{HubName}' group '{GroupName}'")]
    private static partial void LogPublishingCacheExpiration(
        ILogger logger,
        string? key,
        int tagCount,
        string hubName,
        string groupName);
}
