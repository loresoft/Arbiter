#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using E = Tracker.Data.Entities;
using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<E.Status, M.StatusReadModel>>]
internal sealed class StatusToStatusReadModelMapper
    : MapperBase<E.Status, M.StatusReadModel>
{
    protected override Expression<Func<E.Status, M.StatusReadModel>> CreateMapping()
    {
        return source => new M.StatusReadModel
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
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<E.Status, M.StatusUpdateModel>>]
internal sealed class StatusToStatusUpdateModelMapper
    : MapperBase<E.Status, M.StatusUpdateModel>
{
    protected override Expression<Func<E.Status, M.StatusUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<M.StatusCreateModel, E.Status>>]
internal sealed class StatusCreateModelToStatusMapper
    : MapperBase<M.StatusCreateModel, E.Status>
{
    protected override Expression<Func<M.StatusCreateModel, E.Status>> CreateMapping()
    {
        return source => new E.Status
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

[RegisterSingleton<IMapper<M.StatusUpdateModel, E.Status>>]
internal sealed class StatusUpdateModelToStatusMapper
    : MapperBase<M.StatusUpdateModel, E.Status>
{
    protected override Expression<Func<M.StatusUpdateModel, E.Status>> CreateMapping()
    {
        return source => new E.Status
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

