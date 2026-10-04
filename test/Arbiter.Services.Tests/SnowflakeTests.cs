using System.Collections.Concurrent;

using Arbiter.Services;

namespace Arbiter.Services.Tests;

public class SnowflakeTests
{
    [Test]
    public void NextId_ReturnsPositiveValue()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var id = snowflake.NextId();

        id.Should().BePositive();
    }

    [Test]
    public void NextId_CalledTwice_SecondTimestampIsNotEarlier()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var first = snowflake.NextId();
        var second = snowflake.NextId();

        second.Should().NotBe(first);
        snowflake.GetTimestamp(second).Should().BeOnOrAfter(snowflake.GetTimestamp(first));
    }

    [Test]
    public void NextId_CalledRepeatedly_ReturnsIdsAscendingByMillisecond()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var ids = new long[10_000];
        for (int i = 0; i < ids.Length; i++)
            ids[i] = snowflake.NextId();

        ids.Select(snowflake.GetTimestamp).Should().BeInAscendingOrder();
    }

    [Test]
    public void NextId_CalledRepeatedly_ReturnsUniqueIds()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var ids = new HashSet<long>();
        for (int i = 0; i < 10_000; i++)
            ids.Add(snowflake.NextId());

        ids.Should().HaveCount(10_000);
    }

    [Test]
    public async Task NextId_FromMultipleThreads_ReturnsUniqueIds()
    {
        var snowflake = new Snowflake(instanceId: 1);
        var ids = new ConcurrentBag<long>();

        const int threads = 8;
        const int perThread = 2_000;

        var tasks = Enumerable
            .Range(0, threads)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < perThread; i++)
                    ids.Add(snowflake.NextId());
            }));

        await Task.WhenAll(tasks);

        ids.Distinct().Should().HaveCount(threads * perThread);
    }

    [Test]
    public void NextId_WhenCounterExhausted_BorrowsNextMillisecondWithoutBlocking()
    {
        // Two counter bits overflow almost immediately, forcing the borrow-next-millisecond path.
        var snowflake = new Snowflake(instanceId: 1, counterBits: 2);

        var ids = new long[10_000];
        for (int i = 0; i < ids.Length; i++)
            ids[i] = snowflake.NextId();

        ids.Should().OnlyHaveUniqueItems();
        ids.Select(snowflake.GetTimestamp).Should().BeInAscendingOrder();
    }

    [Test]
    public void NextId_LargeVolume_ReturnsUniqueIdsAscendingByMillisecond()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var ids = new long[1_000_000];
        for (int i = 0; i < ids.Length; i++)
            ids[i] = snowflake.NextId();

        ids.Select(snowflake.GetTimestamp).Should().BeInAscendingOrder();
        ids.Distinct().Should().HaveCount(ids.Length);
    }

    [Test]
    public void NextId_ProducesNonConsecutiveIds()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var ids = new long[10_000];
        for (int i = 0; i < ids.Length; i++)
            ids[i] = snowflake.NextId();

        var gaps = ids.Zip(ids.Skip(1), (a, b) => b - a);

        gaps.Should().Contain(g => g > 1);
    }

    [Test]
    public void NextId_WithinSameMillisecond_CountersAreShuffled()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var ids = new long[100_000];
        for (int i = 0; i < ids.Length; i++)
            ids[i] = snowflake.NextId();

        var sameMsGaps = ids
            .Zip(ids.Skip(1), (a, b) => (a, b))
            .Where(p => snowflake.GetTimestamp(p.a) == snowflake.GetTimestamp(p.b))
            .Select(p => p.b - p.a)
            .ToList();

        sameMsGaps.Should().NotBeEmpty();
        sameMsGaps.Should().Contain(g => g < 0);
        sameMsGaps.Count(g => Math.Abs(g) <= 16).Should().BeLessThan(sameMsGaps.Count / 2);
    }

    [Test]
    public void NextId_WhenCounterSpaceFilled_UsesEveryCounterValueOnce()
    {
        const int counterBits = 8;
        var snowflake = new Snowflake(instanceId: 0, instanceBits: 0, counterBits: counterBits);

        var ids = new long[100_000];
        for (int i = 0; i < ids.Length; i++)
            ids[i] = snowflake.NextId();

        var fullMillisecond = ids
            .GroupBy(id => id >> counterBits)
            .FirstOrDefault(g => g.Count() == 1 << counterBits);

        fullMillisecond.Should().NotBeNull();
        fullMillisecond!.Select(id => id & ((1 << counterBits) - 1)).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task NextId_FromMultipleThreads_IsAscendingByMillisecondPerThread()
    {
        var snowflake = new Snowflake(instanceId: 1);

        const int threads = 8;
        const int perThread = 5_000;

        var tasks = Enumerable
            .Range(0, threads)
            .Select(_ => Task.Run(() =>
            {
                var local = new long[perThread];
                for (int i = 0; i < perThread; i++)
                    local[i] = snowflake.NextId();

                return local;
            }));

        var results = await Task.WhenAll(tasks);

        foreach (var local in results)
            local.Select(snowflake.GetTimestamp).Should().BeInAscendingOrder();

        results.SelectMany(r => r).Distinct().Should().HaveCount(threads * perThread);
    }

    [Test]
    public void NextId_EncodesConfiguredInstanceId()
    {
        const long instanceId = 511;
        var snowflake = new Snowflake(instanceId, instanceBits: 10, counterBits: 12);

        var id = snowflake.NextId();
        var encodedInstance = (id >> 12) & 1023;

        encodedInstance.Should().Be(instanceId);
    }

    [Test]
    public void InstanceId_WhenNotSpecified_IsWithinConfiguredRange()
    {
        var snowflake = new Snowflake(instanceBits: 4);

        snowflake.InstanceId.Should().BeInRange(0, 15);
    }

    [Test]
    public void InstanceId_WhenSpecified_IsRetained()
    {
        var snowflake = new Snowflake(instanceId: 7);

        snowflake.InstanceId.Should().Be(7);
    }

    [Test]
    public void GetTimestamp_ReturnsUtcCreationTime()
    {
        var snowflake = new Snowflake(instanceId: 1);
        var before = DateTime.UtcNow;

        var timestamp = snowflake.GetTimestamp(snowflake.NextId());

        timestamp.Kind.Should().Be(DateTimeKind.Utc);
        timestamp.Should().BeCloseTo(before, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void GetTimestamp_HonorsCustomEpoch()
    {
        var epoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var snowflake = new Snowflake(instanceId: 1, epoch: epoch);

        var timestamp = snowflake.GetTimestamp(snowflake.NextId());

        timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void GetTimestamp_ForConsecutiveIds_NeverGoesBackwards()
    {
        var snowflake = new Snowflake(instanceId: 1);

        var first = snowflake.GetTimestamp(snowflake.NextId());
        var second = snowflake.GetTimestamp(snowflake.NextId());

        second.Should().BeOnOrAfter(first);
    }

    [Test]
    public void DefaultEpoch_IsUtc()
    {
        Snowflake.DefaultEpoch.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public void Default_ReturnsSameInstance()
    {
        Snowflake.Default.Should().BeSameAs(Snowflake.Default);
    }

    [Test]
    public void Default_GeneratesPositiveIds()
    {
        Snowflake.Default.NextId().Should().BePositive();
    }

    [Test]
    public void Constructor_WhenEpochIsLocal_CoercesToUtc()
    {
        var epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Local);
        var snowflake = new Snowflake(instanceId: 1, epoch: epoch);

        var timestamp = snowflake.GetTimestamp(snowflake.NextId());

        timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Constructor_WhenInstanceIdExceedsBits_Throws()
    {
        var create = () => new Snowflake(instanceId: 64, instanceBits: 6);

        create.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("instanceId");
    }

    [Test]
    public void Constructor_WhenTimestampBitsIsZero_Throws()
    {
        var create = () => new Snowflake(instanceId: 1, timestampBits: 0);

        create.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("timestampBits");
    }

    [Test]
    public void Constructor_WhenInstanceBitsIsNegative_Throws()
    {
        var create = () => new Snowflake(instanceId: 1, instanceBits: -1);

        create.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("instanceBits");
    }

    [Test]
    public void Constructor_WhenCounterBitsIsZero_Throws()
    {
        var create = () => new Snowflake(instanceId: 1, counterBits: 0);

        create.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("counterBits");
    }

    [Test]
    [Arguments(42, 11, 12)]
    [Arguments(41, 11, 12)]
    [Arguments(62, 1, 1)]
    public void Constructor_WhenBitsExceedSixtyThree_Throws(int timestampBits, int instanceBits, int counterBits)
    {
        var create = () => new Snowflake(instanceId: 0, timestampBits: timestampBits, instanceBits: instanceBits, counterBits: counterBits);

        create.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_WhenBitsSumToSixtyThree_Succeeds()
    {
        var snowflake = new Snowflake(instanceId: 0, timestampBits: 41, instanceBits: 10, counterBits: 12);

        snowflake.NextId().Should().BePositive();
    }
}
