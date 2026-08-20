using Microsoft.Extensions.DependencyInjection;

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

        var result = builder.WithSubscriptionSuffix((string?)null);

        result.Should().BeSameAs(builder);

        options.SubscriptionSuffix.Should().BeNull();
    }

    [Test]
    public void WithSubscriptionSuffix_DoesNotChangeNameSuffix()
    {
        var options = new ServiceBusOptions();
        var builder = CreateBuilder(options);

        builder.WithSubscriptionSuffix("instance1");

        options.NameSuffix.Should().BeNull();
    }

    private static ServiceBusOptionsBuilder CreateBuilder(ServiceBusOptions options)
        => new(options, new ServiceCollection().BuildServiceProvider());
}
