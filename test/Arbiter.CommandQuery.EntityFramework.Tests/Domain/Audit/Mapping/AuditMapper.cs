#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.AuditReadModel, Models.AuditCreateModel>>]
internal sealed class AuditReadModelToAuditCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.AuditReadModel, Models.AuditCreateModel>
{
    protected override Expression<Func<Models.AuditReadModel, Models.AuditCreateModel>> CreateMapping()
    {
        return source => new Models.AuditCreateModel
        {
            Id = source.Id,
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.AuditReadModel, Models.AuditUpdateModel>>]
internal sealed class AuditReadModelToAuditUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.AuditReadModel, Models.AuditUpdateModel>
{
    protected override Expression<Func<Models.AuditReadModel, Models.AuditUpdateModel>> CreateMapping()
    {
        return source => new Models.AuditUpdateModel
        {
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.AuditUpdateModel, Models.AuditCreateModel>>]
internal sealed class AuditUpdateModelToAuditCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.AuditUpdateModel, Models.AuditCreateModel>
{
    protected override Expression<Func<Models.AuditUpdateModel, Models.AuditCreateModel>> CreateMapping()
    {
        return source => new Models.AuditCreateModel
        {
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.Audit, Models.AuditReadModel>>]
internal sealed class AuditToAuditReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Audit, Models.AuditReadModel>
{
    protected override Expression<Func<Entities.Audit, Models.AuditReadModel>> CreateMapping()
    {
        return source => new Models.AuditReadModel
        {
            Id = source.Id,
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Entities.Audit, Models.AuditUpdateModel>>]
internal sealed class AuditToAuditUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Audit, Models.AuditUpdateModel>
{
    protected override Expression<Func<Entities.Audit, Models.AuditUpdateModel>> CreateMapping()
    {
        return source => new Models.AuditUpdateModel
        {
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.AuditCreateModel, Entities.Audit>>]
internal sealed class AuditCreateModelToAuditMapper : CommandQuery.Mapping.MapperBase<Models.AuditCreateModel, Entities.Audit>
{
    protected override Expression<Func<Models.AuditCreateModel, Entities.Audit>> CreateMapping()
    {
        return source => new Entities.Audit
        {
            Id = source.Id,
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.AuditUpdateModel, Entities.Audit>>]
internal sealed class AuditUpdateModelToAuditMapper : CommandQuery.Mapping.MapperBase<Models.AuditUpdateModel, Entities.Audit>
{
    protected override Expression<Func<Models.AuditUpdateModel, Entities.Audit>> CreateMapping()
    {
        return source => new Entities.Audit
        {
            Date = source.Date,
            UserId = source.UserId,
            TaskId = source.TaskId,
            Content = source.Content,
            Username = source.Username,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

