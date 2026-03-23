using System.Collections.Concurrent;
using System.Diagnostics;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class NewSqlGuidTests
{
    [Test]
    public void NewSqlGuid_ShouldGenerateValidGuid()
    {
        // Act
        var guid = Guid.NewSqlGuid();

        // Assert
        guid.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void NewSqlGuid_ShouldGenerateUniqueGuids()
    {
        // Arrange
        const int count = 1000;
        var guids = new HashSet<Guid>();

        // Act
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewSqlGuid());
        }

        // Assert
        guids.Count.Should().Be(count);
    }

    [Test]
    public void NewSqlGuid_ShouldHaveTimestampProgression()
    {
        // Arrange - Generate GUIDs with delays to ensure timestamp changes
        var guids = new List<Guid>();

        for (int i = 0; i < 10; i++)
        {
            guids.Add(Guid.NewSqlGuid());
            Thread.Sleep(5); // Small delay to ensure timestamp changes
        }

        // Assert - In a SQL GUID the 48-bit timestamp occupies positions 24-35 (Data4[2..7])
        // stored in big-endian, so lexicographic comparison reflects numeric ordering
        for (int i = 1; i < guids.Count; i++)
        {
            var timestamp1 = guids[i - 1].ToString().Substring(24, 12);
            var timestamp2 = guids[i].ToString().Substring(24, 12);

            var comparison = string.Compare(timestamp2, timestamp1, StringComparison.Ordinal);
            comparison.Should().BeGreaterThanOrEqualTo(0,
                $"Timestamp portion of GUID {i} ({timestamp2}) should be >= GUID {i-1} ({timestamp1})");
        }
    }

    [Test]
    public void NewSqlGuid_ShouldHaveCorrectVersion()
    {
        for (int i = 0; i < 100; i++)
        {
            var sqlGuid = Guid.NewSqlGuid();
            // Restore standard .NET byte ordering; in the standard UUID string format
            // xxxxxxxx-xxxx-Vxxx-xxxx-xxxxxxxxxxxx the version nibble is at position 14
            var guidString = sqlGuid.FromSqlGuid().ToString();

            guidString[14].Should().Be('7', $"GUID {sqlGuid} should embed UUIDv7 version");
        }
    }

    [Test]
    public void NewSqlGuid_ShouldHaveCorrectVariant()
    {
        for (int i = 0; i < 100; i++)
        {
            var sqlGuid = Guid.NewSqlGuid();
            // Restore standard .NET byte ordering; in the standard UUID string format
            // xxxxxxxx-xxxx-xxxx-Vxxx-xxxxxxxxxxxx the variant nibble is at position 19
            var variantChar = char.ToLower(sqlGuid.FromSqlGuid().ToString()[19]);
            var validVariants = new[] { '8', '9', 'a', 'b' };

            validVariants.Should().Contain(variantChar, $"GUID {sqlGuid} should have RFC 9562 variant (found {variantChar})");
        }
    }

    [Test]
    public void NewSqlGuid_ShouldBeThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int guidsPerThread = 100;
        var allGuids = new ConcurrentBag<Guid>();
        var threads = new List<Thread>();

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            var thread = new Thread(() =>
            {
                for (int j = 0; j < guidsPerThread; j++)
                {
                    allGuids.Add(Guid.NewSqlGuid());
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        allGuids.Count.Should().Be(threadCount * guidsPerThread);
        allGuids.Distinct().Count().Should().Be(threadCount * guidsPerThread);
    }

    [Test]
    public async Task NewSqlGuid_ShouldBeThreadSafeAsync()
    {
        // Arrange
        const int taskCount = 10;
        const int guidsPerTask = 100;
        var allGuids = new ConcurrentBag<Guid>();

        // Act
        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < guidsPerTask; i++)
                {
                    allGuids.Add(Guid.NewSqlGuid());
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        allGuids.Count.Should().Be(taskCount * guidsPerTask);
        allGuids.Distinct().Count().Should().Be(taskCount * guidsPerTask);
    }

    [Test]
    public void NewSqlGuid_ShouldBeMonotonicWithDelay()
    {
        // Arrange
        const int count = 20;
        var guids = new List<Guid>();

        // Act - Generate with delays to ensure time progression
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewSqlGuid());
            Thread.Sleep(2);
        }

        // Assert - In a SQL GUID, positions 24-35 hold the timestamp in big-endian
        for (int i = 1; i < guids.Count; i++)
        {
            var timestamp1 = guids[i - 1].ToString().Substring(24, 12);
            var timestamp2 = guids[i].ToString().Substring(24, 12);

            var comparison = string.Compare(timestamp2, timestamp1, StringComparison.Ordinal);
            comparison.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Test]
    public void NewSqlGuid_TimestampShouldBeSimilarForRapidGeneration()
    {
        // Arrange
        const int count = 100;
        var guids = new List<Guid>();

        // Act - Generate GUIDs as fast as possible
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewSqlGuid());
        }

        // Assert - In a SQL GUID, positions 24-35 hold the timestamp; most should share
        // the same value when generated rapidly within the same millisecond
        var timestamps = guids.Select(g => g.ToString().Substring(24, 12)).ToList();
        var uniqueTimestamps = timestamps.Distinct().Count();

        // Should have relatively few unique timestamps for rapid generation
        uniqueTimestamps.Should().BeLessThan(count / 2,
            "Rapid generation should produce GUIDs with similar timestamps");
    }

    [Test]
    public void NewSqlGuid_ShouldHaveConsistentTimestampPrefixForRapidGeneration()
    {
        // Arrange & Act - Generate many GUIDs rapidly
        var guids = Enumerable.Range(0, 1000)
            .Select(_ => Guid.NewSqlGuid())
            .ToList();

        // Assert - In a SQL GUID, positions 24-35 hold the 48-bit timestamp in big-endian
        var timestampGroups = guids.GroupBy(g => g.ToString().Substring(24, 12)).ToList();

        // Should have relatively few timestamp groups for rapid generation
        timestampGroups.Count.Should().BeLessThan(100,
            "Rapidly generated GUIDs should share similar timestamps, reducing to few groups");

        // The largest group should contain a significant portion of GUIDs
        var largestGroup = timestampGroups.Max(g => g.Count());
        largestGroup.Should().BeGreaterThan(100,
            "Many GUIDs should share the same timestamp when generated rapidly");
    }

    [Test]
    public void NewSqlGuid_ShouldProduceDistinctRandomPortions()
    {
        // Arrange
        const int count = 1000;
        var randomPortions = new HashSet<string>();

        // Act
        for (int i = 0; i < count; i++)
        {
            var guid = Guid.NewSqlGuid();
            var bytes = guid.ToByteArray();

            // Extract random portion (bytes 6-15)
            var randomPortion = BitConverter.ToString(bytes, 6, 10);
            randomPortions.Add(randomPortion);
        }

        // Assert - Should have high diversity in random portions
        randomPortions.Count.Should().BeGreaterThan((int)(count * 0.95),
            "At least 95% of random portions should be unique");
    }

    [Test]
    public void NewSqlGuid_PerformanceBenchmark()
    {
        // Arrange
        const int iterations = 10000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            _ = Guid.NewSqlGuid();
        }

        stopwatch.Stop();

        // Assert - Should complete in reasonable time (< 1 second for 10k iterations)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);

        var averageMicroseconds = (stopwatch.Elapsed.TotalMicroseconds / iterations);
        Console.WriteLine($"Average time per GUID: {averageMicroseconds:F2} μs");
    }

    [Test]
    public void NewSqlGuid_ShouldNotCollideUnderHighLoad()
    {
        // Arrange
        const int totalGuids = 10000;
        var guids = new ConcurrentBag<Guid>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        // Act
        Parallel.For(0, totalGuids, parallelOptions, _ =>
        {
            guids.Add(Guid.NewSqlGuid());
        });

        // Assert
        guids.Count.Should().Be(totalGuids);
        guids.Distinct().Count().Should().Be(totalGuids);
    }

    [Test]
    public void NewSqlGuid_ShouldHaveNonZeroRandomBytes()
    {
        // Arrange
        const int count = 100;

        // Act & Assert
        for (int i = 0; i < count; i++)
        {
            var guid = Guid.NewSqlGuid();
            var bytes = guid.ToByteArray();

            // Random portion is bytes 6-15 (10 bytes total)
            var randomBytes = bytes.Skip(6).Take(10).ToArray();

            // At least one byte should be non-zero (extremely high probability with crypto RNG)
            randomBytes.Any(b => b != 0).Should().BeTrue(
                $"GUID {guid} should have non-zero random bytes");
        }
    }

    [Test]
    public void NewSqlGuid_FormattingShouldMatchStandardGuidFormat()
    {
        // Act
        var guid = Guid.NewSqlGuid();
        var guidString = guid.ToString();

        // Assert - Should match standard GUID format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        guidString.Length.Should().Be(36);
        guidString[8].Should().Be('-');
        guidString[13].Should().Be('-');
        guidString[18].Should().Be('-');
        guidString[23].Should().Be('-');
    }

    [Test]
    public void NewSqlGuid_ShouldProduceDifferentGuidsEachTime()
    {
        // Arrange
        const int iterations = 1000;
        var guids = new List<Guid>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            guids.Add(Guid.NewSqlGuid());
        }

        // Assert - All should be unique
        var distinctCount = guids.Distinct().Count();
        distinctCount.Should().Be(iterations, "Every generated GUID should be unique");
    }

    [Test]
    public void NewSqlGuid_WithExplicitTimestamp_ShouldNotBeEmpty()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var guid = Guid.NewSqlGuid(timestamp);

        // Assert
        guid.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void NewSqlGuid_WithSameTimestamp_ShouldGenerateUniqueGuids()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var guid1 = Guid.NewSqlGuid(timestamp);
        var guid2 = Guid.NewSqlGuid(timestamp);

        // Assert
        guid1.Should().NotBe(guid2);
    }

    [Test]
    public void NewSqlGuid_WithConsecutiveTimestamps_ShouldPreserveChronologicalOrder()
    {
        // Arrange - use an offset large enough to change the timestamp high bits
        var t1 = DateTimeOffset.UnixEpoch.AddMilliseconds(1_000_000);
        var t2 = t1.AddHours(1);

        // Act - convert back to standard .NET byte ordering so Guid.CompareTo reflects timestamp order
        var guid1 = Guid.NewSqlGuid(t1).FromSqlGuid();
        var guid2 = Guid.NewSqlGuid(t2).FromSqlGuid();

        // Assert
        guid1.CompareTo(guid2).Should().BeLessThan(0);
    }
}
