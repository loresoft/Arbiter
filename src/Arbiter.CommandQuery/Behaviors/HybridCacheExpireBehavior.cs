using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Behaviors;

/// <summary>
/// A behavior for removing a cache tag of the response from <see cref="HybridCache"/>.
/// <typeparamref name="TRequest"/> must implement <see cref="ICacheExpire"/> for the cached tag.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class HybridCacheExpireBehavior<TRequest, TResponse> : PipelineBehaviorBase<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ICacheExpire
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HybridCacheExpireBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from</param>
    /// <param name="hybridCache">The hybrid cache.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="hybridCache"/> is null</exception>
    public HybridCacheExpireBehavior(
        ILoggerFactory loggerFactory,
        HybridCache hybridCache)
        : base(loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(hybridCache);

        HybridCache = hybridCache;
    }

    /// <summary>
    /// Gets the <see cref="HybridCache"/> used to expire cache entries.
    /// </summary>
    protected HybridCache HybridCache { get; }

    /// <inheritdoc />
    protected override async ValueTask<TResponse?> Process(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        var response = await next(cancellationToken).ConfigureAwait(false);

        // expire cache
        if (request is ICacheExpire cacheRequest)
            await ExpireCache(cacheRequest, cancellationToken).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    /// Removes the cache key and cache tags of the specified <paramref name="cacheRequest"/> from the local <see cref="HybridCache"/>.
    /// </summary>
    /// <param name="cacheRequest">The request describing the cache key and tags to expire.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous expire operation.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="cacheRequest"/> is null</exception>
    protected async ValueTask ExpireCache(ICacheExpire cacheRequest, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cacheRequest);

        var cacheKey = cacheRequest.GetCacheKey();
        if (!string.IsNullOrEmpty(cacheKey))
            await HybridCache.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        foreach (var cacheTag in cacheRequest.GetCacheTags())
        {
            if (!string.IsNullOrEmpty(cacheTag))
                await HybridCache.RemoveByTagAsync(cacheTag, cancellationToken).ConfigureAwait(false);
        }
    }
}
