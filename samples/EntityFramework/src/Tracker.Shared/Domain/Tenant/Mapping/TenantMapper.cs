#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantReadModelToTenantCreateModelMapper
    : MapperProfile<Models.TenantReadModel, Models.TenantCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TenantReadModel, Models.TenantCreateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantReadModelToTenantUpdateModelMapper
    : MapperProfile<Models.TenantReadModel, Models.TenantUpdateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TenantReadModel, Models.TenantUpdateModel> mapping)
    {
        // custom mapping here
    }
}

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class TenantUpdateModelToTenantCreateModelMapper
    : MapperProfile<Models.TenantUpdateModel, Models.TenantCreateModel>
{
    protected override void ConfigureMapping(MappingBuilder<Models.TenantUpdateModel, Models.TenantCreateModel> mapping)
    {
        // custom mapping here
    }
}

