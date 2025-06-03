#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.Status, Models.StatusReadModel>>]
internal sealed partial class StatusToStatusReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Status, Models.StatusReadModel>
{
    public override partial void Map(Entities.Status source, Models.StatusReadModel destination);

    public override partial IQueryable<Models.StatusReadModel> ProjectTo(IQueryable<Entities.Status> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Status, Models.StatusUpdateModel>>]
internal sealed partial class StatusToStatusUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Status, Models.StatusUpdateModel>
{
    [MapperIgnoreSource(nameof(Entities.Status.Id))]
    [MapperIgnoreSource(nameof(Entities.Status.Created))]
    [MapperIgnoreSource(nameof(Entities.Status.CreatedBy))]
    public override partial void Map(Entities.Status source, Models.StatusUpdateModel destination);

    public override partial IQueryable<Models.StatusUpdateModel> ProjectTo(IQueryable<Entities.Status> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusCreateModel, Entities.Status>>]
internal sealed partial class StatusCreateModelToStatusMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.StatusCreateModel, Entities.Status>
{
    [MapperIgnoreTarget(nameof(Entities.Status.RowVersion))]
    public override partial void Map(Models.StatusCreateModel source, Entities.Status destination);

    public override partial IQueryable<Entities.Status> ProjectTo(IQueryable<Models.StatusCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusUpdateModel, Entities.Status>>]
internal sealed partial class StatusUpdateModelToStatusMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Entities.Status>
{
    [MapperIgnoreTarget(nameof(Entities.Status.Id))]
    [MapperIgnoreTarget(nameof(Entities.Status.Created))]
    [MapperIgnoreTarget(nameof(Entities.Status.CreatedBy))]
    public override partial void Map(Models.StatusUpdateModel source, Entities.Status destination);

    public override partial IQueryable<Entities.Status> ProjectTo(IQueryable<Models.StatusUpdateModel> source);
}

