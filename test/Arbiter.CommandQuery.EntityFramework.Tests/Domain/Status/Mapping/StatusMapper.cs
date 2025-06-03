#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusCreateModel>>]
internal sealed class StatusReadModelToStatusCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.StatusReadModel, Models.StatusCreateModel>
{
    public override void Map(Models.StatusReadModel source, Models.StatusCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.StatusCreateModel> ProjectTo(IQueryable<Models.StatusReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.StatusCreateModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusUpdateModel>>]
internal sealed class StatusReadModelToStatusUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.StatusReadModel, Models.StatusUpdateModel>
{
    public override void Map(Models.StatusReadModel source, Models.StatusUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.StatusUpdateModel> ProjectTo(IQueryable<Models.StatusReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.StatusUpdateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusCreateModel>>]
internal sealed class StatusUpdateModelToStatusCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Models.StatusCreateModel>
{
    public override void Map(Models.StatusUpdateModel source, Models.StatusCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.StatusCreateModel> ProjectTo(IQueryable<Models.StatusUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.StatusCreateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Status, Models.StatusReadModel>>]
internal sealed class StatusToStatusReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Status, Models.StatusReadModel>
{
    public override void Map(Entities.Status source, Models.StatusReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.StatusReadModel> ProjectTo(IQueryable<Entities.Status> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.StatusReadModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
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

[RegisterSingleton<IMapper<Entities.Status, Models.StatusUpdateModel>>]
internal sealed class StatusToStatusUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Status, Models.StatusUpdateModel>
{
    public override void Map(Entities.Status source, Models.StatusUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.StatusUpdateModel> ProjectTo(IQueryable<Entities.Status> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.StatusUpdateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.StatusCreateModel, Entities.Status>>]
internal sealed class StatusCreateModelToStatusMapper : CommandQuery.Mapping.MapperBase<Models.StatusCreateModel, Entities.Status>
{
    public override void Map(Models.StatusCreateModel source, Entities.Status destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Entities.Status> ProjectTo(IQueryable<Models.StatusCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Status
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.StatusUpdateModel, Entities.Status>>]
internal sealed class StatusUpdateModelToStatusMapper : CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Entities.Status>
{
    public override void Map(Models.StatusUpdateModel source, Entities.Status destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Entities.Status> ProjectTo(IQueryable<Models.StatusUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Status
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

