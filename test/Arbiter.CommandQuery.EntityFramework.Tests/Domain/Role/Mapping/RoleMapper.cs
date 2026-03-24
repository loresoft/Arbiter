#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleReadModelToRoleCreateModelMapper
    : MapperProfile<Models.RoleReadModel, Models.RoleCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleReadModelToRoleUpdateModelMapper
    : MapperProfile<Models.RoleReadModel, Models.RoleUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleUpdateModelToRoleCreateModelMapper
    : MapperProfile<Models.RoleUpdateModel, Models.RoleCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleToRoleReadModelMapper
    : MapperProfile<Entities.Role, Models.RoleReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleToRoleUpdateModelMapper
    : MapperProfile<Entities.Role, Models.RoleUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleCreateModelToRoleMapper
    : MapperProfile<Models.RoleCreateModel, Entities.Role>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class RoleUpdateModelToRoleMapper
    : MapperProfile<Models.RoleUpdateModel, Entities.Role>;

