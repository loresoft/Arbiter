using System.Text.Json;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus.Cache;

/// <summary>
/// Processes <see cref="CacheExpireMessage"/> messages from a Service Bus topic subscription and expires the
/// matching entries from the local <see cref="HybridCache"/>.
/// </summary>
public sealed partial class CacheExpireProcessor : ServiceBusProcessorBase
{
    private readonly HybridCache _hybridCache;
    private readonly CacheExpireOptions _options;
    private readonly ILogger<CacheExpireProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheExpireProcessor"/> class.
    /// </summary>
    /// <param name="processor">The Service Bus processor used to receive messages.</param>
    /// <param name="logger">The logger used to write processing messages.</param>
    /// <param name="hybridCache">The hybrid cache to expire.</param>
    /// <param name="options">The cache expiration options.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="hybridCache"/> or <paramref name="options"/> is null.</exception>
    public CacheExpireProcessor(
        ServiceBusProcessor processor,
        ILogger<CacheExpireProcessor> logger,
        HybridCache hybridCache,
        CacheExpireOptions options)
        : base(processor, logger)
    {
        ArgumentNullException.ThrowIfNull(hybridCache);
        ArgumentNullException.ThrowIfNull(options);

        _hybridCache = hybridCache;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Message settlement is handled automatically by the processor (<see cref="ServiceBusProcessorOptions.AutoCompleteMessages" />);
    /// returning successfully completes the message, while throwing abandons it for redelivery. A message with an
    /// unparsable body is non-retryable and is dead-lettered explicitly rather than retried until the delivery count is exhausted.
    /// </remarks>
    protected override async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        CacheExpireMessage? message;

        try
        {
            message = args.ReadFromJson<CacheExpireMessage>();
        }
        catch (JsonException ex)
        {
            // non-retryable: a malformed body never succeeds, so dead-letter immediately instead of
            // abandoning until MaxDeliveryCount is reached
            LogDeadLetteringMessage(_logger, ex, args.Message.MessageId);

            await args.DeadLetterMessageAsync(
                args.Message,
                deadLetterReason: "DeserializationFailed",
                deadLetterErrorDescription: ex.Message,
                args.CancellationToken).ConfigureAwait(false);

            return;
        }

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
            await _hybridCache.RemoveAsync(message.Key, args.CancellationToken).ConfigureAwait(false);

        foreach (var tag in message.Tags)
        {
            if (!string.IsNullOrEmpty(tag))
                await _hybridCache.RemoveByTagAsync(tag, args.CancellationToken).ConfigureAwait(false);
        }

        LogExpiredCache(_logger, message.Key, message.Tags.Count);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Skipping cache expiration message from own source '{SourceId}'")]
    private static partial void LogSkippingOwnMessage(ILogger logger, string sourceId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dead-lettering cache expiration message '{MessageId}' with unparsable body")]
    private static partial void LogDeadLetteringMessage(ILogger logger, Exception exception, string messageId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Expired local cache for key '{Key}' and {TagCount} tag(s)")]
    private static partial void LogExpiredCache(ILogger logger, string? key, int tagCount);
}
