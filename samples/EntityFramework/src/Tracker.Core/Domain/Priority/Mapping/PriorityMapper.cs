#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityToPriorityReadModelMapper
    : MapperProfile<Entities.Priority, Models.PriorityReadModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Priority, Models.PriorityReadModel> mapping)
    {
        mapping
            .Property(dest => dest.TaskCount)
            .From(src => src.Tasks.Count(t => !t.IsDeleted));
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityToPriorityUpdateModelMapper
    : MapperProfile<Entities.Priority, Models.PriorityUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Priority, Models.PriorityUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityCreateModelToPriorityMapper
    : MapperProfile<Models.PriorityCreateModel, Entities.Priority>
{
    protected override void ConfigureMapping(MappingBuilder<Models.PriorityCreateModel, Entities.Priority> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityUpdateModelToPriorityMapper
    : MapperProfile<Models.PriorityUpdateModel, Entities.Priority>
{
    protected override void ConfigureMapping(MappingBuilder<Models.PriorityUpdateModel, Entities.Priority> mapping)
    {
        // custom mapping here
    }
}

