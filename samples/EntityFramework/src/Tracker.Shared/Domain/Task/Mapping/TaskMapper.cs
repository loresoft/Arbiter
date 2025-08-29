#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<M.TaskReadModel, M.TaskCreateModel>>]
internal sealed class TaskReadModelToTaskCreateModelMapper
    : MapperBase<M.TaskReadModel, M.TaskCreateModel>
{
    protected override Expression<Func<M.TaskReadModel, M.TaskCreateModel>> CreateMapping()
    {
        return source => new M.TaskCreateModel
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

[RegisterSingleton<IMapper<M.TaskReadModel, M.TaskUpdateModel>>]
internal sealed class TaskReadModelToTaskUpdateModelMapper
    : MapperBase<M.TaskReadModel, M.TaskUpdateModel>
{
    protected override Expression<Func<M.TaskReadModel, M.TaskUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<M.TaskUpdateModel, M.TaskCreateModel>>]
internal sealed class TaskUpdateModelToTaskCreateModelMapper
    : MapperBase<M.TaskUpdateModel, M.TaskCreateModel>
{
    protected override Expression<Func<M.TaskUpdateModel, M.TaskCreateModel>> CreateMapping()
    {
        return source => new M.TaskCreateModel
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

