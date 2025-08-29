#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<M.PriorityReadModel, M.PriorityCreateModel>>]
internal sealed class PriorityReadModelToPriorityCreateModelMapper
    : MapperBase<M.PriorityReadModel, M.PriorityCreateModel>
{
    protected override Expression<Func<M.PriorityReadModel, M.PriorityCreateModel>> CreateMapping()
    {
        return source => new M.PriorityCreateModel
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

[RegisterSingleton<IMapper<M.PriorityReadModel, M.PriorityUpdateModel>>]
internal sealed class PriorityReadModelToPriorityUpdateModelMapper
    : MapperBase<M.PriorityReadModel, M.PriorityUpdateModel>
{
    protected override Expression<Func<M.PriorityReadModel, M.PriorityUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<M.PriorityUpdateModel, M.PriorityCreateModel>>]
internal sealed class PriorityUpdateModelToPriorityCreateModelMapper
    : MapperBase<M.PriorityUpdateModel, M.PriorityCreateModel>
{
    protected override Expression<Func<M.PriorityUpdateModel, M.PriorityCreateModel>> CreateMapping()
    {
        return source => new M.PriorityCreateModel
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

