using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Arbiter.Messaging.WebPubSub.Tests;

public class WebPubSubExtensionsTests
{
    private const string ConnectionString = "Endpoint=https://test.webpubsub.azure.com;AccessKey=abc;Version=1.0;";

    [Test]
    public void ResolveConnectionString_ReturnsDirectConnectionStringUnchanged()
    {
        var serviceProvider = CreateProvider(new ConfigurationBuilder().Build());

        var result = serviceProvider.ResolveConnectionString(ConnectionString);

        result.Should().Be(ConnectionString);
    }

    [Test]
    public void ResolveConnectionString_ResolvesFromConnectionStringsSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:WebPubSub"] = ConnectionString
            })
            .Build();

        var serviceProvider = CreateProvider(configuration);

        var result = serviceProvider.ResolveConnectionString("WebPubSub");

        result.Should().Be(ConnectionString);
    }

    [Test]
    public void ResolveConnectionString_ResolvesFromRootConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WebPubSubConnection"] = ConnectionString
            })
            .Build();

        var serviceProvider = CreateProvider(configuration);

        var result = serviceProvider.ResolveConnectionString("WebPubSubConnection");

        result.Should().Be(ConnectionString);
    }

    [Test]
    public void ResolveConnectionString_PrioritizesConnectionStringsSection()
    {
        const string sectionValue = "Endpoint=https://from-section.webpubsub.azure.com;AccessKey=abc;Version=1.0;";
        const string rootValue = "Endpoint=https://from-root.webpubsub.azure.com;AccessKey=def;Version=1.0;";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:WebPubSub"] = sectionValue,
                ["WebPubSub"] = rootValue
            })
            .Build();

        var serviceProvider = CreateProvider(configuration);

        var result = serviceProvider.ResolveConnectionString("WebPubSub");

        result.Should().Be(sectionValue);
    }

    [Test]
    public void ResolveConnectionString_ReturnsOriginalValueWhenNotFound()
    {
        var serviceProvider = CreateProvider(new ConfigurationBuilder().Build());

        var result = serviceProvider.ResolveConnectionString("Missing");

        result.Should().Be("Missing");
    }

    [Test]
    public void ResolveConnectionString_ThrowsForNullOrWhiteSpaceValue()
    {
        var serviceProvider = CreateProvider(new ConfigurationBuilder().Build());

        var act = () => serviceProvider.ResolveConnectionString("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddWebPubSub_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddWebPubSub(null, ConnectionString, builder => builder.AddHub("notifications"));

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddWebPubSub_ThrowsForNullOrWhiteSpaceConnectionString()
    {
        var services = new ServiceCollection();

        var act = () => services.AddWebPubSub(null, "   ", builder => builder.AddHub("notifications"));

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddWebPubSub_ThrowsForNullConfigureHubs()
    {
        var services = new ServiceCollection();

        var act = () => services.AddWebPubSub(null, ConnectionString, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddWebPubSub_ConfiguresDefaultNamedOptions()
    {
        var serviceProvider = CreateServiceProviderWithWebPubSub(null);

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<WebPubSubOptions>>()
            .Get(Options.DefaultName);

        options.NameOrConnectionString.Should().Be(ConnectionString);
        options.Credential.Should().BeNull();
        options.Hubs.Should().BeEquivalentTo(["notifications"]);
        options.Groups["notifications"].Should().BeEquivalentTo(["cache-expire"]);
    }

    [Test]
    public void AddWebPubSub_WithServiceName_ConfiguresNamedOptions()
    {
        var serviceProvider = CreateServiceProviderWithWebPubSub("primary");

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<WebPubSubOptions>>()
            .Get("primary");

        options.ServiceKey.Should().Be("primary");
        options.Hubs.Should().BeEquivalentTo(["notifications"]);
    }

    [Test]
    public void AddWebPubSub_RegistersOptionsAsKeyedService()
    {
        var serviceProvider = CreateServiceProviderWithWebPubSub("primary");

        var options = serviceProvider.GetRequiredKeyedService<WebPubSubOptions>("primary");

        options.NameOrConnectionString.Should().Be(ConnectionString);
    }

    [Test]
    public void AddWebPubSub_RegistersServiceClientKeyedByHubName()
    {
        var serviceProvider = CreateServiceProviderWithWebPubSub(null);

        var serviceClient = serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications");

        serviceClient.Should().NotBeNull();
        serviceClient.Hub.Should().Be("notifications");
    }

    [Test]
    public void AddWebPubSub_ServiceClientUsesFormattedHubNameWithSuffix()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddWebPubSub(
            null,
            ConnectionString,
            builder => builder.AddHub("notifications"),
            builder => builder.WithHubSuffix("dev"));

        var serviceProvider = services.BuildServiceProvider();

        var serviceClient = serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications");

        serviceClient.Hub.Should().Be("notifications_dev");
    }

    [Test]
    public void AddWebPubSub_ServiceClientIsSingleton()
    {
        var serviceProvider = CreateServiceProviderWithWebPubSub(null);

        var first = serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications");
        var second = serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications");

        first.Should().BeSameAs(second);
    }

    [Test]
    public void AddWebPubSub_RegistersServiceClientForEachHub()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddWebPubSub(
            null,
            ConnectionString,
            builder => builder
                .AddHub("notifications")
                .AddHub("audit"));

        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications").Hub.Should().Be("notifications");
        serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("audit").Hub.Should().Be("audit");
    }

    [Test]
    public void AddWebPubSub_ResolvesConnectionStringFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:WebPubSub"] = ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddWebPubSub(null, "WebPubSub", builder => builder.AddHub("notifications"));

        var serviceProvider = services.BuildServiceProvider();

        var serviceClient = serviceProvider.GetRequiredKeyedService<WebPubSubServiceClient>("notifications");

        serviceClient.Should().NotBeNull();
    }

    [Test]
    public void AddWebPubSubPublisher_RegistersPublisherAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddWebPubSub(null, ConnectionString, builder => builder.AddHub("notifications", "cache-expire"));
        services.AddWebPubSubPublisher<TestPublisher>(null, "notifications");

        services.Should().Contain(d =>
            d.ServiceType == typeof(TestPublisher)
            && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Test]
    public void AddWebPubSubPublisher_ResolvesFormattedHubName()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddWebPubSub(
            null,
            ConnectionString,
            builder => builder.AddHub("notifications", "cache-expire"),
            builder => builder.WithHubSuffix("dev").WithGroupSuffix("dev"));
        services.AddWebPubSubPublisher<TestPublisher>(null, "notifications");

        var publisher = services.BuildServiceProvider().GetRequiredService<TestPublisher>();

        publisher.ResolvedHubName.Should().Be("notifications_dev");
    }

    [Test]
    public void AddWebPubSubPublisher_ResolvesFormattedGroupName()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddWebPubSub(
            null,
            ConnectionString,
            builder => builder.AddHub("notifications", "cache-expire"),
            builder => builder.WithGroupSuffix("dev"));
        services.AddWebPubSubPublisher<TestPublisher>(null, "notifications");

        var publisher = services.BuildServiceProvider().GetRequiredService<TestPublisher>();

        publisher.ResolvedGroups["cache-expire"].Should().Be("cache-expire-dev");
    }

    [Test]
    public void AddWebPubSubPublisher_ResolvesGroupNamesCaseInsensitively()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddWebPubSub(
            null,
            ConnectionString,
            builder => builder.AddHub("notifications", "cache-expire"),
            builder => builder.WithGroupSuffix("dev"));
        services.AddWebPubSubPublisher<TestPublisher>(null, "notifications");

        var publisher = services.BuildServiceProvider().GetRequiredService<TestPublisher>();

        publisher.GetResolvedGroupName("CACHE-EXPIRE").Should().Be("cache-expire-dev");
    }

    [Test]
    public void AddWebPubSubProcessor_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddWebPubSubProcessor<TestProcessor>(null, "notifications");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddWebPubSubProcessor_ThrowsForNullOrWhiteSpaceHubName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddWebPubSubProcessor<TestProcessor>(null, "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddWebPubSubProcessor_RegistersProcessorAndHostedService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddWebPubSub(null, ConnectionString, builder => builder.AddHub("notifications", "cache-expire"));
        services.AddWebPubSubProcessor<TestProcessor>(null, "notifications");

        services.Should().Contain(d =>
            d.ServiceType == typeof(TestProcessor)
            && d.Lifetime == ServiceLifetime.Singleton);

        services.Should().Contain(d =>
            d.ServiceType == typeof(IHostedService)
            && d.Lifetime == ServiceLifetime.Singleton);
    }

    private static ServiceProvider CreateServiceProviderWithWebPubSub(object? serviceName)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddWebPubSub(
            serviceName,
            ConnectionString,
            builder => builder.AddHub("notifications", "cache-expire"));

        return services.BuildServiceProvider();
    }

    private static ServiceProvider CreateProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);

        return services.BuildServiceProvider();
    }
}
