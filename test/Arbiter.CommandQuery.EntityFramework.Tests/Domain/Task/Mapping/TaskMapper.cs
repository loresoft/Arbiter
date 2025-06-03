#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskCreateModel>>]
internal sealed class TaskReadModelToTaskCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TaskReadModel, Models.TaskCreateModel>
{
    public override void Map(Models.TaskReadModel source, Models.TaskCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.TaskCreateModel> ProjectTo(IQueryable<Models.TaskReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TaskCreateModel
            {
                #region Generated Query Properties
                Id = p.Id,
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskUpdateModel>>]
internal sealed class TaskReadModelToTaskUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.TaskReadModel, Models.TaskUpdateModel>
{
    public override void Map(Models.TaskReadModel source, Models.TaskUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.TaskUpdateModel> ProjectTo(IQueryable<Models.TaskReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TaskUpdateModel
            {
                #region Generated Query Properties
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskCreateModel>>]
internal sealed class TaskUpdateModelToTaskCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Models.TaskCreateModel>
{
    public override void Map(Models.TaskUpdateModel source, Models.TaskCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.TaskCreateModel> ProjectTo(IQueryable<Models.TaskUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TaskCreateModel
            {
                #region Generated Query Properties
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed class TaskToTaskReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskReadModel>
{
    public override void Map(Entities.Task source, Models.TaskReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion

        destination.StatusName = source.Status.Name;
        destination.PriorityName = source.Priority?.Name;
        destination.AssignedName = source.AssignedUser?.EmailAddress;
        destination.TenantName = source.Tenant.Name;
    }

    public override IQueryable<Models.TaskReadModel> ProjectTo(IQueryable<Entities.Task> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TaskReadModel
            {
                #region Generated Query Properties
                Id = p.Id,
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion

                StatusName = p.Status.Name,
                PriorityName = p.Priority != null ? p.Priority.Name : null,
                AssignedName = p.AssignedUser != null ? p.AssignedUser.EmailAddress : null,
                TenantName = p.Tenant.Name,
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Task, Models.TaskUpdateModel>>]
internal sealed class TaskToTaskUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Task, Models.TaskUpdateModel>
{
    public override void Map(Entities.Task source, Models.TaskUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.TaskUpdateModel> ProjectTo(IQueryable<Entities.Task> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TaskUpdateModel
            {
                #region Generated Query Properties
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.TaskCreateModel, Entities.Task>>]
internal sealed class TaskCreateModelToTaskMapper : CommandQuery.Mapping.MapperBase<Models.TaskCreateModel, Entities.Task>
{
    public override void Map(Models.TaskCreateModel source, Entities.Task destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Entities.Task> ProjectTo(IQueryable<Models.TaskCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Task
            {
                #region Generated Query Properties
                Id = p.Id,
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.TaskUpdateModel, Entities.Task>>]
internal sealed class TaskUpdateModelToTaskMapper : CommandQuery.Mapping.MapperBase<Models.TaskUpdateModel, Entities.Task>
{
    public override void Map(Models.TaskUpdateModel source, Entities.Task destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.StatusId = source.StatusId;
        destination.PriorityId = source.PriorityId;
        destination.Title = source.Title;
        destination.Description = source.Description;
        destination.StartDate = source.StartDate;
        destination.DueDate = source.DueDate;
        destination.CompleteDate = source.CompleteDate;
        destination.AssignedId = source.AssignedId;
        destination.TenantId = source.TenantId;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Entities.Task> ProjectTo(IQueryable<Models.TaskUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Task
            {
                #region Generated Query Properties
                StatusId = p.StatusId,
                PriorityId = p.PriorityId,
                Title = p.Title,
                Description = p.Description,
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                CompleteDate = p.CompleteDate,
                AssignedId = p.AssignedId,
                TenantId = p.TenantId,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

