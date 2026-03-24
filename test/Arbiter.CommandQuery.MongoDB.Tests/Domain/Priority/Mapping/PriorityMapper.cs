#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;
using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityReadModelToPriorityCreateModelMapper
    : MapperProfile<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<PriorityReadModel, PriorityCreateModel> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityReadModelToPriorityUpdateModelMapper
    : MapperProfile<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<PriorityReadModel, PriorityUpdateModel> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityUpdateModelToPriorityCreateModelMapper
    : MapperProfile<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<PriorityUpdateModel, PriorityCreateModel> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityToPriorityReadModelMapper
    : MapperProfile<Entities.Priority, Models.PriorityReadModel>

{
    protected override void ConfigureMapping(MappingBuilder<Entities.Priority, PriorityReadModel> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityToPriorityUpdateModelMapper
    : MapperProfile<Entities.Priority, Models.PriorityUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Priority, PriorityUpdateModel> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityCreateModelToPriorityMapper
    : MapperProfile<Models.PriorityCreateModel, Entities.Priority>
{
    protected override void ConfigureMapping(MappingBuilder<PriorityCreateModel, Entities.Priority> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class PriorityUpdateModelToPriorityMapper
    : MapperProfile<Models.PriorityUpdateModel, Entities.Priority>
{
    protected override void ConfigureMapping(MappingBuilder<PriorityUpdateModel, Entities.Priority> mapping)
    {
        // custom mapping here, if needed
        // mapping.Property(p => p.Id).From(p => p.Id);
    }
}

