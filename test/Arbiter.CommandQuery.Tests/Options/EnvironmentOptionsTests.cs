using Arbiter.CommandQuery.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Arbiter.CommandQuery.Tests.Options;

public class EnvironmentOptionsTests
{
    private static IConfiguration CreateConfiguration(Dictionary<string, string?>? values = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values ?? [])
            .Build();
    }

    private static EnvironmentOptions ResolveOptions(IServiceCollection services)
    {
        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptions<EnvironmentOptions>>().Value;
    }

    [Test]
    public void EnvironmentNameWithoutHostEnvironmentDefaultsToDevelopment()
    {
        var services = new ServiceCollection();
        services.AddSingleton(CreateConfiguration());
        services.AddEnvironmentOptions();

        var options = ResolveOptions(services);

        options.EnvironmentName.Should().Be("Development");
    }

    [Test]
    public void EnvironmentNameWithHostEnvironmentDefaultsToHostEnvironmentName()
    {
        var hostEnvironment = new TestHostEnvironment { EnvironmentName = "Staging" };

        var services = new ServiceCollection();
        services.AddSingleton(CreateConfiguration());
        services.AddSingleton<IHostEnvironment>(hostEnvironment);
        services.AddEnvironmentOptions();

        var options = ResolveOptions(services);

        options.EnvironmentName.Should().Be("Staging");
    }

    [Test]
    public void EnvironmentNameFromConfigurationOverridesHostEnvironment()
    {
        var values = new Dictionary<string, string?> { ["EnvironmentName"] = "Production" };
        var hostEnvironment = new TestHostEnvironment { EnvironmentName = "Staging" };

        var services = new ServiceCollection();
        services.AddSingleton(CreateConfiguration(values));
        services.AddSingleton<IHostEnvironment>(hostEnvironment);
        services.AddEnvironmentOptions();

        var options = ResolveOptions(services);

        options.EnvironmentName.Should().Be("Production");
    }

    [Test]
    public void EnvironmentNameFromDelegateOverridesHostEnvironment()
    {
        var hostEnvironment = new TestHostEnvironment { EnvironmentName = "Staging" };

        var services = new ServiceCollection();
        services.AddSingleton(CreateConfiguration());
        services.AddSingleton<IHostEnvironment>(hostEnvironment);
        services.AddEnvironmentOptions(options => options.EnvironmentName = "Testing");

        var options = ResolveOptions(services);

        options.EnvironmentName.Should().Be("Testing");
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;

        public string ApplicationName { get; set; } = "Test";

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
