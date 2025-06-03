#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantCreateModel>>]
internal sealed partial class TenantReadModelToTenantCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TenantReadModel, Models.TenantCreateModel>
{
    [MapperIgnoreSource(nameof(Models.TenantReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.TenantReadModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.TenantCreateModel.Id))]
    public override partial void Map(Models.TenantReadModel source, Models.TenantCreateModel destination);

    public override partial IQueryable<Models.TenantCreateModel> ProjectTo(IQueryable<Models.TenantReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantUpdateModel>>]
internal sealed partial class TenantReadModelToTenantUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TenantReadModel, Models.TenantUpdateModel>
{
    [MapperIgnoreSource(nameof(Models.TenantReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.TenantReadModel.Created))]
    [MapperIgnoreSource(nameof(Models.TenantReadModel.CreatedBy))]
    public override partial void Map(Models.TenantReadModel source, Models.TenantUpdateModel destination);

    public override partial IQueryable<Models.TenantUpdateModel> ProjectTo(IQueryable<Models.TenantReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantCreateModel>>]
internal sealed partial class TenantUpdateModelToTenantCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Models.TenantCreateModel>
{
    [MapperIgnoreSource(nameof(Models.TenantUpdateModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.TenantCreateModel.Id))]
    [MapperIgnoreTarget(nameof(Models.TenantCreateModel.Created))]
    [MapperIgnoreTarget(nameof(Models.TenantCreateModel.CreatedBy))]
    public override partial void Map(Models.TenantUpdateModel source, Models.TenantCreateModel destination);

    public override partial IQueryable<Models.TenantCreateModel> ProjectTo(IQueryable<Models.TenantUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantReadModel>>]
internal sealed partial class TenantUpdateModelToTenantReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Models.TenantReadModel>
{
    [MapperIgnoreTarget(nameof(Models.TenantReadModel.Id))]
    [MapperIgnoreTarget(nameof(Models.TenantReadModel.Created))]
    [MapperIgnoreTarget(nameof(Models.TenantReadModel.CreatedBy))]
    public override partial void Map(Models.TenantUpdateModel source, Models.TenantReadModel destination);

    public override partial IQueryable<Models.TenantReadModel> ProjectTo(IQueryable<Models.TenantUpdateModel> source);
}

