#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusCreateModel>>]
internal sealed class StatusReadModelToStatusCreateModelMapper
    : MapperBase<Models.StatusReadModel, Models.StatusCreateModel>
{
    protected override Expression<Func<Models.StatusReadModel, Models.StatusCreateModel>> CreateMapping()
    {
        return source => new Models.StatusCreateModel
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

[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusUpdateModel>>]
internal sealed class StatusReadModelToStatusUpdateModelMapper
    : MapperBase<Models.StatusReadModel, Models.StatusUpdateModel>
{
    protected override Expression<Func<Models.StatusReadModel, Models.StatusUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusCreateModel>>]
internal sealed class StatusUpdateModelToStatusCreateModelMapper
    : MapperBase<Models.StatusUpdateModel, Models.StatusCreateModel>
{
    protected override Expression<Func<Models.StatusUpdateModel, Models.StatusCreateModel>> CreateMapping()
    {
        return source => new Models.StatusCreateModel
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

