#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityReadModelToPriorityCreateModelMapper
    : MapperProfile<Models.PriorityReadModel, Models.PriorityCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityReadModelToPriorityUpdateModelMapper
    : MapperProfile<Models.PriorityReadModel, Models.PriorityUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityUpdateModelToPriorityCreateModelMapper
    : MapperProfile<Models.PriorityUpdateModel, Models.PriorityCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityToPriorityReadModelMapper
    : MapperProfile<Entities.Priority, Models.PriorityReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityToPriorityUpdateModelMapper
    : MapperProfile<Entities.Priority, Models.PriorityUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityCreateModelToPriorityMapper
    : MapperProfile<Models.PriorityCreateModel, Entities.Priority>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityUpdateModelToPriorityMapper
    : MapperProfile<Models.PriorityUpdateModel, Entities.Priority>;

