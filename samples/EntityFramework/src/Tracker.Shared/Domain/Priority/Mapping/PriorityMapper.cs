#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityReadModelToPriorityCreateModelMapper
    : MapperProfile<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.PriorityReadModel, Models.PriorityCreateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityReadModelToPriorityUpdateModelMapper
    : MapperProfile<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.PriorityReadModel, Models.PriorityUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityUpdateModelToPriorityCreateModelMapper
    : MapperProfile<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.PriorityUpdateModel, Models.PriorityCreateModel> mapping)
    {
        // custom mapping here
    }
}

