#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditReadModelToAuditCreateModelMapper
    : MapperProfile<Models.AuditReadModel, Models.AuditCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditReadModelToAuditUpdateModelMapper
    : MapperProfile<Models.AuditReadModel, Models.AuditUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditUpdateModelToAuditCreateModelMapper
    : MapperProfile<Models.AuditUpdateModel, Models.AuditCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditToAuditReadModelMapper
    : MapperProfile<Entities.Audit, Models.AuditReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditToAuditUpdateModelMapper
    : MapperProfile<Entities.Audit, Models.AuditUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditCreateModelToAuditMapper
    : MapperProfile<Models.AuditCreateModel, Entities.Audit>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class AuditUpdateModelToAuditMapper
    : MapperProfile<Models.AuditUpdateModel, Entities.Audit>;

