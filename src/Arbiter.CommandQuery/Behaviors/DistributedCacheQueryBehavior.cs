using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for caching the response of a query to <see cref="IDistributedCache"/>.
/// <typeparamref name="TRequest"/> must implement <see cref="ICacheResult"/> for the response to be cached.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public partial class DistributedCacheQueryBehavior<TRequest, TResponse> : PipelineBehaviorBase<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IDistributedCache _distributedCache;
    private readonly IDistributedCacheSerializer _distributedCacheSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedCacheQueryBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from</param>
    /// <param name="distributedCache">The distributed cache.</param>
    /// <param name="distributedCacheSerializer">The distributed cache serializer.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DistributedCacheQueryBehavior(
        ILoggerFactory loggerFactory,
        IDistributedCache distributedCache,
        IDistributedCacheSerializer distributedCacheSerializer)
        : base(loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentNullException.ThrowIfNull(distributedCacheSerializer);

        _distributedCache = distributedCache;
        _distributedCacheSerializer = distributedCacheSerializer;
    }

    /// <inheritdoc />
    protected override async ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        // cache only if implements interface
        var cacheRequest = request as ICacheResult;
        if (cacheRequest?.IsCacheable() != true)
            return await next(cancellationToken).ConfigureAwait(false);

        var cacheKey = cacheRequest.GetCacheKey();

        // check cache
        var cachedBuffer = await _distributedCache
            .GetAsync(cacheKey, cancellationToken)
            .ConfigureAwait(false);

        if (cachedBuffer != null)
        {
            var cachedItem = await _distributedCacheSerializer
                .FromByteArrayAsync<TResponse>(cachedBuffer)
                .ConfigureAwait(false);

            LogCacheAction(Logger, "Hit", cacheKey);

            return cachedItem;
        }

        LogCacheAction(Logger, "Miss", cacheKey);

        // continue if not found in cache
        var result = await next(cancellationToken).ConfigureAwait(false);
        if (result == null)
            return result;

        // save to cache
        var itemBuffer = await _distributedCacheSerializer
            .ToByteArrayAsync(result)
            .ConfigureAwait(false);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = cacheRequest.SlidingExpiration(),
            AbsoluteExpiration = cacheRequest.AbsoluteExpiration(),

        };

        await _distributedCache
            .SetAsync(cacheKey, itemBuffer, options, cancellationToken)
            .ConfigureAwait(false);

        LogCacheAction(Logger, "Insert", cacheKey);

        return result;
    }

    [LoggerMessage(1, LogLevel.Trace, "Cache {Action}; Key: '{CacheKey}'")]
    static partial void LogCacheAction(ILogger logger, string action, string cacheKey);

}
