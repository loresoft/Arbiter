namespace Arbiter.Messaging.ServiceBus.Tests;

public class ServiceBusOptionsTests
{
    [Test]
    public void DefaultValues_AreSetCorrectly()
    {
        var options = new ServiceBusOptions();

        options.ServiceKey.Should().BeNull();
        options.NameSuffix.Should().BeNull();
        options.SubscriptionSuffix.Should().BeNull();
        options.Queues.Should().NotBeNull();
        options.Queues.Should().BeEmpty();
        options.Topics.Should().NotBeNull();
        options.Topics.Should().BeEmpty();
        options.Subscriptions.Should().NotBeNull();
        options.Subscriptions.Should().BeEmpty();
        options.DefaultMessageTimeToLive.Should().Be(TimeSpan.FromDays(7));
        options.MaxDeliveryCount.Should().Be(5);
        options.LockDuration.Should().Be(TimeSpan.FromMinutes(5));
        options.DeadLetteringOnMessageExpiration.Should().BeTrue();
        options.EnableBatchedOperations.Should().BeTrue();
    }

    [Test]
    public void WithDefaultMessageTimeToLive_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();
        var ttl = TimeSpan.FromDays(14);

        var result = options.WithDefaultMessageTimeToLive(ttl);

        result.Should().BeSameAs(options);

        options.DefaultMessageTimeToLive.Should().Be(ttl);
    }

    [Test]
    public void WithDefaultMessageTimeToLive_ThrowsForZeroOrNegative()
    {
        var options = new ServiceBusOptions();

        var act = () => options.WithDefaultMessageTimeToLive(TimeSpan.Zero);
        act.Should().Throw<ArgumentOutOfRangeException>();

        act = () => options.WithDefaultMessageTimeToLive(TimeSpan.FromSeconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void WithMaxDeliveryCount_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();

        var result = options.WithMaxDeliveryCount(10);

        result.Should().BeSameAs(options);

        options.MaxDeliveryCount.Should().Be(10);
    }

    [Test]
    public void WithMaxDeliveryCount_ThrowsForLessThanOne()
    {
        var options = new ServiceBusOptions();

        var act = () => options.WithMaxDeliveryCount(0);
        act.Should().Throw<ArgumentOutOfRangeException>();

        act = () => options.WithMaxDeliveryCount(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void WithLockDuration_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();
        var lockDuration = TimeSpan.FromMinutes(10);

        var result = options.WithLockDuration(lockDuration);

        result.Should().BeSameAs(options);

        options.LockDuration.Should().Be(lockDuration);
    }

    [Test]
    public void WithLockDuration_ThrowsForZeroOrNegative()
    {
        var options = new ServiceBusOptions();

        var act = () => options.WithLockDuration(TimeSpan.Zero);
        act.Should().Throw<ArgumentOutOfRangeException>();

        act = () => options.WithLockDuration(TimeSpan.FromSeconds(-1));
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void WithDeadLetteringOnMessageExpiration_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();

        var result = options.WithDeadLetteringOnMessageExpiration(false);

        result.Should().BeSameAs(options);

        options.DeadLetteringOnMessageExpiration.Should().BeFalse();
    }

    [Test]
    public void WithEnableBatchedOperations_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();

        var result = options.WithEnableBatchedOperations(false);

        result.Should().BeSameAs(options);

        options.EnableBatchedOperations.Should().BeFalse();
    }

    [Test]
    public void WithNameSuffix_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();
        var suffix = "dev";

        var result = options.WithNameSuffix(suffix);

        result.Should().BeSameAs(options);

        options.NameSuffix.Should().Be(suffix);
    }

    [Test]
    public void WithNameSuffix_SupportsNullNameSuffix()
    {
        var options = new ServiceBusOptions { NameSuffix = "dev" };

        var result = options.WithNameSuffix(null);

        result.Should().BeSameAs(options);
        options.NameSuffix.Should().BeNull();
    }

    [Test]
    public void WithSubscriptionSuffix_SetsValueAndReturnsInstance()
    {
        var options = new ServiceBusOptions();
        var suffix = "instance1";

        var result = options.WithSubscriptionSuffix(suffix);

        result.Should().BeSameAs(options);

        options.SubscriptionSuffix.Should().Be(suffix);
    }

    [Test]
    public void WithSubscriptionSuffix_SupportsNullSubscriptionSuffix()
    {
        var options = new ServiceBusOptions { SubscriptionSuffix = "instance1" };

        var result = options.WithSubscriptionSuffix(null);

        result.Should().BeSameAs(options);
        options.SubscriptionSuffix.Should().BeNull();
    }

    [Test]
    public void WithSubscriptionSuffix_DoesNotChangeNameSuffix()
    {
        var options = new ServiceBusOptions();

        options.WithSubscriptionSuffix("instance1");

        options.NameSuffix.Should().BeNull();
    }

    [Test]
    public void AddQueue_AddsQueueToCollection()
    {
        var options = new ServiceBusOptions();

        var result = options.AddQueue("orders");

        result.Should().BeSameAs(options);

        options.Queues.Should().Contain("orders");
        options.Queues.Should().HaveCount(1);
    }

    [Test]
    public void AddQueue_ThrowsForNullOrWhiteSpace()
    {
        var options = new ServiceBusOptions();

        var act = () => options.AddQueue(null!);
        act.Should().Throw<ArgumentException>();

        act = () => options.AddQueue("");
        act.Should().Throw<ArgumentException>();

        act = () => options.AddQueue("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddTopic_AddsTopicToCollection()
    {
        var options = new ServiceBusOptions();

        var result = options.AddTopic("events");

        result.Should().BeSameAs(options);

        options.Topics.Should().Contain("events");
        options.Topics.Should().HaveCount(1);
    }

    [Test]
    public void AddTopic_ThrowsForNullOrWhiteSpace()
    {
        var options = new ServiceBusOptions();

        var act = () => options.AddTopic(null!);
        act.Should().Throw<ArgumentException>();

        act = () => options.AddTopic("");
        act.Should().Throw<ArgumentException>();

        act = () => options.AddTopic("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddTopic_WithSubscriptions_AddsTopicAndSubscriptions()
    {
        var options = new ServiceBusOptions();
        var subscriptions = new[] { "sub1", "sub2" };

        var result = options.AddTopic("events", subscriptions);

        result.Should().BeSameAs(options);

        options.Topics.Should().Contain("events");
        options.Topics.Should().HaveCount(1);
        options.Subscriptions.Should().ContainKey("events");
        options.Subscriptions["events"].Should().BeEquivalentTo(subscriptions);
    }

    [Test]
    public void AddTopic_WithSubscriptions_ThrowsForNullTopicName()
    {
        var options = new ServiceBusOptions();

        var act = () => options.AddTopic(null!, new[] { "sub1" });
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void AddTopic_WithSubscriptions_ThrowsForNullSubscriptions()
    {
        var options = new ServiceBusOptions();

        var act = () => options.AddTopic("events", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void FormatName_ReturnsBaseNameWhenNoSuffix()
    {
        var options = new ServiceBusOptions();
        var baseName = "orders";

        var result = options.FormatName(baseName);

        result.Should().Be(baseName);
    }

    [Test]
    public void FormatName_AppendsHyphenAndSuffixWhenSet()
    {
        var options = new ServiceBusOptions { NameSuffix = "dev" };
        var baseName = "orders";

        var result = options.FormatName(baseName);

        result.Should().Be("orders-dev");
    }

    [Test]
    public void FormatName_ThrowsForNullOrWhiteSpace()
    {
        var options = new ServiceBusOptions();

        var act = () => options.FormatName(null!);
        act.Should().Throw<ArgumentException>();

        act = () => options.FormatName("");
        act.Should().Throw<ArgumentException>();

        act = () => options.FormatName("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void FormatSubscriptionName_ReturnsBaseNameWhenNoSuffix()
    {
        var options = new ServiceBusOptions();
        var baseName = "subscription1";

        var result = options.FormatSubscription(baseName);

        result.Should().Be(baseName);
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public void FormatSubscriptionName_ReturnsBaseNameWhenSuffixIsBlank(string suffix)
    {
        var options = new ServiceBusOptions { SubscriptionSuffix = suffix };

        var result = options.FormatSubscription("subscription1");

        result.Should().Be("subscription1");
    }

    [Test]
    public void FormatSubscriptionName_AppendsHyphenAndSuffixWhenSet()
    {
        var options = new ServiceBusOptions { SubscriptionSuffix = "instance1" };

        var result = options.FormatSubscription("subscription1");

        result.Should().Be("subscription1-instance1");
    }

    [Test]
    public void FormatSubscriptionName_IgnoresNameSuffix()
    {
        var options = new ServiceBusOptions { NameSuffix = "dev" };

        var result = options.FormatSubscription("subscription1");

        result.Should().Be("subscription1");
    }

    [Test]
    public void FormatName_IgnoresSubscriptionSuffix()
    {
        var options = new ServiceBusOptions { SubscriptionSuffix = "instance1" };

        var result = options.FormatName("orders");

        result.Should().Be("orders");
    }

    [Test]
    public void FormatSubscriptionName_ThrowsForNullOrWhiteSpace()
    {
        var options = new ServiceBusOptions();

        var act = () => options.FormatSubscription(null!);
        act.Should().Throw<ArgumentException>();

        act = () => options.FormatSubscription("");
        act.Should().Throw<ArgumentException>();

        act = () => options.FormatSubscription("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void FluentChaining_WorksCorrectly()
    {
        var options = new ServiceBusOptions();

        var result = options
            .WithNameSuffix("prod")
            .WithSubscriptionSuffix("instance1")
            .WithDefaultMessageTimeToLive(TimeSpan.FromDays(30))
            .WithMaxDeliveryCount(10)
            .WithLockDuration(TimeSpan.FromMinutes(3))
            .WithDeadLetteringOnMessageExpiration(false)
            .WithEnableBatchedOperations(false)
            .AddQueue("orders")
            .AddQueue("payments")
            .AddTopic("events", new[] { "subscription1", "subscription2" });

        result.Should().BeSameAs(options);

        options.NameSuffix.Should().Be("prod");
        options.SubscriptionSuffix.Should().Be("instance1");
        options.DefaultMessageTimeToLive.Should().Be(TimeSpan.FromDays(30));
        options.MaxDeliveryCount.Should().Be(10);
        options.LockDuration.Should().Be(TimeSpan.FromMinutes(3));
        options.DeadLetteringOnMessageExpiration.Should().BeFalse();
        options.EnableBatchedOperations.Should().BeFalse();
        options.Queues.Should().HaveCount(2);
        options.Queues.Should().Contain("orders");
        options.Queues.Should().Contain("payments");
        options.Topics.Should().HaveCount(1);
        options.Topics.Should().Contain("events");
        options.Subscriptions["events"].Should().HaveCount(2);
    }
}
