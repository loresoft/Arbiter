#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<M.StatusReadModel, M.StatusCreateModel>>]
internal sealed class StatusReadModelToStatusCreateModelMapper
    : MapperBase<M.StatusReadModel, M.StatusCreateModel>
{
    protected override Expression<Func<M.StatusReadModel, M.StatusCreateModel>> CreateMapping()
    {
        return source => new M.StatusCreateModel
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
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

[RegisterSingleton<IMapper<M.StatusReadModel, M.StatusUpdateModel>>]
internal sealed class StatusReadModelToStatusUpdateModelMapper
    : MapperBase<M.StatusReadModel, M.StatusUpdateModel>
{
    protected override Expression<Func<M.StatusReadModel, M.StatusUpdateModel>> CreateMapping()
    {
        return source => new M.StatusUpdateModel
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<M.StatusUpdateModel, M.StatusCreateModel>>]
internal sealed class StatusUpdateModelToStatusCreateModelMapper
    : MapperBase<M.StatusUpdateModel, M.StatusCreateModel>
{
    protected override Expression<Func<M.StatusUpdateModel, M.StatusCreateModel>> CreateMapping()
    {
        return source => new M.StatusCreateModel
        {
            #region Generated Mappings
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

