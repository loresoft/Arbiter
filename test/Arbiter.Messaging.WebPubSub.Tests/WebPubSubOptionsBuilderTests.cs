using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.Messaging.WebPubSub.Tests;

public class WebPubSubOptionsBuilderTests
{
    [Test]
    public void Constructor_ThrowsForNullOptions()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var act = () => new WebPubSubOptionsBuilder(null!, services);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Constructor_ThrowsForNullServices()
    {
        var act = () => new WebPubSubOptionsBuilder(new WebPubSubOptions(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Services_ExposesServiceProvider()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var builder = new WebPubSubOptionsBuilder(new WebPubSubOptions(), services);

        builder.Services.Should().BeSameAs(services);
    }

    [Test]
    public void WithNameSuffix_SetsOptionValue()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);

        var result = builder.WithHubSuffix("dev");

        result.Should().BeSameAs(builder);

        options.HubSuffix.Should().Be("dev");
    }

    [Test]
    public void WithGroupSuffix_SetsOptionValue()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);

        var result = builder.WithGroupSuffix("instance-a");

        result.Should().BeSameAs(builder);

        options.GroupSuffix.Should().Be("instance-a");
    }

    [Test]
    public void WithClientAccessTokenExpiration_SetsOptionValue()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);
        var expiration = TimeSpan.FromMinutes(15);

        var result = builder.WithClientAccessTokenExpiration(expiration);

        result.Should().BeSameAs(builder);

        options.ClientAccessTokenExpiration.Should().Be(expiration);
    }

    [Test]
    public void WithClientAccessTokenExpiration_ThrowsForZero()
    {
        var builder = CreateBuilder(new WebPubSubOptions());

        var act = () => builder.WithClientAccessTokenExpiration(TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void WithAutoReconnect_SetsOptionValue()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);

        var result = builder.WithAutoReconnect(false);

        result.Should().BeSameAs(builder);

        options.AutoReconnect.Should().BeFalse();
    }

    [Test]
    public void WithAutoRejoinGroups_SetsOptionValue()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);

        var result = builder.WithAutoRejoinGroups(false);

        result.Should().BeSameAs(builder);

        options.AutoRejoinGroups.Should().BeFalse();
    }

    [Test]
    public void WithReliableProtocol_SetsOptionValue()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);

        var result = builder.WithReliableProtocol(false);

        result.Should().BeSameAs(builder);

        options.UseReliableProtocol.Should().BeFalse();
    }

    [Test]
    public void Builder_SupportsChaining()
    {
        var options = new WebPubSubOptions();
        var builder = CreateBuilder(options);

        builder
            .WithHubSuffix("dev")
            .WithGroupSuffix("instance-a")
            .WithAutoReconnect(false);

        options.HubSuffix.Should().Be("dev");
        options.GroupSuffix.Should().Be("instance-a");
        options.AutoReconnect.Should().BeFalse();
    }

    private static WebPubSubOptionsBuilder CreateBuilder(WebPubSubOptions options)
        => new(options, new ServiceCollection().BuildServiceProvider());
}
