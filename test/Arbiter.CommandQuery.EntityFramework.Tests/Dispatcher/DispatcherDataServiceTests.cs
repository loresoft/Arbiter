

using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;
using Arbiter.Dispatcher;

using Microsoft.Extensions.DependencyInjection;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Dispatcher;

public class DispatcherDataServiceTests : DatabaseTestBase
{
    [Test]
    public async Task FullTest()
    {
        var dataService = ServiceProvider.GetService<IDispatcherDataService>();
        dataService.Should().NotBeNull();

        var generator = new Faker<PriorityCreateModel>()
            .RuleFor(p => p.Created, (faker, model) => faker.Date.PastOffset())
            .RuleFor(p => p.CreatedBy, (faker, model) => faker.Internet.Email())
            .RuleFor(p => p.Updated, (faker, model) => faker.Date.SoonOffset())
            .RuleFor(p => p.UpdatedBy, (faker, model) => faker.Internet.Email())
            .RuleFor(p => p.Name, (faker, model) => faker.Name.JobType())
            .RuleFor(p => p.Description, (faker, model) => faker.Lorem.Sentence());

        var createModel = generator.Generate();

        var createResult = await dataService.Create<PriorityCreateModel, PriorityReadModel>(createModel);
        createResult.Should().NotBeNull();
        createResult.Name.Should().Be(createModel.Name);

        var searchResult = await dataService.Search<PriorityReadModel>(createModel.Name);
        searchResult.Should().NotBeNull();

        var selectEmptyResult = await dataService.All<PriorityReadModel>();
        selectEmptyResult.Should().NotBeNull();

        var pageEmptyResult = await dataService.Page<PriorityReadModel>();
        pageEmptyResult.Should().NotBeNull();

        var getReadResult = await dataService.Get<int, PriorityReadModel>(createResult.Id);
        getReadResult.Should().NotBeNull();

        var getMultipleResult = await dataService.Get<int, PriorityReadModel>([createResult.Id]);
        getMultipleResult.Should().NotBeNull();


        var getUpdateResult = await dataService.Get<int, PriorityUpdateModel>(createResult.Id);
        getUpdateResult.Should().NotBeNull();

        getUpdateResult.Description = "This is an update";

        var updateResult = await dataService.Update<int, PriorityUpdateModel, PriorityReadModel>(createResult.Id, getUpdateResult);
        updateResult.Should().NotBeNull();

        var deleteResult = await dataService.Delete<int, PriorityReadModel>(createResult.Id);
        deleteResult.Should().NotBeNull();
    }
}
