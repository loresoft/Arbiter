using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Models;
using Arbiter.CommandQuery.Tests.Models;

namespace Arbiter.CommandQuery.Tests.Commands;

public class EntityPatchCommandTests
{
    [Test]
    public void ConstructorNullModel()
    {
        Action act = () => new EntityPatchCommand<Guid, LocationReadModel>(null, Guid.Empty, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ConstructorWithPatch()
    {
        var id = Guid.NewGuid();
        var patchModel = new List<JsonPatchOperation>
        {
            new("replace", "/Name", "Test")
        };

        var updateCommand = new EntityPatchCommand<Guid, LocationReadModel>(MockPrincipal.Default, id, patchModel);
        updateCommand.Should().NotBeNull();

        updateCommand.Id.Should().NotBe(Guid.Empty);
        updateCommand.Id.Should().Be(id);

        updateCommand.Patch.Should().NotBeNull();

        updateCommand.Principal.Should().NotBeNull();
    }
}
