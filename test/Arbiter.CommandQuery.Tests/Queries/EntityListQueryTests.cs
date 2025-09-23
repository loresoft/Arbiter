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
}
