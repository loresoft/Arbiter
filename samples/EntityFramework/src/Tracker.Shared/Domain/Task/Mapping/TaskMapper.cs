#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskReadModelToTaskCreateModelMapper
    : MapperProfile<Models.TaskReadModel, Models.TaskCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TaskReadModel, Models.TaskCreateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskReadModelToTaskUpdateModelMapper
    : MapperProfile<Models.TaskReadModel, Models.TaskUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TaskReadModel, Models.TaskUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TaskUpdateModelToTaskCreateModelMapper
    : MapperProfile<Models.TaskUpdateModel, Models.TaskCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TaskUpdateModel, Models.TaskCreateModel> mapping)
    {
        // custom mapping here
    }
}

