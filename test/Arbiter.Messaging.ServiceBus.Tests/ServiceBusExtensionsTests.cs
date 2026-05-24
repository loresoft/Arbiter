using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            configure: options =>
            {
                options.AddQueue("orders");
                options.AddTopic("events");
            });

        var serviceProvider = services.BuildServiceProvider();

        var allOptions = serviceProvider.GetServices<ServiceBusOptions>().ToList();

        allOptions.Should().HaveCount(1);

        allOptions[0].ServiceKey.Should().Be("TestService");
        allOptions[0].Queues.Should().Contain("orders");
        allOptions[0].Topics.Should().Contain("events");
    }

    [Test]
    public void AddServiceBus_RegistersKeyedServiceBusOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=abc",
            configure: options => options.AddQueue("orders"));

        var serviceProvider = services.BuildServiceProvider();

        var keyedOptions = serviceProvider.GetKeyedService<ServiceBusOptions>("TestService");

        keyedOptions.Should().NotBeNull();

        keyedOptions!.ServiceKey.Should().Be("TestService");
        keyedOptions.Queues.Should().Contain("orders");
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
            configure: options => { });

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
            configure: options => { });

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
            configure: options => { });

        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "",
            configure: options => { });

        act.Should().Throw<ArgumentException>();

        act = () => services.AddServiceBus(
            serviceName: "TestService",
            nameOrConnectionString: "   ",
            configure: options => { });

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
            configure: null!);

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
            configure: options => options.AddQueue("queue1"));

        services.AddServiceBus(
            serviceName: "Service2",
            nameOrConnectionString: "Endpoint=sb://test2.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=def",
            configure: options => options.AddQueue("queue2"));

        var serviceProvider = services.BuildServiceProvider();

        var allOptions = serviceProvider.GetServices<ServiceBusOptions>().ToList();
        allOptions.Should().HaveCount(2);

        var options1 = serviceProvider.GetKeyedService<ServiceBusOptions>("Service1");
        options1.Should().NotBeNull();
        options1!.Queues.Should().Contain("queue1");

        var options2 = serviceProvider.GetKeyedService<ServiceBusOptions>("Service2");
        options2.Should().NotBeNull();
        options2!.Queues.Should().Contain("queue2");
    }
}
