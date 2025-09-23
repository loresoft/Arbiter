using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Tests.Queries;

public class EntitySortTest
{
    [Test]
    [Arguments("Name:Ascending", "Name", SortDirections.Ascending)]
    [Arguments("Name Ascending", "Name", SortDirections.Ascending)]
    [Arguments("Name asc", "Name", SortDirections.Ascending)]
    [Arguments("Name:Descending", "Name", SortDirections.Descending)]
    [Arguments("Name Descending", "Name", SortDirections.Descending)]
    [Arguments("Name desc", "Name", SortDirections.Descending)]
    [Arguments("Name : Descending", "Name", SortDirections.Descending)]
    [Arguments("Name", "Name", null)]
    [Arguments("", null, null)]
    public void Parse(string source, string? name, SortDirections? direction)
    {
        var sort = EntitySort.Parse(source);

        if (string.IsNullOrEmpty(name) && direction == null)
        {
            sort.Should().BeNull();
        }
        else
        {
            sort.Should().NotBeNull();
            sort.Name.Should().Be(name);
            sort.Direction.Should().Be(direction);
        }
    }
}
