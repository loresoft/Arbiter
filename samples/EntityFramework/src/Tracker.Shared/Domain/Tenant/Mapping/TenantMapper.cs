#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<M.TenantReadModel, M.TenantCreateModel>>]
internal sealed class TenantReadModelToTenantCreateModelMapper
    : MapperBase<M.TenantReadModel, M.TenantCreateModel>
{
    protected override Expression<Func<M.TenantReadModel, M.TenantCreateModel>> CreateMapping()
    {
        return source => new M.TenantCreateModel
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

[RegisterSingleton<IMapper<M.TenantReadModel, M.TenantUpdateModel>>]
internal sealed class TenantReadModelToTenantUpdateModelMapper
    : MapperBase<M.TenantReadModel, M.TenantUpdateModel>
{
    protected override Expression<Func<M.TenantReadModel, M.TenantUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<M.TenantUpdateModel, M.TenantCreateModel>>]
internal sealed class TenantUpdateModelToTenantCreateModelMapper
    : MapperBase<M.TenantUpdateModel, M.TenantCreateModel>
{
    protected override Expression<Func<M.TenantUpdateModel, M.TenantCreateModel>> CreateMapping()
    {
        return source => new M.TenantCreateModel
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

