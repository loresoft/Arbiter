using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Definitions;
using Arbiter.Mediation;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus.Cache;

/// <summary>
/// A behavior that expires the local <see cref="HybridCache"/> and publishes a <see cref="CacheExpireMessage"/>
/// to a Service Bus topic so other applications can expire their own caches.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class ServiceBusCacheExpireBehavior<TRequest, TResponse> : HybridCacheExpireBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>, ICacheExpire
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CacheExpireOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusCacheExpireBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create an <see cref="ILogger"/> from.</param>
    /// <param name="hybridCache">The hybrid cache used for local invalidation.</param>
    /// <param name="serviceProvider">The service provider used to resolve the keyed <see cref="ServiceBusSender"/>.</param>
    /// <param name="options">The cache expiration options.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="serviceProvider"/> or <paramref name="options"/> is null.</exception>
    public ServiceBusCacheExpireBehavior(
        ILoggerFactory loggerFactory,
        HybridCache hybridCache,
        IServiceProvider serviceProvider,
        CacheExpireOptions options)
        : base(loggerFactory, hybridCache)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(options);

        _serviceProvider = serviceProvider;
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
            await PublishExpire(cacheRequest, cancellationToken).ConfigureAwait(false);

        return response;
    }

    private async ValueTask PublishExpire(ICacheExpire cacheRequest, CancellationToken cancellationToken)
    {
        var cacheKey = cacheRequest.GetCacheKey();
        var cacheTags = cacheRequest.GetCacheTags()
            .Where(tag => !string.IsNullOrEmpty(tag))
            .ToArray();

        // nothing to expire
        if (string.IsNullOrEmpty(cacheKey) && cacheTags.Length == 0)
            return;

        var message = new CacheExpireMessage
        {
            Key = cacheKey,
            Tags = cacheTags,
            SourceId = _options.SourceId,
        };

        var sender = _serviceProvider.GetRequiredKeyedService<ServiceBusSender>(_options.TopicName);
        await sender.SendAsJsonAsync(message, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
