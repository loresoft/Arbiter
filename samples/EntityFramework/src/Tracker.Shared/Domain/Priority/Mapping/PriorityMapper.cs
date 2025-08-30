#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityCreateModel>>]
internal sealed class PriorityReadModelToPriorityCreateModelMapper
    : MapperBase<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    protected override Expression<Func<Models.PriorityReadModel, Models.PriorityCreateModel>> CreateMapping()
    {
        return source => new Models.PriorityCreateModel
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

[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityUpdateModel>>]
internal sealed class PriorityReadModelToPriorityUpdateModelMapper
    : MapperBase<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    protected override Expression<Func<Models.PriorityReadModel, Models.PriorityUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityCreateModel>>]
internal sealed class PriorityUpdateModelToPriorityCreateModelMapper
    : MapperBase<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    protected override Expression<Func<Models.PriorityUpdateModel, Models.PriorityCreateModel>> CreateMapping()
    {
        return source => new Models.PriorityCreateModel
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

