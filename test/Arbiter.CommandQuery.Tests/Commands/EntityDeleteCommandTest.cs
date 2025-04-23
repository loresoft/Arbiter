using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Tests.Models;

namespace Arbiter.CommandQuery.Tests.Commands;

public class EntityDeleteCommandTest
{
    [Test]
    public void ConstructorWithId()
    {
        var id = Guid.NewGuid();
        var deleteCommand = new EntityDeleteCommand<Guid, LocationReadModel>(MockPrincipal.Default, id);

        deleteCommand.Should().NotBeNull();
        deleteCommand.Id.Should().NotBe(Guid.Empty);
        deleteCommand.Id.Should().Be(id);

        deleteCommand.Principal.Should().NotBeNull();

    }
}
