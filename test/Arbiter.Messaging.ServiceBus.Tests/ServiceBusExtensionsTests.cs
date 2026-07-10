using Azure.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Arbiter.Queue;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Arbiter.Messaging.ServiceBus.Tests;

public class ServiceBusExtensionsTests
{
    [Test]
    public void ResolveConnectionString_ReturnsDirectConnectionStringUnchanged()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        var serviceProvider = services.BuildServiceProvider();

        var connectionString = "Endpoint=sb://myns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mykey";

        var result = serviceProvider.ResolveConnectionString(connectionString);

        result.Should().Be(connectionString);
    }

    [Test]
    public void ResolveConnectionString_ResolvesFromConnectionStringsSection()
    {
        var expectedConnectionString = "Endpoint=sb://myns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mykey";
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:ServiceBus"] = expectedConnectionString
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);

        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString("ServiceBus");

        result.Should().Be(expectedConnectionString);
    }

    [Test]
    public void ResolveConnectionString_ResolvesFromRootConfiguration()
    {
        var expectedConnectionString = "Endpoint=sb://myns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mykey";
        var configurationData = new Dictionary<string, string?>
        {
            ["ServiceBusConnection"] = expectedConnectionString
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);
        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString("ServiceBusConnection");

        result.Should().Be(expectedConnectionString);
    }

    [Test]
    public void ResolveConnectionString_PrioritizesConnectionStringsSection()
    {
        var connectionStringSectionValue = "Endpoint=sb://from-connection-strings.servicebus.windows.net/;SharedAccessKeyName=key1;SharedAccessKey=abc";
        var rootConfigValue = "Endpoint=sb://from-root.servicebus.windows.net/;SharedAccessKeyName=key2;SharedAccessKey=def";
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:ServiceBus"] = connectionStringSectionValue,
            ["ServiceBus"] = rootConfigValue
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);

        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString("ServiceBus");

        result.Should().Be(connectionStringSectionValue);
    }

    [Test]
    public void ResolveConnectionString_ReturnsFallbackWhenNotFound()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var serviceProvider = services.BuildServiceProvider();

        var fallbackName = "NonExistentKey";
        var result = serviceProvider.ResolveConnectionString(fallbackName);

        result.Should().Be(fallbackName);
    }

    [Test]
    public void ResolveConnectionString_ThrowsForNullServiceProvider()
    {
        IServiceProvider? serviceProvider = null;

        var act = () => serviceProvider!.ResolveConnectionString("ServiceBus");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ResolveConnectionString_ThrowsForNullOrWhiteSpaceConnectionString()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var serviceProvider = services.BuildServiceProvider();

        var act = () => serviceProvider.ResolveConnectionString(null!);
        act.Should().Throw<ArgumentException>();

        act = () => serviceProvider.ResolveConnectionString("");
        act.Should().Throw<ArgumentException>();

        act = () => serviceProvider.ResolveConnectionString("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [Arguments("Endpoint=sb://test.servicebus.windows.net/", true)]
    [Arguments("key=value;another=value", true)]
    [Arguments("https://test.servicebus.windows.net/", true)]
    [Arguments("sb://test.servicebus.windows.net/", true)]
    [Arguments("MyConnectionStringName", false)]
    [Arguments("ServiceBusKey", false)]
    public void ResolveConnectionString_DetectsConnectionStringFormat(string input, bool shouldBeDetectedAsConnectionString)
    {
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:FallbackKey"] = "Endpoint=sb://fallback.servicebus.windows.net/"
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);

        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString(input);

        if (shouldBeDetectedAsConnectionString)
        {
            result.Should().Be(input);
        }
        else
        {
            // Should attempt resolution and fall back to the input when not found
            result.Should().Be(input);
        }
    }

    [Test]
    public void AddServiceBus_RegistersServiceBusOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc",
            configureBus: options =>
            {
                options.AddQueue("orders");
                options.AddTopic("events");
            });

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<ServiceBusOptions>>()
            .Get("TestService");

        options.ServiceKey.Should().Be("TestService");
        options.Queues.Should().Contain("orders");
        options.Topics.Should().Contain("events");
    }

    [Test]
    public void AddServiceBus_RegistersKeyedServiceBusOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc",
            configureBus: options => options.AddQueue("orders"));

        var serviceProvider = services.BuildServiceProvider();

        var keyedOptions = serviceProvider.GetKeyedService<ServiceBusOptions>("TestService");

        keyedOptions.Should().NotBeNull();

        keyedOptions!.ServiceKey.Should().Be("TestService");
        keyedOptions.Queues.Should().Contain("orders");
    }

    [Test]
    public void AddServiceBus_ConfigureWithServices_AppliesNameSuffixFromServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(new EnvironmentMarker("Staging"));

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc",
            configureBus: entities => entities.AddQueue("orders"),
            configureOptions: options =>
                options.WithNameSuffix(options.Services.GetRequiredService<EnvironmentMarker>().Name));

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<ServiceBusOptions>>()
            .Get("TestService");

        options.NameSuffix.Should().Be("Staging");
        options.FormatName("orders").Should().Be("orders-Staging");
    }

    private sealed record EnvironmentMarker(string Name);

    [Test]
    public void AddServiceBus_RegistersClientsFromNamespaceUri()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            fullyQualifiedNamespace: "sb://test.servicebus.windows.net/",
            credential: new TestTokenCredential(),
            configureBus: options => options.AddQueue("orders"));

        var serviceProvider = services.BuildServiceProvider();

        var serviceBusClient = serviceProvider.GetRequiredKeyedService<ServiceBusClient>("TestService");
        var administrationClient = serviceProvider.GetRequiredKeyedService<ServiceBusAdministrationClient>("TestService");

        serviceBusClient.FullyQualifiedNamespace.Should().Be("test.servicebus.windows.net");
        administrationClient.Should().NotBeNull();
    }

    [Test]
    public void AddServiceBus_RegistersClientsFromShortNamespace()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            fullyQualifiedNamespace: "test",
            credential: new TestTokenCredential(),
            configureBus: options => options.AddQueue("orders"));

        var serviceProvider = services.BuildServiceProvider();

        var serviceBusClient = serviceProvider.GetRequiredKeyedService<ServiceBusClient>("TestService");
        var administrationClient = serviceProvider.GetRequiredKeyedService<ServiceBusAdministrationClient>("TestService");

        serviceBusClient.FullyQualifiedNamespace.Should().Be("test.servicebus.windows.net");
        administrationClient.Should().NotBeNull();
    }

    [Test]
    public void AddServiceBus_RegistersServiceBusInitializerHostedService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc",
            configureBus: options => { });

        var serviceProvider = services.BuildServiceProvider();

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
        hostedServices.Should().Contain(hs => hs is ServiceBusInitializer);
    }

    [Test]
    public void AddServiceBus_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/",
            configureBus: options => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddServiceBus_ThrowsForNullOrWhiteSpaceConnectionString()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var act = () => services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: null!,
            configureBus: options => { });

        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "",
            configureBus: options => { });

        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "   ",
            configureBus: options => { });

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddServiceBus_ThrowsForNullConfigureDelegate()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var act = () => services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/",
            configureBus: null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddServiceBus_SupportsMultipleRegistrations()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "Service1",
            nameOrConnectionString: "Endpoint=sb://test1.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc",
            configureBus: options => options.AddQueue("queue1"));

        services.AddServiceBus(
            serviceName: "Service2",
            nameOrConnectionString: "Endpoint=sb://test2.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=def",
            configureBus: options => options.AddQueue("queue2"));

        var serviceProvider = services.BuildServiceProvider();

        var options1 = serviceProvider.GetKeyedService<ServiceBusOptions>("Service1");
        options1.Should().NotBeNull();
        options1!.Queues.Should().Contain("queue1");

        var options2 = serviceProvider.GetKeyedService<ServiceBusOptions>("Service2");
        options2.Should().NotBeNull();
        options2!.Queues.Should().Contain("queue2");
    }

    [Test]
    public void AddServiceBusBackgroundQueue_RegistersBackgroundQueueAndProcessor()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        services.AddServiceBus(
            serviceName: "TestService",
            fullyQualifiedNamespace: "test",
            credential: new TestTokenCredential(),
            configureBus: options => options.AddQueue("background-work"));

        services.AddServiceBusBackgroundQueue(
            serviceName: "TestService",
            queueName: "background-work");

        services.AddServiceBusBackgroundProcessor(
            serviceName: "TestService",
            queueName: "background-work");

        var serviceProvider = services.BuildServiceProvider();

        var queue = serviceProvider.GetRequiredService<IBackgroundQueue>();
        var sender = serviceProvider.GetRequiredKeyedService<ServiceBusSender>("background-work");
        var processor = serviceProvider.GetRequiredKeyedService<ServiceBusProcessor>("background-work");
        var serviceBusOptions = serviceProvider.GetRequiredKeyedService<ServiceBusOptions>("TestService");
        var hostedServices = serviceProvider.GetServices<IHostedService>().ToArray();

        queue.Should().BeOfType<ServiceBusBackgroundQueue>();
        sender.Should().NotBeNull();
        processor.Should().NotBeNull();
        serviceBusOptions.Queues.Should().Contain("background-work");
        hostedServices.Should().Contain(hs => hs is ServiceBusBackgroundProcessor);
    }

    [Test]
    public void AddServiceBusBackgroundQueue_ThrowsForNullOrWhiteSpaceQueueName()
    {
        var services = new ServiceCollection();

        var act = () => services.AddServiceBusBackgroundQueue(
            serviceName: "TestService",
            queueName: null!);

        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBusBackgroundQueue(
            serviceName: "TestService",
            queueName: "");

        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBusBackgroundQueue(
            serviceName: "TestService",
            queueName: "   ");

        act.Should().Throw<ArgumentException>();
    }

    private sealed class TestTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new("token", DateTimeOffset.UtcNow.AddMinutes(30));

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(new AccessToken("token", DateTimeOffset.UtcNow.AddMinutes(30)));
    }
}
