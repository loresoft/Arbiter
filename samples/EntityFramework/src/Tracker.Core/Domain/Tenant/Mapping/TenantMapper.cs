#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantReadModel>>]
internal sealed partial class TenantToTenantReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Tenant, Models.TenantReadModel>
{
    public override partial void Map(Entities.Tenant source, Models.TenantReadModel destination);

    public override partial IQueryable<Models.TenantReadModel> ProjectTo(IQueryable<Entities.Tenant> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantUpdateModel>>]
internal sealed partial class TenantToTenantUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.Tenant, Models.TenantUpdateModel>
{
    [MapperIgnoreSource(nameof(Entities.Tenant.Id))]
    [MapperIgnoreSource(nameof(Entities.Tenant.Created))]
    [MapperIgnoreSource(nameof(Entities.Tenant.CreatedBy))]
    public override partial void Map(Entities.Tenant source, Models.TenantUpdateModel destination);

    public override partial IQueryable<Models.TenantUpdateModel> ProjectTo(IQueryable<Entities.Tenant> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantCreateModel, Entities.Tenant>>]
internal sealed partial class TenantCreateModelToTenantMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TenantCreateModel, Entities.Tenant>
{
    [MapperIgnoreTarget(nameof(Entities.Tenant.RowVersion))]
    public override partial void Map(Models.TenantCreateModel source, Entities.Tenant destination);

    public override partial IQueryable<Entities.Tenant> ProjectTo(IQueryable<Models.TenantCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantUpdateModel, Entities.Tenant>>]
internal sealed partial class TenantUpdateModelToTenantMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Entities.Tenant>
{
    [MapperIgnoreTarget(nameof(Entities.Tenant.Id))]
    [MapperIgnoreTarget(nameof(Entities.Tenant.Created))]
    [MapperIgnoreTarget(nameof(Entities.Tenant.CreatedBy))]
    public override partial void Map(Models.TenantUpdateModel source, Entities.Tenant destination);

    public override partial IQueryable<Entities.Tenant> ProjectTo(IQueryable<Models.TenantUpdateModel> source);
}

