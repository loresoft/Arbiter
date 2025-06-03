#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskCreateModel>>]
internal sealed partial class TaskReadModelToTaskCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TaskReadModel, Models.TaskCreateModel>
{
    [MapperIgnoreSource(nameof(Models.TaskReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.TaskReadModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.TaskCreateModel.Id))]
    public override partial void Map(Models.TaskReadModel source, Models.TaskCreateModel destination);

    public override partial IQueryable<Models.TaskCreateModel> ProjectTo(IQueryable<Models.TaskReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskUpdateModel>>]
internal sealed partial class TaskReadModelToTaskUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TaskReadModel, Models.TaskUpdateModel>
{
    [MapperIgnoreSource(nameof(Models.TaskReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.TaskReadModel.Created))]
    [MapperIgnoreSource(nameof(Models.TaskReadModel.CreatedBy))]
    public override partial void Map(Models.TaskReadModel source, Models.TaskUpdateModel destination);

    public override partial IQueryable<Models.TaskUpdateModel> ProjectTo(IQueryable<Models.TaskReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskCreateModel>>]
internal sealed partial class TaskUpdateModelToTaskCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Models.TaskCreateModel>
{
    [MapperIgnoreSource(nameof(Models.TaskUpdateModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.TaskCreateModel.Id))]
    [MapperIgnoreTarget(nameof(Models.TaskCreateModel.Created))]
    [MapperIgnoreTarget(nameof(Models.TaskCreateModel.CreatedBy))]
    public override partial void Map(Models.TaskUpdateModel source, Models.TaskCreateModel destination);

    public override partial IQueryable<Models.TaskCreateModel> ProjectTo(IQueryable<Models.TaskUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskReadModel>>]
internal sealed partial class TaskUpdateModelToTaskReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Models.TaskReadModel>
{
    [MapperIgnoreTarget(nameof(Models.TaskReadModel.Id))]
    [MapperIgnoreTarget(nameof(Models.TaskReadModel.Created))]
    [MapperIgnoreTarget(nameof(Models.TaskReadModel.CreatedBy))]
    public override partial void Map(Models.TaskUpdateModel source, Models.TaskReadModel destination);

    public override partial IQueryable<Models.TaskReadModel> ProjectTo(IQueryable<Models.TaskUpdateModel> source);
}

