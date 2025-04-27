#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using System;
using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Injectio.Attributes;
using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantReadModel>>]
internal sealed partial class TenantToTenantReadModelMapper : IMapper<Entities.Tenant, Models.TenantReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TenantReadModel? Map(Entities.Tenant? source);

    public partial void Map(Entities.Tenant source, Models.TenantReadModel destination);

    public partial IQueryable<Models.TenantReadModel> ProjectTo(IQueryable<Entities.Tenant> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantUpdateModel>>]
internal sealed partial class TenantToTenantUpdateModelMapper : IMapper<Entities.Tenant, Models.TenantUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TenantUpdateModel? Map(Entities.Tenant? source);

    public partial void Map(Entities.Tenant source, Models.TenantUpdateModel destination);

    public partial IQueryable<Models.TenantUpdateModel> ProjectTo(IQueryable<Entities.Tenant> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantCreateModel, Entities.Tenant>>]
internal sealed partial class TenantCreateModelToTenantMapper : IMapper<Models.TenantCreateModel, Entities.Tenant>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Tenant? Map(Models.TenantCreateModel? source);

    public partial void Map(Models.TenantCreateModel source, Entities.Tenant destination);

    public partial IQueryable<Entities.Tenant> ProjectTo(IQueryable<Models.TenantCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantUpdateModel, Entities.Tenant>>]
internal sealed partial class TenantUpdateModelToTenantMapper : IMapper<Models.TenantUpdateModel, Entities.Tenant>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Tenant? Map(Models.TenantUpdateModel? source);

    public partial void Map(Models.TenantUpdateModel source, Entities.Tenant destination);

    public partial IQueryable<Entities.Tenant> ProjectTo(IQueryable<Models.TenantUpdateModel> source);
}

