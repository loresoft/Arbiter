using Arbiter.CommandQuery.Definitions;
using Arbiter.Mediation;
using Arbiter.Messaging.WebPubSub.Cache;

using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Arbiter.Messaging.WebPubSub.Tests;

[NotInParallel]
[Category("LocalOnly")]
[Property("Category", "LocalOnly")]
public class CacheExpireIntegrationTests
{
    private const string ConnectionStringName = "AzureWebPubSub";
    private const string HubName = "cacheExpireTest";
    private const string GroupName = "cache-expire-group";

    private static readonly TimeSpan ExpireTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);

    [Test]
    public async Task ExpireCommandRemovesCacheKeyOnSubscriber()
    {
        var cacheKey = $"cache-expire-key-{Guid.NewGuid():N}";
        var cacheTag = $"cache-expire-tag-{Guid.NewGuid():N}";

        await using var subscriber = await CacheExpireHost.StartSubscriberAsync();
        await using var publisher = await CacheExpireHost.StartPublisherAsync();

        // the subscriber connects and joins the group in the background; Web PubSub does not
        // persist messages, so publishing before the group is joined loses the message
        await WaitForSubscriberReadyAsync(subscriber);

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

        await using var subscriber = await CacheExpireHost.StartSubscriberAsync();
        await using var publisher = await CacheExpireHost.StartPublisherAsync();

        await WaitForSubscriberReadyAsync(subscriber);

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
    public async Task ExpireMessageDoesNotExpireOwnSourceId()
    {
        // the processor must skip messages stamped with its own SourceId
        var cacheKey = $"cache-expire-key-{Guid.NewGuid():N}";
        var cacheTag = $"cache-expire-tag-{Guid.NewGuid():N}";

        await using var subscriber = await CacheExpireHost.StartSubscriberAsync();

        await WaitForSubscriberReadyAsync(subscriber);

        var seeded = await GetOrCreateAsync(subscriber.Cache, cacheKey, cacheTag, "seeded-value");
        seeded.Should().Be("seeded-value");

        var subscriberOptions = subscriber.Services.GetRequiredService<CacheExpireOptions>();
        var serviceClient = subscriber.Services.GetRequiredKeyedService<WebPubSubServiceClient>(HubName);
        var message = new CacheExpireMessage
        {
            Key = cacheKey,
            Tags = [cacheTag],
            SourceId = subscriberOptions.SourceId,
        };
        await serviceClient.SendToGroupAsJsonAsync(
            groupName: subscriberOptions.GroupName,
            message: message,
            jsonTypeInfo: CacheExpireSerializerContext.Default.CacheExpireMessage);

        // give the processor time to receive and skip the message
        await Task.Delay(TimeSpan.FromSeconds(5));

        // the cache entry should still be present because the processor skips its own SourceId
        var current = await GetOrCreateAsync(subscriber.Cache, cacheKey, cacheTag, "fresh-value");

        current.Should().Be("seeded-value", "the processor should skip messages stamped with its own SourceId");
    }


    private static async Task WaitForSubscriberReadyAsync(CacheExpireHost subscriber)
    {
        var probeKey = $"cache-expire-probe-{Guid.NewGuid():N}";
        var probeTag = $"cache-expire-probe-tag-{Guid.NewGuid():N}";

        var options = subscriber.Services.GetRequiredService<CacheExpireOptions>();
        var serviceClient = subscriber.Services.GetRequiredKeyedService<WebPubSubServiceClient>(HubName);

        using var cancellation = new CancellationTokenSource(ReadyTimeout);

        while (!cancellation.IsCancellationRequested)
        {
            await GetOrCreateAsync(subscriber.Cache, probeKey, probeTag, "probe-value");

            // a unique SourceId ensures the processor does not skip the probe
            var probe = new CacheExpireMessage
            {
                Key = probeKey,
                SourceId = Guid.NewGuid().ToString("N"),
            };

            await serviceClient.SendToGroupAsJsonAsync(
                groupName: options.GroupName,
                message: probe,
                jsonTypeInfo: CacheExpireSerializerContext.Default.CacheExpireMessage,
                cancellationToken: cancellation.Token);

            try
            {
                await Task.Delay(PollInterval, cancellation.Token);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            var value = await GetOrCreateAsync(subscriber.Cache, probeKey, probeTag, "probe-expired");
            if (value == "probe-expired")
                return;
        }

        throw new TimeoutException("The subscriber did not join the Web PubSub group before the timeout expired.");
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

        public static async Task<CacheExpireHost> StartPublisherAsync()
        {
            var host = BuildHost(services =>
            {
                services.AddMediator();
                services.TryAddTransient<IRequestHandler<ExpireCacheCommand, bool>, ExpireCacheCommandHandler>();
                services.AddWebPubSubCacheExpirePublisher("CacheExpireService", HubName, GroupName);
            });

            await host.StartAsync();
            return new CacheExpireHost(host);
        }

        public static async Task<CacheExpireHost> StartSubscriberAsync()
        {
            var host = BuildHost(services =>
                services.AddWebPubSubCacheExpire(
                    serviceName: "CacheExpireService",
                    hubName: HubName,
                    groupName: GroupName));

            await host.StartAsync();
            return new CacheExpireHost(host);
        }

        private static IHost BuildHost(Action<IServiceCollection> configure)
        {
            var builder = Host.CreateApplicationBuilder();

            // user-secret connection string shared with the other Web PubSub tests
            builder.Configuration.AddUserSecrets("a6c8f6a5-2d49-4b1b-a7df-0243c7ed11b7");

            builder.Services.AddHybridCache();
            builder.Services.AddWebPubSub(
                serviceName: "CacheExpireService",
                nameOrConnectionString: ConnectionStringName,
                configureHubs: hubs => hubs.AddHub(HubName, GroupName));

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
