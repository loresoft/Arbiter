using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;
using Arbiter.CommandQuery.EntityFramework.Tests.Pipeline;
using Arbiter.CommandQuery.Notifications;
using Arbiter.Mediation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using SystemTextJsonPatch;
using SystemTextJsonPatch.Operations;

using Task = System.Threading.Tasks.Task;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Acceptance;

public class EntityModifierTests : DatabaseTestBase
{
    [Test]
    public async Task CreateAppliesDefaultModifiersInPriorityOrder()
    {
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var recorder = ServiceProvider.GetRequiredService<AuditModifierRecorder>();

        var marker = AuditModifierRecorder.CreateMarker();
        var createModel = CreateModel(marker);

        var createCommand = new EntityCreateCommand<AuditCreateModel, AuditReadModel>(MockPrincipal.Default, createModel);
        var createResult = await mediator.Send(createCommand);

        createResult.Should().NotBeNull();
        createResult.Content.Should().Be("Create [Created]");

        var invocations = recorder.GetInvocations(marker);
        invocations.Select(i => i.Modifier).Should().Equal(AuditFirstModifier.Name, AuditDefaultModifier.Name);
        invocations.Should().AllSatisfy(i =>
        {
            i.Operation.Should().Be(EntityChangeOperation.Created);
            i.State.Should().Be(EntityState.Added);
            i.ActivatedBy.Should().Be(createCommand.ActivatedBy);
            i.Principal.Should().BeSameAs(MockPrincipal.Default);
        });

        // related entity added by the modifier is saved with the created entity
        var childExists = await AuditExists(marker + AuditModifierRecorder.ChildSuffix);
        childExists.Should().BeTrue();
    }

    [Test]
    public async Task UpdateAppliesModifiersWithUpdatedOperation()
    {
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var recorder = ServiceProvider.GetRequiredService<AuditModifierRecorder>();

        var marker = AuditModifierRecorder.CreateMarker();
        var createResult = await CreateAudit(mediator, marker);

        var updateModel = UpdateModel(marker);
        var updateCommand = new EntityUpdateCommand<int, AuditUpdateModel, AuditReadModel>(MockPrincipal.Default, createResult.Id, updateModel);
        var updateResult = await mediator.Send(updateCommand);

        updateResult.Should().NotBeNull();
        updateResult.Content.Should().Be("Update [Updated]");

        var invocations = recorder.GetInvocations(marker)
            .Where(i => i.Operation == EntityChangeOperation.Updated)
            .ToList();

        invocations.Select(i => i.Modifier).Should().Equal(AuditFirstModifier.Name, AuditDefaultModifier.Name);
    }

    [Test]
    public async Task UpsertInsertAppliesModifiersWithCreatedOperation()
    {
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var recorder = ServiceProvider.GetRequiredService<AuditModifierRecorder>();

        var marker = AuditModifierRecorder.CreateMarker();
        var updateModel = UpdateModel(marker);

        var upsertCommand = new EntityUpdateCommand<int, AuditUpdateModel, AuditReadModel>(MockPrincipal.Default, 0, updateModel, upsert: true);
        var upsertResult = await mediator.Send(upsertCommand);

        upsertResult.Should().NotBeNull();
        upsertResult.Content.Should().Be("Update [Created]");

        var invocations = recorder.GetInvocations(marker);
        invocations.Select(i => i.Modifier).Should().Equal(AuditFirstModifier.Name, AuditDefaultModifier.Name);
        invocations.Should().AllSatisfy(i =>
        {
            i.Operation.Should().Be(EntityChangeOperation.Created);
            i.State.Should().Be(EntityState.Added);
        });

        var childExists = await AuditExists(marker + AuditModifierRecorder.ChildSuffix);
        childExists.Should().BeTrue();

        // upsert of an existing entity is an update
        var upsertUpdateCommand = new EntityUpdateCommand<int, AuditUpdateModel, AuditReadModel>(MockPrincipal.Default, upsertResult.Id, updateModel, upsert: true);
        var upsertUpdateResult = await mediator.Send(upsertUpdateCommand);

        upsertUpdateResult.Should().NotBeNull();
        upsertUpdateResult.Content.Should().Be("Update [Updated]");
    }

    [Test]
    public async Task PatchAppliesModifiersWithUpdatedOperation()
    {
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var recorder = ServiceProvider.GetRequiredService<AuditModifierRecorder>();

        var marker = AuditModifierRecorder.CreateMarker();
        var createResult = await CreateAudit(mediator, marker);

        var patchOperation = new Operation
        {
            Op = "replace",
            Path = "/Content",
            Value = "Patch",
        };

        var patchModel = new JsonPatchDocument();
        patchModel.Operations.Add(patchOperation);

        var patchCommand = new EntityPatchCommand<int, AuditReadModel>(MockPrincipal.Default, createResult.Id, patchModel);
        var patchResult = await mediator.Send(patchCommand);

        patchResult.Should().NotBeNull();
        patchResult.Content.Should().Be("Patch [Updated]");

        var invocations = recorder.GetInvocations(marker)
            .Where(i => i.Operation == EntityChangeOperation.Updated)
            .ToList();

        invocations.Select(i => i.Modifier).Should().Equal(AuditFirstModifier.Name, AuditDefaultModifier.Name);
    }

    [Test]
    public async Task DeleteAppliesModifiersWithDeletedOperation()
    {
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var recorder = ServiceProvider.GetRequiredService<AuditModifierRecorder>();

        var marker = AuditModifierRecorder.CreateMarker();
        var createResult = await CreateAudit(mediator, marker);

        var deleteCommand = new EntityDeleteCommand<int, AuditReadModel>(MockPrincipal.Default, createResult.Id);
        var deleteResult = await mediator.Send(deleteCommand);

        deleteResult.Should().NotBeNull();

        var invocations = recorder.GetInvocations(marker)
            .Where(i => i.Operation == EntityChangeOperation.Deleted)
            .ToList();

        invocations.Select(i => i.Modifier).Should().Equal(AuditFirstModifier.Name, AuditDefaultModifier.Name);

        var exists = await AuditExists(marker);
        exists.Should().BeFalse();
    }

    [Test]
    public async Task NamedPipelineAppliesOnlyKeyedModifiers()
    {
        var mediator = ServiceProvider.GetRequiredService<IMediator>();
        var recorder = ServiceProvider.GetRequiredService<AuditModifierRecorder>();

        var marker = AuditModifierRecorder.CreateMarker();
        var createModel = CreateModel(marker);

        var createCommand = new EntityCreateCommand<AuditCreateModel, AuditReadModel>(MockPrincipal.Default, createModel, AuditModifierRecorder.PipelineName);
        var createResult = await mediator.Send(createCommand);

        createResult.Should().NotBeNull();
        createResult.Content.Should().Be("Create [Named]");

        var invocations = recorder.GetInvocations(marker);
        invocations.Should().ContainSingle();
        invocations[0].Modifier.Should().Be(AuditNamedModifier.Name);
        invocations[0].Operation.Should().Be(EntityChangeOperation.Created);
        invocations[0].Principal.Should().BeSameAs(MockPrincipal.Default);

        var childExists = await AuditExists(marker + AuditModifierRecorder.ChildSuffix);
        childExists.Should().BeFalse();
    }

    private static AuditCreateModel CreateModel(string marker)
    {
        return new AuditCreateModel
        {
            Date = DateTime.UtcNow,
            Content = "Create",
            Username = marker,
        };
    }

    private static AuditUpdateModel UpdateModel(string marker)
    {
        return new AuditUpdateModel
        {
            Date = DateTime.UtcNow,
            Content = "Update",
            Username = marker,
        };
    }

    private static async Task<AuditReadModel> CreateAudit(IMediator mediator, string marker)
    {
        var createModel = CreateModel(marker);
        var createCommand = new EntityCreateCommand<AuditCreateModel, AuditReadModel>(MockPrincipal.Default, createModel);

        var createResult = await mediator.Send(createCommand);
        createResult.Should().NotBeNull();

        return createResult!;
    }

    private async Task<bool> AuditExists(string username)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<TrackerContext>();

        return await dataContext.Audits.AnyAsync(a => a.Username == username);
    }
}
