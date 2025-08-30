#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantCreateModel>>]
internal sealed class TenantReadModelToTenantCreateModelMapper
    : MapperBase<Models.TenantReadModel, Models.TenantCreateModel>
{
    protected override Expression<Func<Models.TenantReadModel, Models.TenantCreateModel>> CreateMapping()
    {
        return source => new Models.TenantCreateModel
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

[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantUpdateModel>>]
internal sealed class TenantReadModelToTenantUpdateModelMapper
    : MapperBase<Models.TenantReadModel, Models.TenantUpdateModel>
{
    protected override Expression<Func<Models.TenantReadModel, Models.TenantUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantCreateModel>>]
internal sealed class TenantUpdateModelToTenantCreateModelMapper
    : MapperBase<Models.TenantUpdateModel, Models.TenantCreateModel>
{
    protected override Expression<Func<Models.TenantUpdateModel, Models.TenantCreateModel>> CreateMapping()
    {
        return source => new Models.TenantCreateModel
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

