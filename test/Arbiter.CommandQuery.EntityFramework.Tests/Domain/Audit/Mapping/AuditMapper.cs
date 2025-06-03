#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.AuditReadModel, Models.AuditCreateModel>>]
internal sealed class AuditReadModelToAuditCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.AuditReadModel, Models.AuditCreateModel>
{
    public override void Map(Models.AuditReadModel source, Models.AuditCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.AuditCreateModel> ProjectTo(IQueryable<Models.AuditReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.AuditCreateModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.AuditReadModel, Models.AuditUpdateModel>>]
internal sealed class AuditReadModelToAuditUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.AuditReadModel, Models.AuditUpdateModel>
{
    public override void Map(Models.AuditReadModel source, Models.AuditUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.AuditUpdateModel> ProjectTo(IQueryable<Models.AuditReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.AuditUpdateModel
            {
                #region Generated Query Properties
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.AuditUpdateModel, Models.AuditCreateModel>>]
internal sealed class AuditUpdateModelToAuditCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.AuditUpdateModel, Models.AuditCreateModel>
{
    public override void Map(Models.AuditUpdateModel source, Models.AuditCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.AuditCreateModel> ProjectTo(IQueryable<Models.AuditUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.AuditCreateModel
            {
                #region Generated Query Properties
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Audit, Models.AuditReadModel>>]
internal sealed class AuditToAuditReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Audit, Models.AuditReadModel>
{
    public override void Map(Entities.Audit source, Models.AuditReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.AuditReadModel> ProjectTo(IQueryable<Entities.Audit> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.AuditReadModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Audit, Models.AuditUpdateModel>>]
internal sealed class AuditToAuditUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Audit, Models.AuditUpdateModel>
{
    public override void Map(Entities.Audit source, Models.AuditUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.AuditUpdateModel> ProjectTo(IQueryable<Entities.Audit> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.AuditUpdateModel
            {
                #region Generated Query Properties
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.AuditCreateModel, Entities.Audit>>]
internal sealed class AuditCreateModelToAuditMapper : CommandQuery.Mapping.MapperBase<Models.AuditCreateModel, Entities.Audit>
{
    public override void Map(Models.AuditCreateModel source, Entities.Audit destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Entities.Audit> ProjectTo(IQueryable<Models.AuditCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Audit
            {
                #region Generated Query Properties
                Id = p.Id,
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.AuditUpdateModel, Entities.Audit>>]
internal sealed class AuditUpdateModelToAuditMapper : CommandQuery.Mapping.MapperBase<Models.AuditUpdateModel, Entities.Audit>
{
    public override void Map(Models.AuditUpdateModel source, Entities.Audit destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Date = source.Date;
        destination.UserId = source.UserId;
        destination.TaskId = source.TaskId;
        destination.Content = source.Content;
        destination.Username = source.Username;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Entities.Audit> ProjectTo(IQueryable<Models.AuditUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Audit
            {
                #region Generated Query Properties
                Date = p.Date,
                UserId = p.UserId,
                TaskId = p.TaskId,
                Content = p.Content,
                Username = p.Username,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

