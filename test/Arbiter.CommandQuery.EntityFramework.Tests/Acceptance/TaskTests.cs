using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.EntityFramework.Tests.Constants;
using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;
using Arbiter.CommandQuery.Queries;
using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;

using SystemTextJsonPatch;
using SystemTextJsonPatch.Operations;

using Task = System.Threading.Tasks.Task;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Acceptance;

public class TaskTests : DatabaseTestBase
{
    [Test]
    public async Task FullTest()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Create Entity
        var generator = new Faker<TaskCreateModel>()
            .RuleFor(t => t.TenantId, f => TenantConstants.Test)
            .RuleFor(t => t.StatusId, f => StatusConstants.NotStarted)
            .RuleFor(t => t.PriorityId, f => PriorityConstants.Normal)
            .RuleFor(t => t.Title, f => f.Lorem.Word())
            .RuleFor(t => t.Description, f => f.Lorem.Sentence());

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<TaskCreateModel, TaskReadModel>(MockPrincipal.Default, createModel);
        var createResult = await mediator.Send(createCommand);
        createResult.Should().NotBeNull();

        // check mapping
        createResult.PriorityName.Should().Be("Normal");
        createResult.StatusName.Should().Be("Not Started");

        // Get Entity by Key
        var identifierQuery = new EntityIdentifierQuery<int, TaskReadModel>(MockPrincipal.Default, createResult.Id);
        var identifierResult = await mediator.Send(identifierQuery);
        identifierResult.Should().NotBeNull();
        identifierResult.Title.Should().Be(createModel.Title);
        // check mapping
        identifierResult.PriorityName.Should().Be("Normal");
        identifierResult.StatusName.Should().Be("Not Started");

        // Query Entity
        var entityQuery = new EntityQuery
        {
            Sort = new List<EntitySort> { new EntitySort { Name = "Updated", Direction = "Descending" } },
            Filter = new EntityFilter { Name = "StatusId", Value = StatusConstants.NotStarted }
        };
        var listQuery = new EntityPagedQuery<TaskReadModel>(MockPrincipal.Default, entityQuery);

        var listResult = await mediator.Send(listQuery);
        listResult.Should().NotBeNull();

        // Patch Entity
        var patchModel = new JsonPatchDocument();
        patchModel.Operations.Add(new Operation
        {
            Op = "replace",
            Path = "/Title",
            Value = "Patch Update"
        });

        var patchCommand = new EntityPatchCommand<int, TaskReadModel>(MockPrincipal.Default, createResult.Id, patchModel);
        var patchResult = await mediator.Send(patchCommand);
        patchResult.Should().NotBeNull();
        patchResult.Title.Should().Be("Patch Update");

        // Update Entity
        var updateModel = mapper.Map<TaskReadModel, TaskUpdateModel>(patchResult);
        updateModel.Title = "Update Command";

        var updateCommand = new EntityUpdateCommand<int, TaskUpdateModel, TaskReadModel>(MockPrincipal.Default, createResult.Id, updateModel);
        var updateResult = await mediator.Send(updateCommand);
        updateResult.Should().NotBeNull();
        updateResult.Title.Should().Be("Update Command");

        // Delete Entity
        var deleteCommand = new EntityDeleteCommand<int, TaskReadModel>(MockPrincipal.Default, createResult.Id);
        var deleteResult = await mediator.Send(deleteCommand);
        deleteResult.Should().NotBeNull();
        deleteResult.Id.Should().Be(createResult.Id);
    }

    [Test]
    public async Task Upsert()
    {
        var key = 0;
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Update Entity
        var generator = new Faker<TaskUpdateModel>()
            .RuleFor(t => t.TenantId, f => TenantConstants.Test)
            .RuleFor(t => t.StatusId, f => StatusConstants.NotStarted)
            .RuleFor(t => t.PriorityId, f => PriorityConstants.Normal)
            .RuleFor(t => t.Title, f => f.Lorem.Word())
            .RuleFor(t => t.Description, f => f.Lorem.Sentence());

        var updateModel = generator.Generate();

        var upsertCommandNew = new EntityUpsertCommand<int, TaskUpdateModel, TaskReadModel>(MockPrincipal.Default, key, updateModel);
        var upsertResultNew = await mediator.Send(upsertCommandNew);
        upsertResultNew.Should().NotBeNull();

        key = upsertResultNew.Id;

        // Get Entity by Key
        var identifierQuery = new EntityIdentifierQuery<int, TaskReadModel>(MockPrincipal.Default, key);
        var identifierResult = await mediator.Send(identifierQuery);
        identifierResult.Should().NotBeNull();
        identifierResult.Title.Should().Be(updateModel.Title);

        // update model
        updateModel.Description = "Update " + DateTime.Now.Ticks;

        // Upsert again, should be update
        var upsertCommandUpdate = new EntityUpsertCommand<int, TaskUpdateModel, TaskReadModel>(MockPrincipal.Default, key, updateModel);
        var upsertResultUpdate = await mediator.Send(upsertCommandUpdate);
        upsertResultUpdate.Should().NotBeNull();
        upsertResultUpdate.Description.Should().NotBe(upsertResultNew.Description);
    }

    [Test]
    public async Task TenantDoesNotMatch()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Create Entity
        var generator = new Faker<TaskCreateModel>()
            .RuleFor(t => t.TenantId, f => 100)
            .RuleFor(t => t.StatusId, f => StatusConstants.NotStarted)
            .RuleFor(t => t.PriorityId, f => PriorityConstants.Normal)
            .RuleFor(t => t.Title, f => f.Lorem.Word())
            .RuleFor(t => t.Description, f => f.Lorem.Sentence());

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<TaskCreateModel, TaskReadModel>(MockPrincipal.Default, createModel);
        await Assert.ThrowsAsync<DomainException>(() => mediator.Send(createCommand).AsTask());
    }

    [Test]
    public async Task TenantSetDefault()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        // Create Entity
        var generator = new Faker<TaskCreateModel>()
            .RuleFor(t => t.StatusId, f => StatusConstants.NotStarted)
            .RuleFor(t => t.PriorityId, f => PriorityConstants.Normal)
            .RuleFor(t => t.Title, f => f.Lorem.Word())
            .RuleFor(t => t.Description, f => f.Lorem.Sentence());

        var createModel = generator.Generate();

        var createCommand = new EntityCreateCommand<TaskCreateModel, TaskReadModel>(MockPrincipal.Default, createModel);
        var createResult = await mediator.Send(createCommand);

        createResult.Should().NotBeNull();
        createResult.TenantId.Should().Be(TenantConstants.Test);
    }

    [Test]
    public async Task EntityPageQuery()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var filter = new EntityFilter { Name = "StatusId", Value = StatusConstants.NotStarted };
        var entityQuery = new EntityQuery { Filter = filter };
        var pagedQuery = new EntityPagedQuery<TaskReadModel>(MockPrincipal.Default, entityQuery);

        var selectResult = await mediator.Send(pagedQuery);
        selectResult.Should().NotBeNull();
    }

    [Test]
    public async Task EntitySelectQuery()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var filter = new EntityFilter { Name = "StatusId", Value = StatusConstants.NotStarted };
        var select = new EntitySelect(filter);
        var selectQuery = new EntitySelectQuery<TaskReadModel>(MockPrincipal.Default, select);

        var selectResult = await mediator.Send(selectQuery);
        selectResult.Should().NotBeNull();
    }

    [Test]
    public async Task EntitySelectQueryDelete()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var filter = new EntityFilter { Name = "IsDeleted", Value = true };
        var select = new EntitySelect(filter);
        var selectQuery = new EntitySelectQuery<TaskReadModel>(MockPrincipal.Default, select);

        var selectResult = await mediator.Send(selectQuery);
        selectResult.Should().NotBeNull();
    }

    [Test]
    public async Task EntitySelectQueryDeleteNested()
    {
        var mediator = ServiceProvider.GetService<IMediator>();
        mediator.Should().NotBeNull();

        var mapper = ServiceProvider.GetService<IMapper>();
        mapper.Should().NotBeNull();

        var filter = new EntityFilter
        {
            Filters = new List<EntityFilter>
            {
                new EntityFilter {Name = "IsDeleted", Value = true},
                new EntityFilter { Name = "StatusId", Value = StatusConstants.NotStarted }
            }
        };


        var select = new EntitySelect(filter);
        var selectQuery = new EntitySelectQuery<TaskReadModel>(MockPrincipal.Default, select);

        var selectResult = await mediator.Send(selectQuery);
        selectResult.Should().NotBeNull();
    }
}
