using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Tests.Models;

namespace Arbiter.CommandQuery.Tests.Commands;

public class EntityCreateCommandTests
{
    [Test]
    public void ConstructorNullModel()
    {
        Action act = () => new EntityCreateCommand<LocationCreateModel, LocationReadModel>(null, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ConstructorWithModel()
    {
        var generator = new Faker<LocationCreateModel>()
            .RuleFor(p => p.Name, (faker, model) => faker.Company.CompanyName())
            .RuleFor(p => p.Description, (faker, model) => faker.Lorem.Sentence())
            .RuleFor(p => p.AddressLine1, (faker, model) => faker.Address.StreetAddress())
            .RuleFor(p => p.AddressLine2, (faker, model) => faker.Address.SecondaryAddress())
            .RuleFor(p => p.City, (faker, model) => faker.Address.City())
            .RuleFor(p => p.StateProvince, (faker, model) => faker.Address.StateAbbr())
            .RuleFor(p => p.PostalCode, (faker, model) => faker.Address.ZipCode())
            .RuleFor(p => p.Latitude, (faker, model) => faker.Address.Latitude())
            .RuleFor(p => p.Longitude, (faker, model) => faker.Address.Longitude());

        var createModel = generator.Generate();
        createModel.Should().NotBeNull();

        var createCommand = new EntityCreateCommand<LocationCreateModel, LocationReadModel>(MockPrincipal.Default, createModel);
        createCommand.Should().NotBeNull();
        createCommand.Model.Should().NotBeNull();
        createCommand.Principal.Should().NotBeNull();
    }
}
