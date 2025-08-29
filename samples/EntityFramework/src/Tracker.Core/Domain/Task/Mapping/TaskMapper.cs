#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using E = Tracker.Data.Entities;
using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<E.Task, M.TaskReadModel>>]
internal sealed class TaskToTaskReadModelMapper
    : MapperBase<E.Task, M.TaskReadModel>
{
    protected override Expression<Func<E.Task, M.TaskReadModel>> CreateMapping()
    {
        return source => new M.TaskReadModel
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

[RegisterSingleton<IMapper<E.Task, M.TaskUpdateModel>>]
internal sealed class TaskToTaskUpdateModelMapper
    : MapperBase<E.Task, M.TaskUpdateModel>
{
    protected override Expression<Func<E.Task, M.TaskUpdateModel>> CreateMapping()
    {
        return source => new M.TaskUpdateModel
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

[RegisterSingleton<IMapper<M.TaskCreateModel, E.Task>>]
internal sealed class TaskCreateModelToTaskMapper
    : MapperBase<M.TaskCreateModel, E.Task>
{
    protected override Expression<Func<M.TaskCreateModel, E.Task>> CreateMapping()
    {
        return source => new E.Task
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

[RegisterSingleton<IMapper<M.TaskUpdateModel, E.Task>>]
internal sealed class TaskUpdateModelToTaskMapper
    : MapperBase<M.TaskUpdateModel, E.Task>
{
    protected override Expression<Func<M.TaskUpdateModel, E.Task>> CreateMapping()
    {
        return source => new E.Task
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

