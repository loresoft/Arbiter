#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantReadModel>>]
internal sealed class TenantToTenantReadModelMapper
    : MapperBase<Entities.Tenant, Models.TenantReadModel>
{
    protected override Expression<Func<Entities.Tenant, Models.TenantReadModel>> CreateMapping()
    {
        return source => new Models.TenantReadModel
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            #endregion

            // Manual Mappings
            Id = source.Id,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantUpdateModel>>]
internal sealed class TenantToTenantUpdateModelMapper
    : MapperBase<Entities.Tenant, Models.TenantUpdateModel>
{
    protected override Expression<Func<Entities.Tenant, Models.TenantUpdateModel>> CreateMapping()
    {
        return source => new Models.TenantUpdateModel
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<Models.TenantCreateModel, Entities.Tenant>>]
internal sealed class TenantCreateModelToTenantMapper
    : MapperBase<Models.TenantCreateModel, Entities.Tenant>
{
    protected override Expression<Func<Models.TenantCreateModel, Entities.Tenant>> CreateMapping()
    {
        return source => new Entities.Tenant
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            #endregion

            // Manual Mappings
            Id = source.Id,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

[RegisterSingleton<IMapper<Models.TenantUpdateModel, Entities.Tenant>>]
internal sealed class TenantUpdateModelToTenantMapper
    : MapperBase<Models.TenantUpdateModel, Entities.Tenant>
{
    protected override Expression<Func<Models.TenantUpdateModel, Entities.Tenant>> CreateMapping()
    {
        return source => new Entities.Tenant
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

