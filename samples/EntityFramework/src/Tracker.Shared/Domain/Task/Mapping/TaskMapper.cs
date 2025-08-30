#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskCreateModel>>]
internal sealed class TaskReadModelToTaskCreateModelMapper
    : MapperBase<Models.TaskReadModel, Models.TaskCreateModel>
{
    protected override Expression<Func<Models.TaskReadModel, Models.TaskCreateModel>> CreateMapping()
    {
        return source => new Models.TaskCreateModel
        {
            #region Generated Mappings
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            IsDeleted = source.IsDeleted,
            TenantId = source.TenantId,
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            AssignedId = source.AssignedId,
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

[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskUpdateModel>>]
internal sealed class TaskReadModelToTaskUpdateModelMapper
    : MapperBase<Models.TaskReadModel, Models.TaskUpdateModel>
{
    protected override Expression<Func<Models.TaskReadModel, Models.TaskUpdateModel>> CreateMapping()
    {
        return source => new Models.TaskUpdateModel
        {
            #region Generated Mappings
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            IsDeleted = source.IsDeleted,
            TenantId = source.TenantId,
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            AssignedId = source.AssignedId,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskCreateModel>>]
internal sealed class TaskUpdateModelToTaskCreateModelMapper
    : MapperBase<Models.TaskUpdateModel, Models.TaskCreateModel>
{
    protected override Expression<Func<Models.TaskUpdateModel, Models.TaskCreateModel>> CreateMapping()
    {
        return source => new Models.TaskCreateModel
        {
            #region Generated Mappings
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            IsDeleted = source.IsDeleted,
            TenantId = source.TenantId,
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            AssignedId = source.AssignedId,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

