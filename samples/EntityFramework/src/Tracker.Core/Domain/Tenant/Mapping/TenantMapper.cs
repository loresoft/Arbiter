#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using E = Tracker.Data.Entities;
using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<E.Tenant, M.TenantReadModel>>]
internal sealed class TenantToTenantReadModelMapper
    : MapperBase<E.Tenant, M.TenantReadModel>
{
    protected override Expression<Func<E.Tenant, M.TenantReadModel>> CreateMapping()
    {
        return source => new M.TenantReadModel
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

[RegisterSingleton<IMapper<E.Tenant, M.TenantUpdateModel>>]
internal sealed class TenantToTenantUpdateModelMapper
    : MapperBase<E.Tenant, M.TenantUpdateModel>
{
    protected override Expression<Func<E.Tenant, M.TenantUpdateModel>> CreateMapping()
    {
        return source => new M.TenantUpdateModel
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

[RegisterSingleton<IMapper<M.TenantCreateModel, E.Tenant>>]
internal sealed class TenantCreateModelToTenantMapper
    : MapperBase<M.TenantCreateModel, E.Tenant>
{
    protected override Expression<Func<M.TenantCreateModel, E.Tenant>> CreateMapping()
    {
        return source => new E.Tenant
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

[RegisterSingleton<IMapper<M.TenantUpdateModel, E.Tenant>>]
internal sealed class TenantUpdateModelToTenantMapper
    : MapperBase<M.TenantUpdateModel, E.Tenant>
{
    protected override Expression<Func<M.TenantUpdateModel, E.Tenant>> CreateMapping()
    {
        return source => new E.Tenant
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

