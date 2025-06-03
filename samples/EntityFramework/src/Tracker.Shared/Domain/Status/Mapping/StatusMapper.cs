#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusCreateModel>>]
internal sealed partial class StatusReadModelToStatusCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.StatusReadModel, Models.StatusCreateModel>
{
    [MapperIgnoreSource(nameof(Models.StatusReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.StatusReadModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.StatusCreateModel.Id))]
    public override partial void Map(Models.StatusReadModel source, Models.StatusCreateModel destination);

    public override partial IQueryable<Models.StatusCreateModel> ProjectTo(IQueryable<Models.StatusReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusUpdateModel>>]
internal sealed partial class StatusReadModelToStatusUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.StatusReadModel, Models.StatusUpdateModel>
{
    [MapperIgnoreSource(nameof(Models.StatusReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.StatusReadModel.Created))]
    [MapperIgnoreSource(nameof(Models.StatusReadModel.CreatedBy))]
    public override partial void Map(Models.StatusReadModel source, Models.StatusUpdateModel destination);

    public override partial IQueryable<Models.StatusUpdateModel> ProjectTo(IQueryable<Models.StatusReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusCreateModel>>]
internal sealed partial class StatusUpdateModelToStatusCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Models.StatusCreateModel>
{
    [MapperIgnoreSource(nameof(Models.StatusUpdateModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.StatusCreateModel.Id))]
    [MapperIgnoreTarget(nameof(Models.StatusCreateModel.Created))]
    [MapperIgnoreTarget(nameof(Models.StatusCreateModel.CreatedBy))]
    public override partial void Map(Models.StatusUpdateModel source, Models.StatusCreateModel destination);

    public override partial IQueryable<Models.StatusCreateModel> ProjectTo(IQueryable<Models.StatusUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusReadModel>>]
internal sealed partial class StatusUpdateModelToStatusReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Models.StatusReadModel>
{
    [MapperIgnoreTarget(nameof(Models.StatusReadModel.Id))]
    [MapperIgnoreTarget(nameof(Models.StatusReadModel.Created))]
    [MapperIgnoreTarget(nameof(Models.StatusReadModel.CreatedBy))]
    public override partial void Map(Models.StatusUpdateModel source, Models.StatusReadModel destination);

    public override partial IQueryable<Models.StatusReadModel> ProjectTo(IQueryable<Models.StatusUpdateModel> source);
}

