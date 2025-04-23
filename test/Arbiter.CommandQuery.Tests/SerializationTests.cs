using System.Text.Json;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Converters;
using Arbiter.CommandQuery.Queries;
using Arbiter.CommandQuery.Tests.Fakes;
using Arbiter.CommandQuery.Tests.Models;
using Arbiter.Mediation;


namespace Arbiter.CommandQuery.Tests;

public class SerializationTests
{
    [Test]
    public void EntityCreateCommandSerialize()
    {
        // Create Entity
        var generator = new LocationCreateModelFaker();
        generator.UseSeed(1);

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<LocationCreateModel, LocationReadModel>(MockPrincipal.Default, createModel);

        var options = SerializerOptions();

        var json = JsonSerializer.Serialize(createCommand, options);
        json.Should().NotBeNullOrEmpty();


        var deserializeCommand = JsonSerializer.Deserialize<EntityCreateCommand<LocationCreateModel, LocationReadModel>>(json, options);
        deserializeCommand.Should().NotBeNull();
    }

    [Test]
    public void PolymorphicConverterTest()
    {
        // Create Entity
        var generator = new LocationCreateModelFaker();
        generator.UseSeed(2);

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<LocationCreateModel, LocationReadModel>(MockPrincipal.Default, createModel);

        var options = SerializerOptions();

        var json = JsonSerializer.Serialize<IRequest>(createCommand, options);
        json.Should().NotBeNullOrEmpty();

        var deserializeCommand = JsonSerializer.Deserialize(json, typeof(IRequest), options);
        deserializeCommand.Should().NotBeNull();
        deserializeCommand.Should().BeAssignableTo<IRequest>();
    }

    [Test]
    public void PolymorphicConverterEntityIdentifierQueryTest()
    {
        var queryCommand = new EntityIdentifierQuery<int, LocationReadModel>(MockPrincipal.Default, 1);

        var options = SerializerOptions();

        var json = JsonSerializer.Serialize<IRequest>(queryCommand, options);
        json.Should().NotBeNullOrEmpty();

        var deserializeCommand = JsonSerializer.Deserialize(json, typeof(IRequest), options);
        deserializeCommand.Should().NotBeNull();
        deserializeCommand.Should().BeAssignableTo<IRequest>();
    }

    [Test]
    public void PolymorphicConverterNullTest()
    {
        EntityCreateCommand<LocationCreateModel, LocationReadModel>? createCommand = null;

        var options = SerializerOptions();

        var json = JsonSerializer.Serialize<IRequest?>(createCommand, options);
        json.Should().NotBeNullOrEmpty();

        var deserializeCommand = JsonSerializer.Deserialize(json, typeof(IRequest), options);
        deserializeCommand.Should().BeNull();
    }

    private static JsonSerializerOptions SerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        options.Converters.Add(new ClaimsPrincipalConverter());
        options.Converters.Add(new PolymorphicConverter<IRequest>());

        return options;
    }

}
