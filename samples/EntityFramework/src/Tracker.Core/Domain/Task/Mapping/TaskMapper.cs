#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskToTaskReadModelMapper
    : MapperProfile<Entities.Task, Models.TaskReadModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Task, Models.TaskReadModel> mapping)
    {
        mapping
            .Property(dest => dest.TenantName)
            .From(src => src.Tenant.Name);

        mapping
            .Property(dest => dest.StatusName)
            .From(src => src.Status.Name);

        mapping
            .Property(dest => dest.PriorityName)
            .From(src => src.Priority!.Name);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskToTaskUpdateModelMapper
    : MapperProfile<Entities.Task, Models.TaskUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Task, Models.TaskUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskCreateModelToTaskMapper
    : MapperProfile<Models.TaskCreateModel, Entities.Task>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TaskCreateModel, Entities.Task> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskUpdateModelToTaskMapper
    : MapperProfile<Models.TaskUpdateModel, Entities.Task>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TaskUpdateModel, Entities.Task> mapping)
    {
        // custom mapping here
    }
}

