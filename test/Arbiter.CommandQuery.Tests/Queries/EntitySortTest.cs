using Arbiter.CommandQuery.Queries;

namespace Arbiter.CommandQuery.Tests.Queries;

public class EntitySortTest
{
    [Test]
    [Arguments("Name:Ascending", "Name", "Ascending")]
    [Arguments("Name:Descending", "Name", "Descending")]
    [Arguments("Name : Descending", "Name", "Descending")]
    [Arguments("Name", "Name", null)]
    [Arguments("", null, null)]
    public void Parse(string source, string? name, string? direction)
    {
        var sort = EntitySort.Parse(source);

        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(direction))
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
