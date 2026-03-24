#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusReadModelToStatusCreateModelMapper
    : MapperProfile<Models.StatusReadModel, Models.StatusCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.StatusReadModel, Models.StatusCreateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusReadModelToStatusUpdateModelMapper
    : MapperProfile<Models.StatusReadModel, Models.StatusUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.StatusReadModel, Models.StatusUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class StatusUpdateModelToStatusCreateModelMapper
    : MapperProfile<Models.StatusUpdateModel, Models.StatusCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.StatusUpdateModel, Models.StatusCreateModel> mapping)
    {
        // custom mapping here
    }
}

