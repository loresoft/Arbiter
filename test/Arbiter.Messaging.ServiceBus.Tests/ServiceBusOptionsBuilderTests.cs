using Arbiter.CommandQuery.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;

namespace Arbiter.Messaging.ServiceBus.Tests;

public class ServiceBusOptionsBuilderTests
{
    [Test]
    public void WithSubscriptionSuffix_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();
        var builder = CreateBuilder(options);

        var result = builder.WithSubscriptionSuffix("instance1");

        result.Should().BeSameAs(builder);

        options.SubscriptionSuffix.Should().Be("instance1");
    }

    [Test]
    public void WithSubscriptionSuffix_SupportsNullSubscriptionSuffix()
    {
        var options = new ServiceBusOptions { SubscriptionSuffix = "instance1" };
        var builder = CreateBuilder(options);

        var result = builder.WithSubscriptionSuffix(string.Empty);

        result.Should().BeSameAs(builder);

        options.SubscriptionSuffix.Should().BeEmpty();
    }

    [Test]
    public void WithSubscriptionSuffix_DoesNotChangeNameSuffix()
    {
        var options = new ServiceBusOptions();
        var builder = CreateBuilder(options);

        builder.WithSubscriptionSuffix("instance1");

        options.NameSuffix.Should().BeNull();
    }

    [Test]
    public void WithEnvironmentSuffix_UsesEnvironmentOptions()
    {
        var environmentOptions = new EnvironmentOptions { EnvironmentName = "Staging" };
        var hostEnvironment = new HostingEnvironment { EnvironmentName = "Production" };

        var services = new ServiceCollection();
        services.AddSingleton(Options.Create(environmentOptions));
        services.AddSingleton<IHostEnvironment>(hostEnvironment);

        var options = new ServiceBusOptions();
        var builder = new ServiceBusOptionsBuilder(options, services.BuildServiceProvider());

        var result = builder.WithEnvironmentSuffix();

        result.Should().BeSameAs(builder);

        options.NameSuffix.Should().Be("staging");
    }

    [Test]
    public void WithEnvironmentSuffix_FallsBackToHostEnvironment()
    {
        var hostEnvironment = new HostingEnvironment { EnvironmentName = "Production" };

        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(hostEnvironment);

        var options = new ServiceBusOptions();
        var builder = new ServiceBusOptionsBuilder(options, services.BuildServiceProvider());

        builder.WithEnvironmentSuffix();

        options.NameSuffix.Should().Be("production");
    }

    [Test]
    public void WithEnvironmentSuffix_LeavesNameSuffixWhenEnvironmentNotRegistered()
    {
        var options = new ServiceBusOptions { NameSuffix = "existing" };
        var builder = CreateBuilder(options);

        builder.WithEnvironmentSuffix();

        options.NameSuffix.Should().Be("existing");
    }

    private static ServiceBusOptionsBuilder CreateBuilder(ServiceBusOptions options)
        => new(options, new ServiceCollection().BuildServiceProvider());
}
