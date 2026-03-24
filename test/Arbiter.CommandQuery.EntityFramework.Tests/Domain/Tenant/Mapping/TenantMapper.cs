#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantReadModelToTenantCreateModelMapper
    : MapperProfile<Models.TenantReadModel, Models.TenantCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantReadModelToTenantUpdateModelMapper
    : MapperProfile<Models.TenantReadModel, Models.TenantUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantUpdateModelToTenantCreateModelMapper
    : MapperProfile<Models.TenantUpdateModel, Models.TenantCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantToTenantReadModelMapper
    : MapperProfile<Entities.Tenant, Models.TenantReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantToTenantUpdateModelMapper
    : MapperProfile<Entities.Tenant, Models.TenantUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantCreateModelToTenantMapper
    : MapperProfile<Models.TenantCreateModel, Entities.Tenant>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantUpdateModelToTenantMapper
    : MapperProfile<Models.TenantUpdateModel, Entities.Tenant>;

