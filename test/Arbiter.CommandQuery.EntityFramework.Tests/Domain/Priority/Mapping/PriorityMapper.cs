#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityCreateModel>>]
internal sealed class PriorityReadModelToPriorityCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    public override void Map(Models.PriorityReadModel source, Models.PriorityCreateModel destination)
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

    public override IQueryable<Models.PriorityCreateModel> ProjectTo(IQueryable<Models.PriorityReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.PriorityCreateModel
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

[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityUpdateModel>>]
internal sealed class PriorityReadModelToPriorityUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    public override void Map(Models.PriorityReadModel source, Models.PriorityUpdateModel destination)
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

    public override IQueryable<Models.PriorityUpdateModel> ProjectTo(IQueryable<Models.PriorityReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.PriorityUpdateModel
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

[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityCreateModel>>]
internal sealed class PriorityUpdateModelToPriorityCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    public override void Map(Models.PriorityUpdateModel source, Models.PriorityCreateModel destination)
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

    public override IQueryable<Models.PriorityCreateModel> ProjectTo(IQueryable<Models.PriorityUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.PriorityCreateModel
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

[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityReadModel>>]
internal sealed class PriorityToPriorityReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Priority, Models.PriorityReadModel>
{
    public override void Map(Entities.Priority source, Models.PriorityReadModel destination)
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

    public override IQueryable<Models.PriorityReadModel> ProjectTo(IQueryable<Entities.Priority> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.PriorityReadModel
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

[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityUpdateModel>>]
internal sealed class PriorityToPriorityUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Priority, Models.PriorityUpdateModel>
{
    public override void Map(Entities.Priority source, Models.PriorityUpdateModel destination)
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

    public override IQueryable<Models.PriorityUpdateModel> ProjectTo(IQueryable<Entities.Priority> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.PriorityUpdateModel
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

[RegisterSingleton<IMapper<Models.PriorityCreateModel, Entities.Priority>>]
internal sealed class PriorityCreateModelToPriorityMapper : CommandQuery.Mapping.MapperBase<Models.PriorityCreateModel, Entities.Priority>
{
    public override void Map(Models.PriorityCreateModel source, Entities.Priority destination)
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

    public override IQueryable<Entities.Priority> ProjectTo(IQueryable<Models.PriorityCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Priority
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

[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Entities.Priority>>]
internal sealed class PriorityUpdateModelToPriorityMapper : CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Entities.Priority>
{
    public override void Map(Models.PriorityUpdateModel source, Entities.Priority destination)
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

    public override IQueryable<Entities.Priority> ProjectTo(IQueryable<Models.PriorityUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Priority
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

