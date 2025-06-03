#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantCreateModel>>]
internal sealed class TenantReadModelToTenantCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TenantReadModel, Models.TenantCreateModel>
{
    public override void Map(Models.TenantReadModel source, Models.TenantCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.TenantCreateModel> ProjectTo(IQueryable<Models.TenantReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TenantCreateModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
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

[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantUpdateModel>>]
internal sealed class TenantReadModelToTenantUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.TenantReadModel, Models.TenantUpdateModel>
{
    public override void Map(Models.TenantReadModel source, Models.TenantUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.TenantUpdateModel> ProjectTo(IQueryable<Models.TenantReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TenantUpdateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantCreateModel>>]
internal sealed class TenantUpdateModelToTenantCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Models.TenantCreateModel>
{
    public override void Map(Models.TenantUpdateModel source, Models.TenantCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.TenantCreateModel> ProjectTo(IQueryable<Models.TenantUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TenantCreateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantReadModel>>]
internal sealed class TenantToTenantReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Tenant, Models.TenantReadModel>
{
    public override void Map(Entities.Tenant source, Models.TenantReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.TenantReadModel> ProjectTo(IQueryable<Entities.Tenant> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TenantReadModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
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

[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantUpdateModel>>]
internal sealed class TenantToTenantUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Tenant, Models.TenantUpdateModel>
{
    public override void Map(Entities.Tenant source, Models.TenantUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.TenantUpdateModel> ProjectTo(IQueryable<Entities.Tenant> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.TenantUpdateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.TenantCreateModel, Entities.Tenant>>]
internal sealed class TenantCreateModelToTenantMapper : CommandQuery.Mapping.MapperBase<Models.TenantCreateModel, Entities.Tenant>
{
    public override void Map(Models.TenantCreateModel source, Entities.Tenant destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Entities.Tenant> ProjectTo(IQueryable<Models.TenantCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Tenant
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
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

[RegisterSingleton<IMapper<Models.TenantUpdateModel, Entities.Tenant>>]
internal sealed class TenantUpdateModelToTenantMapper : CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Entities.Tenant>
{
    public override void Map(Models.TenantUpdateModel source, Entities.Tenant destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Entities.Tenant> ProjectTo(IQueryable<Models.TenantUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Tenant
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                IsActive = p.IsActive,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

