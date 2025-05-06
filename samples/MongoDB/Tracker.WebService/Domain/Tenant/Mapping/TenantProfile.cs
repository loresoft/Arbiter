#pragma warning disable IDE0130 // Namespace does not match folder structure

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Mapping;

public partial class TenantProfile
    : AutoMapper.Profile
{
    public TenantProfile()
    {
        CreateMap<Data.Entities.Tenant, TenantReadModel>();
        CreateMap<TenantCreateModel, Data.Entities.Tenant>();
        CreateMap<Data.Entities.Tenant, TenantUpdateModel>();
        CreateMap<TenantUpdateModel, Data.Entities.Tenant>();
    }

}
