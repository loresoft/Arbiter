namespace Arbiter.Messaging.WebPubSub.Tests;

public class WebPubSubEntityBuilderTests
{
    [Test]
    public void Constructor_ThrowsForNullOptions()
    {
        var act = () => new WebPubSubBuilder(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void AddHub_AddsHubToOptions()
    {
        var options = new WebPubSubOptions();
        var builder = new WebPubSubBuilder(options);

        var result = builder.AddHub("notifications");

        result.Should().BeSameAs(builder);

        options.Hubs.Should().ContainSingle().Which.Should().Be("notifications");
    }

    [Test]
    public void AddHub_WithGroups_AddsHubAndGroupsToOptions()
    {
        var options = new WebPubSubOptions();
        var builder = new WebPubSubBuilder(options);

        builder.AddHub("notifications", "cache-expire");

        options.Hubs.Should().ContainSingle().Which.Should().Be("notifications");
        options.Groups["notifications"].Should().BeEquivalentTo(["cache-expire"]);
    }

    [Test]
    public void AddHub_SupportsChainingMultipleHubs()
    {
        var options = new WebPubSubOptions();
        var builder = new WebPubSubBuilder(options);

        builder
            .AddHub("notifications")
            .AddHub("audit", "cache-expire");

        options.Hubs.Should().BeEquivalentTo(["notifications", "audit"]);
        options.Groups.Should().ContainKey("audit");
        options.Groups.Should().NotContainKey("notifications");
    }
}
