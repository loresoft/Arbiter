#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusReadModelToStatusCreateModelMapper
    : MapperProfile<Models.StatusReadModel, Models.StatusCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusReadModelToStatusUpdateModelMapper
    : MapperProfile<Models.StatusReadModel, Models.StatusUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusUpdateModelToStatusCreateModelMapper
    : MapperProfile<Models.StatusUpdateModel, Models.StatusCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusToStatusReadModelMapper
    : MapperProfile<Entities.Status, Models.StatusReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusToStatusUpdateModelMapper
    : MapperProfile<Entities.Status, Models.StatusUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusCreateModelToStatusMapper
    : MapperProfile<Models.StatusCreateModel, Entities.Status>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusUpdateModelToStatusMapper
    : MapperProfile<Models.StatusUpdateModel, Entities.Status>;

