namespace Arbiter.Messaging.WebPubSub.Tests;

public class WebPubSubOptionsTests
{
    [Test]
    public void DefaultValues_AreSetCorrectly()
    {
        var options = new WebPubSubOptions();

        options.ServiceKey.Should().BeNull();
        options.Credential.Should().BeNull();
        options.HubSuffix.Should().BeNull();
        options.GroupSuffix.Should().BeNull();
        options.Hubs.Should().NotBeNull();
        options.Hubs.Should().BeEmpty();
        options.Groups.Should().NotBeNull();
        options.Groups.Should().BeEmpty();
        options.ClientAccessTokenExpiration.Should().Be(TimeSpan.FromHours(1));
        options.AutoReconnect.Should().BeTrue();
        options.AutoRejoinGroups.Should().BeTrue();
        options.UseReliableProtocol.Should().BeTrue();
    }

    [Test]
    public void WithNameSuffix_SetsValueAndReturnsInstance()
    {
        var options = new WebPubSubOptions();

        var result = options.WithHubSuffix("dev");

        result.Should().BeSameAs(options);

        options.HubSuffix.Should().Be("dev");
    }

    [Test]
    public void WithGroupSuffix_SetsValueAndReturnsInstance()
    {
        var options = new WebPubSubOptions();

        var result = options.WithGroupSuffix("instance-a");

        result.Should().BeSameAs(options);

        options.GroupSuffix.Should().Be("instance-a");
    }

    [Test]
    public void WithClientAccessTokenExpiration_SetsValueAndReturnsInstance()
    {
        var options = new WebPubSubOptions();
        var expiration = TimeSpan.FromMinutes(30);

        var result = options.WithClientAccessTokenExpiration(expiration);

        result.Should().BeSameAs(options);

        options.ClientAccessTokenExpiration.Should().Be(expiration);
    }

    [Test]
    public void WithClientAccessTokenExpiration_ThrowsForZeroOrNegative()
    {
        var options = new WebPubSubOptions();

        var act = () => options.WithClientAccessTokenExpiration(TimeSpan.Zero);
        act.Should().Throw<ArgumentOutOfRangeException>();

        act = () => options.WithClientAccessTokenExpiration(TimeSpan.FromSeconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void WithAutoReconnect_SetsValueAndReturnsInstance()
    {
        var options = new WebPubSubOptions();

        var result = options.WithAutoReconnect(false);

        result.Should().BeSameAs(options);

        options.AutoReconnect.Should().BeFalse();
    }

    [Test]
    public void WithAutoRejoinGroups_SetsValueAndReturnsInstance()
    {
        var options = new WebPubSubOptions();

        var result = options.WithAutoRejoinGroups(false);

        result.Should().BeSameAs(options);

        options.AutoRejoinGroups.Should().BeFalse();
    }

    [Test]
    public void WithReliableProtocol_SetsValueAndReturnsInstance()
    {
        var options = new WebPubSubOptions();

        var result = options.WithReliableProtocol(false);

        result.Should().BeSameAs(options);

        options.UseReliableProtocol.Should().BeFalse();
    }

    [Test]
    public void AddHub_AddsHubName()
    {
        var options = new WebPubSubOptions();

        var result = options.AddHub("notifications");

        result.Should().BeSameAs(options);

        options.Hubs.Should().ContainSingle().Which.Should().Be("notifications");
    }

    [Test]
    public void AddHub_WithoutGroups_DoesNotAddGroupEntry()
    {
        var options = new WebPubSubOptions();

        options.AddHub("notifications");

        options.Groups.Should().BeEmpty();
    }

    [Test]
    public void AddHub_ThrowsForNullOrWhiteSpaceHubName()
    {
        var options = new WebPubSubOptions();

        var act = () => options.AddHub("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddHub_WithGroups_AddsHubAndGroups()
    {
        var options = new WebPubSubOptions();

        var result = options.AddHub("notifications", "cache-expire", "alerts");

        result.Should().BeSameAs(options);

        options.Hubs.Should().ContainSingle().Which.Should().Be("notifications");
        options.Groups.Should().ContainKey("notifications");
        options.Groups["notifications"].Should().BeEquivalentTo(["cache-expire", "alerts"]);
    }

    [Test]
    public void Groups_LookupIsCaseInsensitive()
    {
        var options = new WebPubSubOptions();

        options.AddHub("Notifications", "cache-expire");

        options.Groups.Should().ContainKey("notifications");
    }

    [Test]
    public void FormatHub_WithoutSuffix_ReturnsBaseName()
    {
        var options = new WebPubSubOptions();

        options.FormatHub("notifications").Should().Be("notifications");
    }

    [Test]
    public void FormatHub_WithSuffix_AppendsSuffix()
    {
        var options = new WebPubSubOptions().WithHubSuffix("dev");

        options.FormatHub("notifications").Should().Be("notifications_dev");
    }

    [Test]
    public void FormatHub_IgnoresGroupSuffix()
    {
        var options = new WebPubSubOptions().WithGroupSuffix("instance-a");

        options.FormatHub("notifications").Should().Be("notifications");
    }

    [Test]
    public void FormatGroup_WithoutSuffix_ReturnsBaseName()
    {
        var options = new WebPubSubOptions();

        options.FormatGroup("cache-expire").Should().Be("cache-expire");
    }

    [Test]
    public void FormatGroup_WithSuffix_AppendsSuffix()
    {
        var options = new WebPubSubOptions().WithGroupSuffix("instance-a");

        options.FormatGroup("cache-expire").Should().Be("cache-expire-instance-a");
    }

    [Test]
    public void FormatGroup_IgnoresNameSuffix()
    {
        var options = new WebPubSubOptions().WithHubSuffix("dev");

        options.FormatGroup("cache-expire").Should().Be("cache-expire");
    }

    [Test]
    public void FormatHub_ThrowsForNullOrWhiteSpaceBaseName()
    {
        var options = new WebPubSubOptions();

        var act = () => options.FormatHub("   ");

        act.Should().Throw<ArgumentException>();
    }
}
