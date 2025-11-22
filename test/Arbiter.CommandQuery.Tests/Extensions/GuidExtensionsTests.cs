using System.Collections.Concurrent;
using System.Diagnostics;

using Arbiter.CommandQuery.Extensions;

namespace Arbiter.CommandQuery.Tests.Extensions;

public class GuidExtensionsTests
{
    [Test]
    public void NewSequentialId_ShouldGenerateValidGuid()
    {
        // Act
        var guid = Guid.NewSequentialId();

        // Assert
        guid.Should().NotBe(Guid.Empty);
    }

    [Test]
    public void NewSequentialId_ShouldGenerateUniqueGuids()
    {
        // Arrange
        const int count = 1000;
        var guids = new HashSet<Guid>();

        // Act
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewSequentialId());
        }

        // Assert
        guids.Count.Should().Be(count);
    }

    [Test]
    public void NewSequentialId_ShouldHaveTimestampProgression()
    {
        // Arrange - Generate GUIDs with delays to ensure timestamp changes
        var guids = new List<Guid>();

        for (int i = 0; i < 10; i++)
        {
            guids.Add(Guid.NewSequentialId());
            Thread.Sleep(5); // Small delay to ensure timestamp changes
        }

        // Assert - The first 8 characters (timestamp portion) should generally increase
        for (int i = 1; i < guids.Count; i++)
        {
            var timestamp1 = guids[i - 1].ToString().Substring(0, 8);
            var timestamp2 = guids[i].ToString().Substring(0, 8);

            var comparison = string.Compare(timestamp2, timestamp1, StringComparison.Ordinal);
            comparison.Should().BeGreaterThanOrEqualTo(0,
                $"Timestamp portion of GUID {i} ({timestamp2}) should be >= GUID {i-1} ({timestamp1})");
        }
    }

    [Test]
    public void NewSequentialId_ShouldHaveCorrectVersion()
    {
        // Act - Generate multiple GUIDs to test version consistently
        for (int i = 0; i < 100; i++)
        {
            var guid = Guid.NewSequentialId();
            var guidString = guid.ToString();

            // In the GUID string format xxxxxxxx-xxxx-Vxxx-xxxx-xxxxxxxxxxxx,
            // the version is at the first digit of the third group (index 14)
            // But due to GUID's little-endian storage, we need to check position 19
            // Actually, for version 8 UUID, the version bits are in byte 6 (bits 48-51)
            // In string format with mixed endianness: position 14-15 shows byte 7, position 16-17 shows byte 6
            // So the version nibble is at position 16
            var versionChar = guidString[16];

            // Should be version 8 (upper nibble of byte 6)
            versionChar.Should().Be('8', $"GUID {guid} should have version 8 at position 16");
        }
    }

    [Test]
    public void NewSequentialId_ShouldHaveCorrectVariant()
    {
        // Act - Generate multiple GUIDs to test variant consistently
        for (int i = 0; i < 100; i++)
        {
            var guid = Guid.NewSequentialId();
            var guidString = guid.ToString();

            // Variant is at position 19 in the string format: xxxxxxxx-xxxx-xxxx-Vxxx-xxxxxxxxxxxx
            // RFC 9562 variant bits are 10xx, which means first hex digit should be 8, 9, a, or b
            var variantChar = char.ToLower(guidString[19]);
            var validVariants = new[] { '8', '9', 'a', 'b' };

            validVariants.Should().Contain(variantChar, $"GUID {guid} should have RFC 9562 variant (found {variantChar})");
        }
    }

    [Test]
    public void NewSequentialId_ShouldBeThreadSafe()
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
                    allGuids.Add(Guid.NewSequentialId());
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
    public async Task NewSequentialId_ShouldBeThreadSafeAsync()
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
                    allGuids.Add(Guid.NewSequentialId());
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        allGuids.Count.Should().Be(taskCount * guidsPerTask);
        allGuids.Distinct().Count().Should().Be(taskCount * guidsPerTask);
    }

    [Test]
    public void NewSequentialId_ShouldBeMonotonicWithDelay()
    {
        // Arrange
        const int count = 20;
        var guids = new List<Guid>();

        // Act - Generate with delays to ensure time progression
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewSequentialId());
            Thread.Sleep(2);
        }

        // Assert - Check first 8 characters (timestamp) increase or stay same
        for (int i = 1; i < guids.Count; i++)
        {
            var timestamp1 = guids[i - 1].ToString().Substring(0, 8);
            var timestamp2 = guids[i].ToString().Substring(0, 8);

            var comparison = string.Compare(timestamp2, timestamp1, StringComparison.Ordinal);
            comparison.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Test]
    public void NewSequentialId_TimestampShouldBeSimilarForRapidGeneration()
    {
        // Arrange
        const int count = 100;
        var guids = new List<Guid>();

        // Act - Generate GUIDs as fast as possible
        for (int i = 0; i < count; i++)
        {
            guids.Add(Guid.NewSequentialId());
        }

        // Assert - Most GUIDs should share the same timestamp prefix (first few characters)
        var timestamps = guids.Select(g => g.ToString().Substring(0, 8)).ToList();
        var uniqueTimestamps = timestamps.Distinct().Count();

        // Should have relatively few unique timestamps for rapid generation
        uniqueTimestamps.Should().BeLessThan(count / 2,
            "Rapid generation should produce GUIDs with similar timestamps");
    }

    [Test]
    public void NewSequentialId_ShouldHaveConsistentTimestampPrefixForRapidGeneration()
    {
        // Arrange & Act - Generate many GUIDs rapidly
        var guids = Enumerable.Range(0, 1000)
            .Select(_ => Guid.NewSequentialId())
            .ToList();

        // Assert - Most GUIDs should share the same first few characters (timestamp portion)
        // Group by first 8 characters (timestamp prefix)
        var timestampGroups = guids.GroupBy(g => g.ToString().Substring(0, 8)).ToList();

        // Should have relatively few timestamp groups for rapid generation
        timestampGroups.Count.Should().BeLessThan(100,
            "Rapidly generated GUIDs should share similar timestamps, reducing to few groups");

        // The largest group should contain a significant portion of GUIDs
        var largestGroup = timestampGroups.Max(g => g.Count());
        largestGroup.Should().BeGreaterThan(100,
            "Many GUIDs should share the same timestamp prefix when generated rapidly");
    }

    [Test]
    public void NewSequentialId_ShouldProduceDistinctRandomPortions()
    {
        // Arrange
        const int count = 1000;
        var randomPortions = new HashSet<string>();

        // Act
        for (int i = 0; i < count; i++)
        {
            var guid = Guid.NewSequentialId();
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
    public void NewSequentialId_PerformanceBenchmark()
    {
        // Arrange
        const int iterations = 10000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            _ = Guid.NewSequentialId();
        }

        stopwatch.Stop();

        // Assert - Should complete in reasonable time (< 1 second for 10k iterations)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);

        var averageMicroseconds = (stopwatch.Elapsed.TotalMicroseconds / iterations);
        Console.WriteLine($"Average time per GUID: {averageMicroseconds:F2} μs");
    }

    [Test]
    public void NewSequentialId_ShouldNotCollideUnderHighLoad()
    {
        // Arrange
        const int totalGuids = 10000;
        var guids = new ConcurrentBag<Guid>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        // Act
        Parallel.For(0, totalGuids, parallelOptions, _ =>
        {
            guids.Add(Guid.NewSequentialId());
        });

        // Assert
        guids.Count.Should().Be(totalGuids);
        guids.Distinct().Count().Should().Be(totalGuids);
    }

    [Test]
    public void NewSequentialId_ShouldHaveNonZeroRandomBytes()
    {
        // Arrange
        const int count = 100;

        // Act & Assert
        for (int i = 0; i < count; i++)
        {
            var guid = Guid.NewSequentialId();
            var bytes = guid.ToByteArray();

            // Random portion is bytes 6-15 (10 bytes total)
            var randomBytes = bytes.Skip(6).Take(10).ToArray();

            // At least one byte should be non-zero (extremely high probability with crypto RNG)
            randomBytes.Any(b => b != 0).Should().BeTrue(
                $"GUID {guid} should have non-zero random bytes");
        }
    }

    [Test]
    public void NewSequentialId_FormattingShouldMatchStandardGuidFormat()
    {
        // Act
        var guid = Guid.NewSequentialId();
        var guidString = guid.ToString();

        // Assert - Should match standard GUID format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        guidString.Length.Should().Be(36);
        guidString[8].Should().Be('-');
        guidString[13].Should().Be('-');
        guidString[18].Should().Be('-');
        guidString[23].Should().Be('-');
    }

    [Test]
    public void NewSequentialId_ShouldProduceDifferentGuidsEachTime()
    {
        // Arrange
        const int iterations = 1000;
        var guids = new List<Guid>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            guids.Add(Guid.NewSequentialId());
        }

        // Assert - All should be unique
        var distinctCount = guids.Distinct().Count();
        distinctCount.Should().Be(iterations, "Every generated GUID should be unique");
    }
}
