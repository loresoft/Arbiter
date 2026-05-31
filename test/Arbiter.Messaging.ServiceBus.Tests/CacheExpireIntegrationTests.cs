using Arbiter.CommandQuery.Definitions;
using Arbiter.Mediation;
using Arbiter.Messaging.ServiceBus.Cache;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Arbiter.Messaging.ServiceBus.Tests;

[NotInParallel]
[Category("LocalOnly")]
[Property("Category", "LocalOnly")]
public class CacheExpireIntegrationTests
{
    private const string ConnectionStringName = "AzureWebJobsServiceBus";
    private const string TopicName = "cache-expire-test";
    private const string SubscriptionName = "cache-expire-sub";

    private static readonly TimeSpan ExpireTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);

    [Test]
    public async Task ExpireCommandRemovesCacheKeyOnSubscriber()
    {
        var cacheKey = $"cache-expire-key-{Guid.NewGuid():N}";
        var cacheTag = $"cache-expire-tag-{Guid.NewGuid():N}";

        await using var subscriber = await CacheExpireHost.StartSubscriberAsync("subscriber-instance");
        await using var publisher = await CacheExpireHost.StartPublisherAsync("publisher-instance");

        // seed the subscriber cache so there is an entry to expire
        var seeded = await GetOrCreateAsync(subscriber.Cache, cacheKey, cacheTag, "seeded-value");
        seeded.Should().Be("seeded-value");

        // sending the command publishes a CacheExpireMessage from the publisher instance
        var command = new ExpireCacheCommand(cacheKey, cacheTag);
        var response = await publisher.Mediator.Send<ExpireCacheCommand, bool>(command);
        response.Should().BeTrue();

        var expired = await WaitForKeyExpiredAsync(subscriber.Cache, cacheKey, cacheTag);

        expired.Should().BeTrue("the subscriber should expire the cache key after receiving the message");
    }

    [Test]
    public async Task ExpireCommandRemovesCacheTagOnSubscriber()
    {
        var cacheKey = $"cache-expire-key-{Guid.NewGuid():N}";
        var cacheTag = $"cache-expire-tag-{Guid.NewGuid():N}";
        var taggedKey = $"tagged-key-{Guid.NewGuid():N}";

        await using var subscriber = await CacheExpireHost.StartSubscriberAsync("subscriber-instance");
        await using var publisher = await CacheExpireHost.StartPublisherAsync("publisher-instance");

        // seed a separate entry that only shares the tag, to verify tag-based expiration
        var seeded = await GetOrCreateAsync(subscriber.Cache, taggedKey, cacheTag, "tagged-value");
        seeded.Should().Be("tagged-value");

        var command = new ExpireCacheCommand(cacheKey, cacheTag);
        var response = await publisher.Mediator.Send<ExpireCacheCommand, bool>(command);
        response.Should().BeTrue();

        var expired = await WaitForKeyExpiredAsync(subscriber.Cache, taggedKey, cacheTag);

        expired.Should().BeTrue("the subscriber should expire the tagged cache entry after receiving the message");
    }

    [Test]
    public async Task ExpireCommandDoesNotExpirePublisherOwnMessage()
    {
        // the publisher also subscribes with the same SourceId; the processor must skip messages it published
        var cacheKey = $"cache-expire-key-{Guid.NewGuid():N}";
        var cacheTag = $"cache-expire-tag-{Guid.NewGuid():N}";

        await using var publisher = await CacheExpireHost.StartSubscriberAsync("shared-instance");

        var seeded = await GetOrCreateAsync(publisher.Cache, cacheKey, cacheTag, "seeded-value");
        seeded.Should().Be("seeded-value");

        // publish a message stamped with the same SourceId the subscriber uses
        var sender = publisher.Services.GetRequiredKeyedService<Azure.Messaging.ServiceBus.ServiceBusSender>(TopicName);
        var message = new CacheExpireMessage
        {
            Key = cacheKey,
            Tags = [cacheTag],
            SourceId = "shared-instance",
        };
        await sender.SendAsJsonAsync(message);

        // give the processor time to receive and skip the message
        await Task.Delay(TimeSpan.FromSeconds(5));

        // the cache entry should still be present because the processor skips its own SourceId
        var current = await GetOrCreateAsync(publisher.Cache, cacheKey, cacheTag, "fresh-value");

        current.Should().Be("seeded-value", "the processor should skip messages stamped with its own SourceId");
    }


    private static async Task<bool> WaitForKeyExpiredAsync(HybridCache cache, string cacheKey, string cacheTag)
    {
        using var cancellation = new CancellationTokenSource(ExpireTimeout);

        while (!cancellation.IsCancellationRequested)
        {
            // a cache miss runs the factory which returns the sentinel value
            var value = await GetOrCreateAsync(cache, cacheKey, cacheTag, "expired-sentinel");
            if (value == "expired-sentinel")
                return true;

            try
            {
                await Task.Delay(PollInterval, cancellation.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        return false;
    }

    private static async ValueTask<string> GetOrCreateAsync(HybridCache cache, string cacheKey, string cacheTag, string value)
    {
        return await cache.GetOrCreateAsync(
            cacheKey,
            _ => ValueTask.FromResult(value),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
            },
            tags: [cacheTag]);
    }


    private sealed record ExpireCacheCommand(string Key, string Tag) : IRequest<bool>, ICacheExpire
    {
        public string? GetCacheKey() => Key;

        public string? GetCacheTag() => Tag;
    }

    private sealed class ExpireCacheCommandHandler : IRequestHandler<ExpireCacheCommand, bool>
    {
        // the handler itself does no work; cache expiration is performed by the pipeline behavior
        public ValueTask<bool> Handle(ExpireCacheCommand request, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(true);
    }

    private sealed class CacheExpireHost : IAsyncDisposable
    {
        private readonly IHost _host;

        private CacheExpireHost(IHost host)
        {
            _host = host;
        }

        public IServiceProvider Services => _host.Services;

        public HybridCache Cache => _host.Services.GetRequiredService<HybridCache>();

        public IMediator Mediator => _host.Services.GetRequiredService<IMediator>();

        public static async Task<CacheExpireHost> StartPublisherAsync(string sourceId)
        {
            var host = BuildHost(services =>
            {
                services.AddMediator();
                services.TryAddTransient<IRequestHandler<ExpireCacheCommand, bool>, ExpireCacheCommandHandler>();
                services.AddServiceBusCacheExpirePublisher(TopicName, options => options.SourceId = sourceId);
            });

            await host.StartAsync();
            return new CacheExpireHost(host);
        }

        public static async Task<CacheExpireHost> StartSubscriberAsync(string sourceId)
        {
            var host = BuildHost(services =>
                services.AddServiceBusCacheExpire(
                    serviceName: "CacheExpireService",
                    topicName: TopicName,
                    subscriptionName: SubscriptionName,
                    configureOptions: options => options.SourceId = sourceId));

            await host.StartAsync();
            return new CacheExpireHost(host);
        }

        private static IHost BuildHost(Action<IServiceCollection> configure)
        {
            var builder = Host.CreateApplicationBuilder();

            // user-secret connection string shared with ServiceBusIntegrationTests
            builder.Configuration.AddUserSecrets("a6c8f6a5-2d49-4b1b-a7df-0243c7ed11b7");

            builder.Services.AddHybridCache();
            builder.Services.AddServiceBus(
                serviceName: "CacheExpireService",
                nameOrConnectionString: ConnectionStringName,
                configureBus: entities => entities.AddTopic(TopicName, SubscriptionName));

            configure(builder.Services);

            return builder.Build();
        }

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
