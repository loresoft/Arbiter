#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityReadModel>>]
internal sealed partial class PriorityToPriorityReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Priority, Models.PriorityReadModel>
{
    public override partial void Map(Entities.Priority source, Models.PriorityReadModel destination);

    public override partial IQueryable<Models.PriorityReadModel> ProjectTo(IQueryable<Entities.Priority> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityUpdateModel>>]
internal sealed partial class PriorityToPriorityUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Priority, Models.PriorityUpdateModel>
{
    [MapperIgnoreSource(nameof(Entities.Priority.Id))]
    [MapperIgnoreSource(nameof(Entities.Priority.Created))]
    [MapperIgnoreSource(nameof(Entities.Priority.CreatedBy))]
    public override partial void Map(Entities.Priority source, Models.PriorityUpdateModel destination);

    public override partial IQueryable<Models.PriorityUpdateModel> ProjectTo(IQueryable<Entities.Priority> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityCreateModel, Entities.Priority>>]
internal sealed partial class PriorityCreateModelToPriorityMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.PriorityCreateModel, Entities.Priority>
{
    [MapperIgnoreTarget(nameof(Entities.Priority.RowVersion))]
    public override partial void Map(Models.PriorityCreateModel source, Entities.Priority destination);

    public override partial IQueryable<Entities.Priority> ProjectTo(IQueryable<Models.PriorityCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Entities.Priority>>]
internal sealed partial class PriorityUpdateModelToPriorityMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Entities.Priority>
{
    [MapperIgnoreTarget(nameof(Entities.Priority.Id))]
    [MapperIgnoreTarget(nameof(Entities.Priority.Created))]
    [MapperIgnoreTarget(nameof(Entities.Priority.CreatedBy))]
    public override partial void Map(Models.PriorityUpdateModel source, Entities.Priority destination);

    public override partial IQueryable<Entities.Priority> ProjectTo(IQueryable<Models.PriorityUpdateModel> source);
}

