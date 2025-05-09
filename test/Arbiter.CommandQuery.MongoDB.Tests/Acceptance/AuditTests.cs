using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;
using Arbiter.CommandQuery.Queries;
using Arbiter.Mediation;

using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

using SystemTextJsonPatch;
using SystemTextJsonPatch.Operations;

using Task = System.Threading.Tasks.Task;

namespace Arbiter.CommandQuery.MongoDB.Tests.Acceptance;

public class AuditTests : DatabaseTestBase
{
    [Test]
    public async Task FullTest()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Create Entity
        var generator = new Faker<AuditCreateModel>()
            .RuleFor(p => p.Id, (faker, model) => ObjectId.GenerateNewId().ToString())
            .RuleFor(p => p.Username, (faker, model) => faker.Internet.UserName())
            .RuleFor(p => p.Content, (faker, model) => faker.Lorem.Paragraph())
            .RuleFor(p => p.Created, (faker, model) => faker.Date.PastOffset())
            .RuleFor(p => p.CreatedBy, (faker, model) => faker.Internet.Email())
            .RuleFor(p => p.Updated, (faker, model) => faker.Date.SoonOffset())
            .RuleFor(p => p.UpdatedBy, (faker, model) => faker.Internet.Email())
            .RuleFor(p => p.Date, (faker, model) => faker.Date.Soon());

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<AuditCreateModel, AuditReadModel>(MockPrincipal.Default, createModel);
        var createResult = await mediator.Send(createCommand);
        createResult.Should().NotBeNull();

        // Get Entity by Key
        var key = createResult.Id;
        var identifierQuery = new EntityIdentifierQuery<string, AuditReadModel>(MockPrincipal.Default, key);
        var identifierResult = await mediator.Send(identifierQuery);
        identifierResult.Should().NotBeNull();
        identifierResult.Username.Should().Be(createModel.Username);

        // Query Entity
        var entityQuery = new EntityQuery
        {
            Sort = new List<EntitySort> { new EntitySort { Name = "Updated", Direction = "Descending" } },
            Filter = new EntityFilter { Name = "Username", Value = "TEST" }
        };
        var listQuery = new EntityPagedQuery<AuditReadModel>(MockPrincipal.Default, entityQuery);

        var listResult = await mediator.Send(listQuery);
        listResult.Should().NotBeNull();

        // Patch Entity
        var patchModel = new JsonPatchDocument();
        patchModel.Operations.Add(new Operation<Audit>
        {
            Op = "replace",
            Path = "/Content",
            Value = "Patch Update"
        });


        var patchCommand = new EntityPatchCommand<string, AuditReadModel>(MockPrincipal.Default, key, patchModel);
        var patchResult = await mediator.Send(patchCommand);
        patchResult.Should().NotBeNull();
        patchResult.Content.Should().Be("Patch Update");

        // Update Entity
        var updateModel = mapper.Map<AuditUpdateModel>(patchResult);
        updateModel.Content = "Update Command";

        var updateCommand = new EntityUpdateCommand<string, AuditUpdateModel, AuditReadModel>(MockPrincipal.Default, key, updateModel);
        var updateResult = await mediator.Send(updateCommand);
        updateResult.Should().NotBeNull();
        updateResult.Content.Should().Be("Update Command");

        // Delete Entity
        var deleteCommand = new EntityDeleteCommand<string, AuditReadModel>(MockPrincipal.Default, key);
        var deleteResult = await mediator.Send(deleteCommand);
        deleteResult.Should().NotBeNull();
        deleteResult.Id.Should().Be(createResult.Id);
    }

    [Test]
    public async Task Upsert()
    {
        var key = ObjectId.GenerateNewId().ToString();

        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Update Entity
        var generator = new Faker<AuditUpdateModel>()
            .RuleFor(p => p.Username, (faker, model) => faker.Internet.UserName())
            .RuleFor(p => p.Content, (faker, model) => faker.Lorem.Paragraph())
            .RuleFor(p => p.Updated, (faker, model) => faker.Date.SoonOffset())
            .RuleFor(p => p.UpdatedBy, (faker, model) => faker.Internet.Email())
            .RuleFor(p => p.Date, (faker, model) => faker.Date.Soon());

        var updateModel = generator.Generate();

        var upsertCommandNew = new EntityUpsertCommand<string, AuditUpdateModel, AuditReadModel>(MockPrincipal.Default, key, updateModel);
        var upsertResultNew = await mediator.Send(upsertCommandNew);
        upsertResultNew.Should().NotBeNull();
        upsertResultNew.Id.Should().Be(key);

        // Get Entity by Key
        var identifierQuery = new EntityIdentifierQuery<string, AuditReadModel>(MockPrincipal.Default, key);
        var identifierResult = await mediator.Send(identifierQuery);
        identifierResult.Should().NotBeNull();
        identifierResult.Username.Should().Be(updateModel.Username);

        // update model
        updateModel.Content = "Update " + DateTime.Now.Ticks;

        // Upsert again, should be update
        var upsertCommandUpdate = new EntityUpsertCommand<string, AuditUpdateModel, AuditReadModel>(MockPrincipal.Default, key, updateModel);
        var upsertResultUpdate = await mediator.Send(upsertCommandUpdate);
        upsertResultUpdate.Should().NotBeNull();
        upsertResultUpdate.Content.Should().NotBe(upsertResultNew.Content);
    }
}
