using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Tests.Models;

namespace Arbiter.CommandQuery.Tests.Queries;

public class EntityIdentifierQueryTests
{
    [Test]
    public void ConstructorNull()
    {
        var identifierQuery = new EntityIdentifierQuery<Guid, LocationReadModel>(null, Guid.Empty);
        identifierQuery.Should().NotBeNull();
    }

    [Test]
    public void ConstructorWithParameters()
    {
        var id = Guid.NewGuid();
        var identifierQuery = new EntityIdentifierQuery<Guid, LocationReadModel>(MockPrincipal.Default, id);
        identifierQuery.Should().NotBeNull();

        identifierQuery.Id.Should().NotBe(Guid.Empty);
        identifierQuery.Id.Should().Be(id);

        identifierQuery.Principal.Should().NotBeNull();
    }
}
