using System.Threading;

using Arbiter.CommandQuery.Commands;
using Arbiter.Dispatcher.Client.Tests.Fakes;
using Arbiter.Dispatcher.Client.Tests.Models;

using MessagePack;

namespace Arbiter.Dispatcher.Client.Tests;

public class SerializationTests
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance)
        .WithCompression(MessagePackCompression.Lz4BlockArray);

    [Test]
    public void EntityCreateCommandSerialize()
    {
        // Create Entity
        var generator = new LocationCreateModelFaker();
        generator.UseSeed(1);

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<LocationCreateModel, LocationReadModel>(MockPrincipal.Default, createModel);

        var requestBytes = MessagePackSerializer.Serialize(createCommand, Options);
        requestBytes.Should().NotBeNullOrEmpty();
    }

}
