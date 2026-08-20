using System.Text.Json;

using Azure.Messaging.WebPubSub.Clients;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub.Cache;

/// <summary>
/// Processes <see cref="CacheExpireMessage"/> messages from a Web PubSub group and expires the
/// matching entries from the local <see cref="HybridCache"/>.
/// </summary>
/// <remarks>
/// This processor applies distributed invalidation to the local node by removing cache entries identified by
/// the message key and tags.
/// <para>
/// Messages published by the same source identifier configured in <see cref="CacheExpireOptions.SourceId" /> are
/// ignored because local expiration has already been applied by the publisher.
/// </para>
/// <para>
/// Delivery is best-effort through Azure Web PubSub. Messages sent while the processor is disconnected are not
/// replayed, so applications should tolerate temporary cache staleness.
/// </para>
/// </remarks>
public sealed partial class CacheExpireProcessor : WebPubSubProcessorBase
{
    private readonly HybridCache _hybridCache;
    private readonly CacheExpireOptions _options;
    private readonly ILogger<CacheExpireProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheExpireProcessor"/> class.
    /// </summary>
    /// <param name="context">The resolved hub, group, and client configuration for the listener.</param>
    /// <param name="lifetime">The host lifetime used to defer connecting until the application has started.</param>
    /// <param name="logger">The logger used to write processing messages.</param>
    /// <param name="hybridCache">The hybrid cache to expire.</param>
    /// <param name="options">The cache expiration options.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="hybridCache"/> or <paramref name="options"/> is null.</exception>
    public CacheExpireProcessor(
        WebPubSubHubContext context,
        IHostApplicationLifetime lifetime,
        ILogger<CacheExpireProcessor> logger,
        HybridCache hybridCache,
        CacheExpireOptions options)
        : base(context, lifetime, logger)
    {
        ArgumentNullException.ThrowIfNull(hybridCache);
        ArgumentNullException.ThrowIfNull(options);

        _hybridCache = hybridCache;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Web PubSub does not provide a dead-letter queue or redelivery, so a message with an unparsable body is
    /// logged and dropped rather than dead-lettered.
    /// </remarks>
    protected override async Task ProcessGroupMessageAsync(WebPubSubGroupMessageEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        CacheExpireMessage? message;

        try
        {
            message = args.ReadFromJson(CacheExpireSerializerContext.Default.CacheExpireMessage);
        }
        catch (JsonException ex)
        {
            LogDroppingMessage(_logger, ex, args.Message.Group);
            return;
        }

        await ExpireCacheAsync(message, args.CancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Web PubSub does not provide a dead-letter queue or redelivery, so a message with an unparsable body is
    /// logged and dropped rather than dead-lettered.
    /// </remarks>
    protected override async Task ProcessServerMessageAsync(WebPubSubServerMessageEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        CacheExpireMessage? message;

        try
        {
            message = args.ReadFromJson(CacheExpireSerializerContext.Default.CacheExpireMessage);
        }
        catch (JsonException ex)
        {
            LogDroppingMessage(_logger, ex, groupName: null);
            return;
        }

        await ExpireCacheAsync(message, args.CancellationToken).ConfigureAwait(false);
    }


    private async Task ExpireCacheAsync(CacheExpireMessage? message, CancellationToken cancellationToken)
    {
        if (message is null)
            return;

        // skip messages this application published; it already expired its own local cache
        if (!string.IsNullOrEmpty(message.SourceId)
            && string.Equals(message.SourceId, _options.SourceId, StringComparison.Ordinal))
        {
            LogSkippingOwnMessage(_logger, message.SourceId);
            return;
        }

        if (!string.IsNullOrEmpty(message.Key))
        {
            await _hybridCache.RemoveAsync(message.Key, cancellationToken).ConfigureAwait(false);
        }

        if (message.Tags is not null && message.Tags.Count != 0)
        {
            foreach (var tag in message.Tags)
            {
                if (!string.IsNullOrEmpty(tag))
                    await _hybridCache.RemoveByTagAsync(tag, cancellationToken).ConfigureAwait(false);
            }
        }


        LogExpiredCache(_logger, message.Key, message.Tags?.Count ?? 0);

    }


    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipping cache expiration message from own source '{SourceId}'")]
    private static partial void LogSkippingOwnMessage(ILogger logger, string sourceId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dropping cache expiration message from group '{GroupName}' with unparsable body")]
    private static partial void LogDroppingMessage(ILogger logger, Exception exception, string? groupName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Expired local cache for key '{Key}' and {TagCount} tag(s)")]
    private static partial void LogExpiredCache(ILogger logger, string? key, int tagCount);

}
