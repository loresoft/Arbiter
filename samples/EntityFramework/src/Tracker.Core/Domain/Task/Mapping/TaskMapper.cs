#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed partial class TaskToTaskReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskReadModel>
{
    public override partial void Map(Entities.Task source, Models.TaskReadModel destination);

    public override partial IQueryable<Models.TaskReadModel> ProjectTo(IQueryable<Entities.Task> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Task, Models.TaskUpdateModel>>]
internal sealed partial class TaskToTaskUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskUpdateModel>
{
    [MapperIgnoreSource(nameof(Entities.Task.Id))]
    [MapperIgnoreSource(nameof(Entities.Task.Created))]
    [MapperIgnoreSource(nameof(Entities.Task.CreatedBy))]
    public override partial void Map(Entities.Task source, Models.TaskUpdateModel destination);

    public override partial IQueryable<Models.TaskUpdateModel> ProjectTo(IQueryable<Entities.Task> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskCreateModel, Entities.Task>>]
internal sealed partial class TaskCreateModelToTaskMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TaskCreateModel, Entities.Task>
{
    [MapperIgnoreTarget(nameof(Entities.Task.RowVersion))]
    public override partial void Map(Models.TaskCreateModel source, Entities.Task destination);

    public override partial IQueryable<Entities.Task> ProjectTo(IQueryable<Models.TaskCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskUpdateModel, Entities.Task>>]
internal sealed partial class TaskUpdateModelToTaskMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Entities.Task>
{
    [MapperIgnoreTarget(nameof(Entities.Task.Id))]
    [MapperIgnoreTarget(nameof(Entities.Task.Created))]
    [MapperIgnoreTarget(nameof(Entities.Task.CreatedBy))]
    public override partial void Map(Models.TaskUpdateModel source, Entities.Task destination);

    public override partial IQueryable<Entities.Task> ProjectTo(IQueryable<Models.TaskUpdateModel> source);
}

