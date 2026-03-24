#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantToTenantReadModelMapper
    : MapperProfile<Entities.Tenant, Models.TenantReadModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Tenant, Models.TenantReadModel> mapping)
    {
        mapping
            .Property(dest => dest.TaskCount)
            .From(src => src.Tasks.Count);

        mapping
            .Property(dest => dest.Tasks)
            .From(src => src.Tasks.Select(t => t.Title).ToList());
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantToTenantUpdateModelMapper
    : MapperProfile<Entities.Tenant, Models.TenantUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Entities.Tenant, Models.TenantUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantCreateModelToTenantMapper
    : MapperProfile<Models.TenantCreateModel, Entities.Tenant>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TenantCreateModel, Entities.Tenant> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantUpdateModelToTenantMapper
    : MapperProfile<Models.TenantUpdateModel, Entities.Tenant>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TenantUpdateModel, Entities.Tenant> mapping)
    {
        // custom mapping here
    }
}

