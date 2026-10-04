using Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Arbiter.CommandQuery.EntityFramework.ValueGeneration;
using Arbiter.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Event = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities.Event;

namespace Arbiter.CommandQuery.EntityFramework.Tests.ValueGeneration;

public class SnowflakeGeneratorTests : DatabaseTestBase
{
    [Test]
    public void GeneratesTemporaryValues_IsFalse()
    {
        var generator = new SnowflakeGenerator();

        generator.GeneratesTemporaryValues.Should().BeFalse();
    }

    [Test]
    public async Task Next_ReturnsPositiveUniqueValuesAscendingByMillisecond()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TrackerContext>();

        var item = new Event { Name = "Next" };
        var entry = context.Entry(item);

        var generator = new SnowflakeGenerator();
        var first = generator.Next(entry);
        var second = generator.Next(entry);

        var firstTimestamp = Snowflake.Default.GetTimestamp(first);
        var secondTimestamp = Snowflake.Default.GetTimestamp(second);

        first.Should().BeGreaterThan(0);
        second.Should().BeGreaterThan(0);
        second.Should().NotBe(first);
        secondTimestamp.Should().BeOnOrAfter(firstTimestamp);
    }

    [Test]
    public async Task Next_EncodesCurrentTimestamp()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TrackerContext>();

        var item = new Event { Name = "Timestamp" };
        var entry = context.Entry(item);

        var before = DateTime.UtcNow.AddSeconds(-1);
        var generator = new SnowflakeGenerator();
        var id = generator.Next(entry);
        var after = DateTime.UtcNow.AddSeconds(1);

        var timestamp = Snowflake.Default.GetTimestamp(id);
        timestamp.Should().BeOnOrAfter(before);
        timestamp.Should().BeOnOrBefore(after);
    }

    [Test]
    public async Task Add_AssignsSnowflakeId()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TrackerContext>();

        var item = new Event { Name = "Add" };
        context.Events.Add(item);

        item.Id.Should().BeGreaterThan(0);
        context.Entry(item).Property(p => p.Id).IsTemporary.Should().BeFalse();
    }

    [Test]
    public async Task SaveChanges_PersistsSnowflakeIds()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TrackerContext>();

        var items = Enumerable.Range(1, 10)
            .Select(i => new Event { Name = $"Event {i}" })
            .ToList();

        context.Events.AddRange(items);
        await context.SaveChangesAsync();

        var ids = items.Select(p => p.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
        ids.Select(Snowflake.Default.GetTimestamp).Should().BeInAscendingOrder();
        ids.Should().AllSatisfy(id => id.Should().BeGreaterThan(0));

        await using var readScope = ServiceProvider.CreateAsyncScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<TrackerContext>();

        var saved = await readContext.Events
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Id)
            .ToListAsync();

        var expected = items
            .OrderBy(p => p.Id)
            .ToList();

        saved.Select(p => p.Id).Should().Equal(expected.Select(p => p.Id));
        saved.Select(p => p.Name).Should().Equal(expected.Select(p => p.Name));
    }
}
