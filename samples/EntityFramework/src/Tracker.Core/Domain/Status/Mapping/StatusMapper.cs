#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Entities.Status, Models.StatusReadModel>>]
internal sealed class StatusToStatusReadModelMapper
    : MapperBase<Entities.Status, Models.StatusReadModel>
{
    protected override Expression<Func<Entities.Status, Models.StatusReadModel>> CreateMapping()
    {
        return source => new Models.StatusReadModel
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

[RegisterSingleton<IMapper<Entities.Status, Models.StatusUpdateModel>>]
internal sealed class StatusToStatusUpdateModelMapper
    : MapperBase<Entities.Status, Models.StatusUpdateModel>
{
    protected override Expression<Func<Entities.Status, Models.StatusUpdateModel>> CreateMapping()
    {
        return source => new Models.StatusUpdateModel
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

[RegisterSingleton<IMapper<Models.StatusCreateModel, Entities.Status>>]
internal sealed class StatusCreateModelToStatusMapper
    : MapperBase<Models.StatusCreateModel, Entities.Status>
{
    protected override Expression<Func<Models.StatusCreateModel, Entities.Status>> CreateMapping()
    {
        return source => new Entities.Status
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

[RegisterSingleton<IMapper<Models.StatusUpdateModel, Entities.Status>>]
internal sealed class StatusUpdateModelToStatusMapper
    : MapperBase<Models.StatusUpdateModel, Entities.Status>
{
    protected override Expression<Func<Models.StatusUpdateModel, Entities.Status>> CreateMapping()
    {
        return source => new Entities.Status
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

