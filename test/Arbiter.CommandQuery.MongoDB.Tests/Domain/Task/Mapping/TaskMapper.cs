#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskReadModelToTaskCreateModelMapper
    : MapperProfile<Models.TaskReadModel, Models.TaskCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskReadModelToTaskUpdateModelMapper
    : MapperProfile<Models.TaskReadModel, Models.TaskUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskUpdateModelToTaskCreateModelMapper
    : MapperProfile<Models.TaskUpdateModel, Models.TaskCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskToTaskReadModelMapper
    : MapperProfile<Entities.Task, Models.TaskReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskToTaskNameModelMapper
    : MapperProfile<Entities.Task, Models.TaskNameModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskToTaskUpdateModelMapper
    : MapperProfile<Entities.Task, Models.TaskUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskCreateModelToTaskMapper
    : MapperProfile<Models.TaskCreateModel, Entities.Task>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskUpdateModelToTaskMapper
    : MapperProfile<Models.TaskUpdateModel, Entities.Task>;

