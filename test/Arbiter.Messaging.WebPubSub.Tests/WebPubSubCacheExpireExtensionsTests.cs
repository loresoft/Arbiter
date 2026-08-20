using Arbiter.Mediation;
using Arbiter.Messaging.WebPubSub.Cache;

using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Messaging.WebPubSub.Tests;

public class WebPubSubCacheExpireExtensionsTests
{
    private const string ConnectionString = "Endpoint=https://test.webpubsub.azure.com;AccessKey=abc;Version=1.0;";

    [Test]
    public void AddWebPubSubCacheExpirePublisher_RegistersPipelineBehavior()
    {
        var services = new ServiceCollection();

        services.AddWebPubSubCacheExpirePublisher(null, "notifications", "cache-expire");

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>)
            && d.ImplementationType == typeof(WebPubSubCacheExpireBehavior<,>));
    }

    [Test]
    public void AddWebPubSubCacheExpirePublisher_RegistersCacheExpirePublisher()
    {
        var services = new ServiceCollection();

        services.AddWebPubSubCacheExpirePublisher(null, "notifications", "cache-expire");

        services.Should().Contain(d =>
            d.ServiceType == typeof(CacheExpirePublisher)
            && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Test]
    public void AddWebPubSubCacheExpirePublisher_RegistersOptionsWithHubAndGroupName()
    {
        var services = new ServiceCollection();

        services.AddWebPubSubCacheExpirePublisher(null, "notifications", "cache-expire");

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<CacheExpireOptions>();

        options.HubName.Should().Be("notifications");
        options.GroupName.Should().Be("cache-expire");
    }

    [Test]
    public void AddWebPubSubCacheExpirePublisher_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddWebPubSubCacheExpirePublisher(null, "notifications", "cache-expire");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddWebPubSubCacheExpirePublisher_ThrowsForNullOrWhiteSpaceHubName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddWebPubSubCacheExpirePublisher(null, "   ", "cache-expire");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddWebPubSubCacheExpirePublisher_ThrowsForNullOrWhiteSpaceGroupName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddWebPubSubCacheExpirePublisher(null, "notifications", "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddWebPubSubCacheExpireSubscriber_RegistersProcessor()
    {
        var services = CreateServicesWithWebPubSub();

        services.AddWebPubSubCacheExpireSubscriber(null, "notifications", "cache-expire");

        services.Should().Contain(d => d.ServiceType == typeof(CacheExpireProcessor));
    }

    [Test]
    public void AddWebPubSubCacheExpireSubscriber_RegistersOptionsWithHubAndGroupName()
    {
        var services = CreateServicesWithWebPubSub();

        services.AddWebPubSubCacheExpireSubscriber(null, "notifications", "cache-expire");

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<CacheExpireOptions>();

        options.HubName.Should().Be("notifications");
        options.GroupName.Should().Be("cache-expire");
    }

    [Test]
    public void AddWebPubSubCacheExpireSubscriber_DoesNotRegisterPipelineBehavior()
    {
        var services = CreateServicesWithWebPubSub();

        services.AddWebPubSubCacheExpireSubscriber(null, "notifications", "cache-expire");

        services.Should().NotContain(d => d.ServiceType == typeof(IPipelineBehavior<,>));
    }

    [Test]
    public void AddWebPubSubCacheExpire_RegistersPublisherAndSubscriber()
    {
        var services = CreateServicesWithWebPubSub();

        services.AddWebPubSubCacheExpire(null, "notifications", "cache-expire");

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>)
            && d.ImplementationType == typeof(WebPubSubCacheExpireBehavior<,>));

        services.Should().Contain(d => d.ServiceType == typeof(CacheExpireProcessor));
    }

    [Test]
    public void AddWebPubSubCacheExpire_SharesSingleOptionsInstance()
    {
        var services = CreateServicesWithWebPubSub();

        services.AddWebPubSubCacheExpire(
            null,
            "notifications",
            "cache-expire");

        var serviceProvider = services.BuildServiceProvider();

        var first = serviceProvider.GetRequiredService<CacheExpireOptions>();
        var second = serviceProvider.GetRequiredService<CacheExpireOptions>();

        first.Should().BeSameAs(second);
        first.SourceId.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void AddWebPubSubCacheExpire_ResolvesServiceClientForHub()
    {
        var services = CreateServicesWithWebPubSub();

        services.AddWebPubSubCacheExpire(null, "notifications", "cache-expire");

        var serviceProvider = services.BuildServiceProvider();
        var serviceClient = serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications");

        serviceClient.Should().NotBeNull();
    }

    private static ServiceCollection CreateServicesWithWebPubSub()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddWebPubSub(
            null,
            ConnectionString,
            builder => builder.AddHub("notifications", "cache-expire"));

        return services;
    }
}
