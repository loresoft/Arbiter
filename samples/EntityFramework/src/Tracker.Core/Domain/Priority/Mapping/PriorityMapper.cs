#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using E = Tracker.Data.Entities;
using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<E.Priority, M.PriorityReadModel>>]
internal sealed class PriorityToPriorityReadModelMapper
    : MapperBase<E.Priority, M.PriorityReadModel>
{
    protected override Expression<Func<E.Priority, M.PriorityReadModel>> CreateMapping()
    {
        return source => new M.PriorityReadModel
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

[RegisterSingleton<IMapper<E.Priority, M.PriorityUpdateModel>>]
internal sealed class PriorityToPriorityUpdateModelMapper
    : MapperBase<E.Priority, M.PriorityUpdateModel>
{
    protected override Expression<Func<E.Priority, M.PriorityUpdateModel>> CreateMapping()
    {
        return source => new M.PriorityUpdateModel
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

[RegisterSingleton<IMapper<M.PriorityCreateModel, E.Priority>>]
internal sealed class PriorityCreateModelToPriorityMapper
    : MapperBase<M.PriorityCreateModel, E.Priority>
{
    protected override Expression<Func<M.PriorityCreateModel, E.Priority>> CreateMapping()
    {
        return source => new E.Priority
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

[RegisterSingleton<IMapper<M.PriorityUpdateModel, E.Priority>>]
internal sealed class PriorityUpdateModelToPriorityMapper
    : MapperBase<M.PriorityUpdateModel, E.Priority>
{
    protected override Expression<Func<M.PriorityUpdateModel, E.Priority>> CreateMapping()
    {
        return source => new E.Priority
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

