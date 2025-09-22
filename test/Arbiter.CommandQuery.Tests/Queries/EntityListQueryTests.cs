using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Tests.Models;

namespace Arbiter.CommandQuery.Tests.Queries;

public class EntityListQueryTests
{
    [Test]
    public void ConstructorNull()
    {
        var listQuery = new EntityPagedQuery<LocationReadModel>(null, null);
        listQuery.Should().NotBeNull();
    }

    [Test]
    public void ConstructorWithParameters()
    {
        var entityFilter = new EntityFilter { Name = "rank", Value = 7 };
        var entityQuery = new EntityQuery("name = 'blah'", 2, 10, "updated:desc");
        entityQuery.Filter = entityFilter;

        var listQuery = new EntityPagedQuery<LocationReadModel>(MockPrincipal.Default, entityQuery);
        listQuery.Should().NotBeNull();

        listQuery.Query.Should().NotBeNull();
        listQuery.Principal.Should().NotBeNull();
    }
}
