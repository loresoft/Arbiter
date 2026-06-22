using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Arbiter.Storage.Blobs.Tests;

public class ContainerExtensionsTests
{
    [Test]
    public void ResolveConnectionString_ReturnsDirectConnectionStringUnchanged()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        var serviceProvider = services.BuildServiceProvider();

        var connectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net";

        var result = serviceProvider.ResolveConnectionString(connectionString);

        result.Should().Be(connectionString);
    }

    [Test]
    public void ResolveConnectionString_ResolvesFromConnectionStringsSection()
    {
        var expectedConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net";
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Storage"] = expectedConnectionString
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);

        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString("Storage");

        result.Should().Be(expectedConnectionString);
    }

    [Test]
    public void ResolveConnectionString_ResolvesFromRootConfiguration()
    {
        var expectedConnectionString = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net";
        var configurationData = new Dictionary<string, string?>
        {
            ["StorageConnection"] = expectedConnectionString
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);
        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString("StorageConnection");

        result.Should().Be(expectedConnectionString);
    }

    [Test]
    public void ResolveConnectionString_PrioritizesConnectionStringsSection()
    {
        var connectionStringSectionValue = "DefaultEndpointsProtocol=https;AccountName=section;AccountKey=key;EndpointSuffix=core.windows.net";
        var rootConfigValue = "DefaultEndpointsProtocol=https;AccountName=root;AccountKey=key;EndpointSuffix=core.windows.net";
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Storage"] = connectionStringSectionValue,
            ["Storage"] = rootConfigValue
        };

        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configurationRoot);

        var serviceProvider = services.BuildServiceProvider();

        var result = serviceProvider.ResolveConnectionString("Storage");

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

        var act = () => serviceProvider!.ResolveConnectionString("Storage");

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
    [Arguments("DefaultEndpointsProtocol=https;AccountName=test", true)]
    [Arguments("key=value;another=value", true)]
    [Arguments("https://test.blob.core.windows.net/", true)]
    [Arguments("StorageConnectionName", false)]
    [Arguments("StorageKey", false)]
    public void ResolveConnectionString_DetectsConnectionStringFormat(string input, bool shouldBeDetectedAsConnectionString)
    {
        var configurationData = new Dictionary<string, string?>
        {
            ["ConnectionStrings:FallbackKey"] = "DefaultEndpointsProtocol=https;AccountName=fallback"
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
            result.Should().Be(input);
        }
    }

    [Test]
    public void AddBlobStorage_RegistersBlobStorageOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => options.AddContainer("documents"));

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<BlobStorageOptions>>()
            .Get("TestStorage");

        options.ServiceKey.Should().Be("TestStorage");
        options.Containers.Should().ContainKey("documents");
        options.Containers["documents"].Should().Be(PublicAccessType.None);
    }

    [Test]
    public void AddBlobStorage_RegistersKeyedBlobStorageOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => options.AddContainer("documents"));

        var serviceProvider = services.BuildServiceProvider();

        var keyedOptions = serviceProvider.GetKeyedService<BlobStorageOptions>("TestStorage");

        keyedOptions.Should().NotBeNull();
        keyedOptions.ServiceKey.Should().Be("TestStorage");
        keyedOptions.Containers.Should().ContainKey("documents");
    }

    [Test]
    public void AddBlobStorage_RegistersBlobContainerClientFromConnectionString()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "UseDevelopmentStorage=true",
            configureOptions: options => options.AddContainer("documents"));

        var serviceProvider = services.BuildServiceProvider();

        var containerClient = serviceProvider.GetRequiredKeyedService<BlobContainerClient>("documents");

        containerClient.Name.Should().Be("documents");
        containerClient.Uri.Host.Should().Be("127.0.0.1");
    }

    [Test]
    public void AddBlobStorage_RegistersBlobContainerClientFromUrl()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "https://test.blob.core.windows.net",
            configureOptions: options => options.AddContainer("documents"));

        var serviceProvider = services.BuildServiceProvider();

        var containerClient = serviceProvider.GetRequiredKeyedService<BlobContainerClient>("documents");

        containerClient.Name.Should().Be("documents");
        containerClient.Uri.AbsoluteUri.Should().Be("https://test.blob.core.windows.net/documents");
    }

    [Test]
    public void AddBlobStorage_RegistersBlobContainerClientFromStorageName()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "storageaccount",
            configureOptions: options => options.AddContainer("documents"));

        var serviceProvider = services.BuildServiceProvider();

        var containerClient = serviceProvider.GetRequiredKeyedService<BlobContainerClient>("documents");

        containerClient.Name.Should().Be("documents");
        containerClient.Uri.AbsoluteUri.Should().Be("https://storageaccount.blob.core.windows.net/documents");
    }

    [Test]
    public void AddBlobStorage_AppliesNameSuffixFromConfiguration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => options
                .AddContainer("documents")
                .WithNameSuffix("staging"));

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<BlobStorageOptions>>()
            .Get("TestStorage");

        options.NameSuffix.Should().Be("staging");
        options.FormatName("documents").Should().Be("documents-staging");
    }

    [Test]
    public void AddBlobStorage_StoresContainerPublicAccessType()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => options.AddContainer("documents", PublicAccessType.Blob));

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<BlobStorageOptions>>()
            .Get("TestStorage");

        options.Containers["documents"].Should().Be(PublicAccessType.Blob);
    }

    [Test]
    public void AddBlobStorage_RegistersContainerInitializerHostedService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => { });

        var serviceProvider = services.BuildServiceProvider();

        var hostedServices = serviceProvider.GetServices<IHostedService>().ToList();
        hostedServices.Should().Contain(hs => hs is BlobStorageInitializer);
    }

    [Test]
    public void AddBlobStorage_ThrowsForNullServices()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "https://test.blob.core.windows.net/",
            configureOptions: options => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddBlobStorage_ThrowsForNullOrWhiteSpaceConnectionString()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var act = () => services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: null!,
            configureOptions: options => { });

        act.Should().Throw<ArgumentException>();

        act = () => services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "",
            configureOptions: options => { });

        act.Should().Throw<ArgumentException>();

        act = () => services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "   ",
            configureOptions: options => { });

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddBlobStorage_AllowsNullConfigureDelegate()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        var act = () => services.AddBlobStorage(
            serviceName: "TestStorage",
            nameOrConnectionString: "https://test.blob.core.windows.net/",
            configureOptions: null);

        act.Should().NotThrow();
    }

    [Test]
    public void AddBlobStorage_SupportsMultipleRegistrations()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddBlobStorage(
            serviceName: "Storage1",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test1;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => options.AddContainer("documents"));

        services.AddBlobStorage(
            serviceName: "Storage2",
            nameOrConnectionString: "DefaultEndpointsProtocol=https;AccountName=test2;AccountKey=key;EndpointSuffix=core.windows.net",
            configureOptions: options => options.AddContainer("images"));

        var serviceProvider = services.BuildServiceProvider();

        var options1 = serviceProvider.GetKeyedService<BlobStorageOptions>("Storage1");
        options1.Should().NotBeNull();
        options1!.Containers.Should().ContainKey("documents");

        var options2 = serviceProvider.GetKeyedService<BlobStorageOptions>("Storage2");
        options2.Should().NotBeNull();
        options2!.Containers.Should().ContainKey("images");
    }

}
