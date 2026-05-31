using System.Text.Json;

using Arbiter.Mediation;
using Arbiter.Messaging.ServiceBus.Cache;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Messaging.ServiceBus.Tests;

public class ServiceBusCacheExpireExtensionsTests
{
    private const string ConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc";

    [Test]
    public void AddServiceBusCacheExpirePublisher_RegistersPipelineBehavior()
    {
        var services = new ServiceCollection();

        services.AddServiceBusCacheExpirePublisher("cache-expire");

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>)
            && d.ImplementationType == typeof(ServiceBusCacheExpireBehavior<,>));
    }

    [Test]
    public void AddServiceBusCacheExpirePublisher_RegistersOptionsWithTopicName()
    {
        var services = new ServiceCollection();

        services.AddServiceBusCacheExpirePublisher("cache-expire");

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<CacheExpireOptions>();

        options.TopicName.Should().Be("cache-expire");
    }

    [Test]
    public void AddServiceBusCacheExpirePublisher_AppliesConfigureOptions()
    {
        var services = new ServiceCollection();

        services.AddServiceBusCacheExpirePublisher(
            "cache-expire",
            options => options.SourceId = "instance-a");

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<CacheExpireOptions>();

        options.SourceId.Should().Be("instance-a");
    }

    [Test]
    public void AddServiceBusCacheExpirePublisher_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddServiceBusCacheExpirePublisher("cache-expire");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddServiceBusCacheExpirePublisher_ThrowsForNullOrWhiteSpaceTopicName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddServiceBusCacheExpirePublisher("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddServiceBusCacheExpireSubscriber_RegistersProcessor()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: ConnectionString,
            configureBus: bus => bus.AddTopic("cache-expire"));

        services.AddServiceBusCacheExpireSubscriber(
            serviceName: "TestService",
            topicName: "cache-expire",
            subscriptionName: "app-instance");

        services.Should().Contain(d => d.ServiceType == typeof(CacheExpireProcessor));
    }

    [Test]
    public void AddServiceBusCacheExpireSubscriber_ThrowsForNullOrWhiteSpaceSubscriptionName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddServiceBusCacheExpireSubscriber(
            serviceName: "TestService",
            topicName: "cache-expire",
            subscriptionName: "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddServiceBusCacheExpire_RegistersPublisherAndSubscriber()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: ConnectionString,
            configureBus: bus => bus.AddTopic("cache-expire"));

        services.AddServiceBusCacheExpire(
            serviceName: "TestService",
            topicName: "cache-expire",
            subscriptionName: "app-instance");

        services.Should().Contain(d =>
            d.ServiceType == typeof(IPipelineBehavior<,>)
            && d.ImplementationType == typeof(ServiceBusCacheExpireBehavior<,>));
        services.Should().Contain(d => d.ServiceType == typeof(CacheExpireProcessor));
    }

    [Test]
    public void CacheExpireOptions_GeneratesUniqueSourceIdPerInstance()
    {
        var first = new CacheExpireOptions { TopicName = "cache-expire" };
        var second = new CacheExpireOptions { TopicName = "cache-expire" };

        first.SourceId.Should().NotBe(second.SourceId);
    }

    [Test]
    public void CacheExpireMessage_RoundTripsThroughJson()
    {
        var message = new CacheExpireMessage
        {
            Key = "key-1",
            Tags = ["tag-a", "tag-b"],
            SourceId = "instance-a",
        };

        var json = JsonSerializer.Serialize(message);
        var result = JsonSerializer.Deserialize<CacheExpireMessage>(json);

        result.Should().NotBeNull();
        result!.Key.Should().Be("key-1");
        result.Tags.Should().BeEquivalentTo(["tag-a", "tag-b"]);
        result.SourceId.Should().Be("instance-a");
    }
}
