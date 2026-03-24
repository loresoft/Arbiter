#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusToStatusReadModelMapper
    : MapperProfile<Entities.Status, Models.StatusReadModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Status, Models.StatusReadModel> mapping)
    {
        mapping
            .Property(dest => dest.TaskCount)
            .From(src => src.Tasks.Count);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusToStatusUpdateModelMapper
    : MapperProfile<Entities.Status, Models.StatusUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Status, Models.StatusUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusCreateModelToStatusMapper
    : MapperProfile<Models.StatusCreateModel, Entities.Status>
{
    protected override void ConfigureMapping(MappingBuilder<Models.StatusCreateModel, Entities.Status> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusUpdateModelToStatusMapper
    : MapperProfile<Models.StatusUpdateModel, Entities.Status>
{
    protected override void ConfigureMapping(MappingBuilder<Models.StatusUpdateModel, Entities.Status> mapping)
    {
        // custom mapping here
    }
}

