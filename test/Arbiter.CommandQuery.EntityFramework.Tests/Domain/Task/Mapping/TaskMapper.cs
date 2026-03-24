#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

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
    : MapperProfile<Entities.Task, Models.TaskReadModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Task, Models.TaskReadModel> mapping)
    {
        mapping
            .Property(dest => dest.StatusName)
            .From(src => src.Status.Name);
        mapping
            .Property(dest => dest.PriorityName)
            .From(src => src.Priority!.Name);
        mapping
            .Property(dest => dest.AssignedName)
            .From(src => src.AssignedUser!.EmailAddress);
        mapping
            .Property(dest => dest.TenantName)
            .From(src => src.Tenant.Name);
    }
}

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

