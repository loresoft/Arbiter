using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;
using Arbiter.Mediation;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.WebPubSub.Cache;

/// <summary>
/// A behavior that expires the local <see cref="HybridCache"/> and then attempts to publish a
/// <see cref="CacheExpireMessage"/> to a Web PubSub group so other application instances can expire their own caches.
/// </summary>
/// <remarks>
/// This behavior extends <see cref="HybridCacheExpireBehavior{TRequest, TResponse}"/> by adding cross-instance
/// invalidation over Azure Web PubSub after local invalidation succeeds.
/// <para>
/// Publish failures are caught and logged so request processing and local cache expiration are not rolled back.
/// As a result, remote invalidation is best-effort and temporary cache divergence between instances is possible.
/// </para>
/// <para>
/// Outbound messages include the configured <see cref="CacheExpireOptions.SourceId"/> so receivers can ignore
/// self-originated invalidation events.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class WebPubSubCacheExpireBehavior<TRequest, TResponse> : HybridCacheExpireBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ICacheExpire
{
    private readonly CacheExpirePublisher _publisher;
    private readonly CacheExpireOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPubSubCacheExpireBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from.</param>
    /// <param name="hybridCache">The hybrid cache used for local invalidation.</param>
    /// <param name="publisher">The publisher used to send cache expiration messages.</param>
    /// <param name="options">The cache expiration options.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loggerFactory"/>, <paramref name="hybridCache"/>, <paramref name="publisher"/>, or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public WebPubSubCacheExpireBehavior(
        ILoggerFactory loggerFactory,
        HybridCache hybridCache,
        CacheExpirePublisher publisher,
        CacheExpireOptions options)
        : base(loggerFactory, hybridCache)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(options);

        _publisher = publisher;
        _options = options;
    }

    /// <inheritdoc />
    protected override async ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        // local expiration first via the base behavior
        var response = await base.Process(request, next, cancellationToken).ConfigureAwait(false);

        if (request is ICacheExpire cacheRequest)
            PublishExpire(cacheRequest, cancellationToken);

        return response;
    }

    private void PublishExpire(ICacheExpire cacheRequest, CancellationToken cancellationToken)
    {
        var cacheKey = cacheRequest.GetCacheKey();
        var cacheTags = cacheRequest.GetCacheTags();

        // nothing to expire
        if (string.IsNullOrEmpty(cacheKey) && (cacheTags?.Any() != true))
            return;

        var message = new CacheExpireMessage
        {
            Key = cacheKey,
            Tags = cacheTags?.ToArray(),
            SourceId = _options.SourceId,
        };

        try
        {
            // fire-and-forget publish to prevent slowing down request processing, log any errors
            _publisher
                .PublishAsync(message, cancellationToken)
                .RunInBackground(Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error publishing cache expiration message: {ErrorMessage}", ex.Message);
        }
    }
}
