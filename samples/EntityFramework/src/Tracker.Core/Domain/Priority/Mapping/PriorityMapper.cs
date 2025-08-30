#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityReadModel>>]
internal sealed class PriorityToPriorityReadModelMapper
    : MapperBase<Entities.Priority, Models.PriorityReadModel>
{
    protected override Expression<Func<Entities.Priority, Models.PriorityReadModel>> CreateMapping()
    {
        return source => new Models.PriorityReadModel
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

[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityUpdateModel>>]
internal sealed class PriorityToPriorityUpdateModelMapper
    : MapperBase<Entities.Priority, Models.PriorityUpdateModel>
{
    protected override Expression<Func<Entities.Priority, Models.PriorityUpdateModel>> CreateMapping()
    {
        return source => new Models.PriorityUpdateModel
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

[RegisterSingleton<IMapper<Models.PriorityCreateModel, Entities.Priority>>]
internal sealed class PriorityCreateModelToPriorityMapper
    : MapperBase<Models.PriorityCreateModel, Entities.Priority>
{
    protected override Expression<Func<Models.PriorityCreateModel, Entities.Priority>> CreateMapping()
    {
        return source => new Entities.Priority
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

[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Entities.Priority>>]
internal sealed class PriorityUpdateModelToPriorityMapper
    : MapperBase<Models.PriorityUpdateModel, Entities.Priority>
{
    protected override Expression<Func<Models.PriorityUpdateModel, Entities.Priority>> CreateMapping()
    {
        return source => new Entities.Priority
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

