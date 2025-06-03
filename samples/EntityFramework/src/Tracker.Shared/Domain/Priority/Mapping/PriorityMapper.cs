#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityCreateModel>>]
internal sealed partial class PriorityReadModelToPriorityCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    [MapperIgnoreSource(nameof(Models.PriorityReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.PriorityReadModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.PriorityCreateModel.Id))]
    public override partial void Map(Models.PriorityReadModel source, Models.PriorityCreateModel destination);

    public override partial IQueryable<Models.PriorityCreateModel> ProjectTo(IQueryable<Models.PriorityReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityUpdateModel>>]
internal sealed partial class PriorityReadModelToPriorityUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    [MapperIgnoreSource(nameof(Models.PriorityReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.PriorityReadModel.Created))]
    [MapperIgnoreSource(nameof(Models.PriorityReadModel.CreatedBy))]
    public override partial void Map(Models.PriorityReadModel source, Models.PriorityUpdateModel destination);

    public override partial IQueryable<Models.PriorityUpdateModel> ProjectTo(IQueryable<Models.PriorityReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityCreateModel>>]
internal sealed partial class PriorityUpdateModelToPriorityCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    [MapperIgnoreSource(nameof(Models.PriorityUpdateModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.PriorityCreateModel.Id))]
    [MapperIgnoreTarget(nameof(Models.PriorityCreateModel.Created))]
    [MapperIgnoreTarget(nameof(Models.PriorityCreateModel.CreatedBy))]
    public override partial void Map(Models.PriorityUpdateModel source, Models.PriorityCreateModel destination);

    public override partial IQueryable<Models.PriorityCreateModel> ProjectTo(IQueryable<Models.PriorityUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityReadModel>>]
internal sealed partial class PriorityUpdateModelToPriorityReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Models.PriorityReadModel>
{
    [MapperIgnoreTarget(nameof(Models.PriorityReadModel.Id))]
    [MapperIgnoreTarget(nameof(Models.PriorityReadModel.Created))]
    [MapperIgnoreTarget(nameof(Models.PriorityReadModel.CreatedBy))]
    public override partial void Map(Models.PriorityUpdateModel source, Models.PriorityReadModel destination);

    public override partial IQueryable<Models.PriorityReadModel> ProjectTo(IQueryable<Models.PriorityUpdateModel> source);
}

