#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed class TaskToTaskReadModelMapper
    : MapperBase<Entities.Task, Models.TaskReadModel>
{
    protected override Expression<Func<Entities.Task, Models.TaskReadModel>> CreateMapping()
    {
        return source => new Models.TaskReadModel
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
            RowVersion = source.RowVersion,

            // Navigation Mappings
            TenantName = source.Tenant != null ? source.Tenant.Name : null,
            StatusName = source.Status != null ? source.Status.Name : null,
            PriorityName = source.Priority != null ? source.Priority.Name : null,
        };
    }
}

[RegisterSingleton<IMapper<Entities.Task, Models.TaskUpdateModel>>]
internal sealed class TaskToTaskUpdateModelMapper
    : MapperBase<Entities.Task, Models.TaskUpdateModel>
{
    protected override Expression<Func<Entities.Task, Models.TaskUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<Models.TaskCreateModel, Entities.Task>>]
internal sealed class TaskCreateModelToTaskMapper
    : MapperBase<Models.TaskCreateModel, Entities.Task>
{
    protected override Expression<Func<Models.TaskCreateModel, Entities.Task>> CreateMapping()
    {
        return source => new Entities.Task
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

[RegisterSingleton<IMapper<Models.TaskUpdateModel, Entities.Task>>]
internal sealed class TaskUpdateModelToTaskMapper
    : MapperBase<Models.TaskUpdateModel, Entities.Task>
{
    protected override Expression<Func<Models.TaskUpdateModel, Entities.Task>> CreateMapping()
    {
        return source => new Entities.Task
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

