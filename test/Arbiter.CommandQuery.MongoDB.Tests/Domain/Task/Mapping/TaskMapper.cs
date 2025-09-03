#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;
using System.Linq.Expressions;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskCreateModel>>]
internal sealed class TaskReadModelToTaskCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TaskReadModel, Models.TaskCreateModel>
{
    protected override Expression<Func<Models.TaskReadModel, Models.TaskCreateModel>> CreateMapping()
    {
        return source => new Models.TaskCreateModel
        {
            Id = source.Id,
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskUpdateModel>>]
internal sealed class TaskReadModelToTaskUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.TaskReadModel, Models.TaskUpdateModel>
{
    protected override Expression<Func<Models.TaskReadModel, Models.TaskUpdateModel>> CreateMapping()
    {
        return source => new Models.TaskUpdateModel
        {
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskCreateModel>>]
internal sealed class TaskUpdateModelToTaskCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Models.TaskCreateModel>
{
    protected override Expression<Func<Models.TaskUpdateModel, Models.TaskCreateModel>> CreateMapping()
    {
        return source => new Models.TaskCreateModel
        {
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed class TaskToTaskReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskReadModel>
{
    protected override Expression<Func<Entities.Task, Models.TaskReadModel>> CreateMapping()
    {
        return source => new Models.TaskReadModel
        {
            Id = source.Id,
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

[RegisterSingleton<IMapper<Entities.Task, Models.TaskNameModel>>]
internal sealed class TaskToTaskNameModelMapper : CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskNameModel>
{
    protected override Expression<Func<Entities.Task, Models.TaskNameModel>> CreateMapping()
    {
        return source => new Models.TaskNameModel
        {
            Id = source.Id,
            Title = source.Title,
        };
    }
}

[RegisterSingleton<IMapper<Entities.Task, Models.TaskUpdateModel>>]
internal sealed class TaskToTaskUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskUpdateModel>
{
    protected override Expression<Func<Entities.Task, Models.TaskUpdateModel>> CreateMapping()
    {
        return source => new Models.TaskUpdateModel
        {
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

[RegisterSingleton<IMapper<Models.TaskCreateModel, Entities.Task>>]
internal sealed class TaskCreateModelToTaskMapper : CommandQuery.Mapping.MapperBase<Models.TaskCreateModel, Entities.Task>
{
    protected override Expression<Func<Models.TaskCreateModel, Entities.Task>> CreateMapping()
    {
        return source => new Entities.Task
        {
            Id = source.Id,
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.TaskUpdateModel, Entities.Task>>]
internal sealed class TaskUpdateModelToTaskMapper : CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Entities.Task>
{
    protected override Expression<Func<Models.TaskUpdateModel, Entities.Task>> CreateMapping()
    {
        return source => new Entities.Task
        {
            StatusId = source.StatusId,
            PriorityId = source.PriorityId,
            Title = source.Title,
            Description = source.Description,
            StartDate = source.StartDate,
            DueDate = source.DueDate,
            CompleteDate = source.CompleteDate,
            AssignedId = source.AssignedId,
            TenantId = source.TenantId,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

