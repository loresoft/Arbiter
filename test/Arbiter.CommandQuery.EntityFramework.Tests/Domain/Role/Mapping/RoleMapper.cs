#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.RoleReadModel, Models.RoleCreateModel>>]
internal sealed class RoleReadModelToRoleCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.RoleReadModel, Models.RoleCreateModel>
{
    public override void Map(Models.RoleReadModel source, Models.RoleCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.RoleCreateModel> ProjectTo(IQueryable<Models.RoleReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.RoleCreateModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.RoleReadModel, Models.RoleUpdateModel>>]
internal sealed class RoleReadModelToRoleUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.RoleReadModel, Models.RoleUpdateModel>
{
    public override void Map(Models.RoleReadModel source, Models.RoleUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.RoleUpdateModel> ProjectTo(IQueryable<Models.RoleReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.RoleUpdateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.RoleUpdateModel, Models.RoleCreateModel>>]
internal sealed class RoleUpdateModelToRoleCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.RoleUpdateModel, Models.RoleCreateModel>
{
    public override void Map(Models.RoleUpdateModel source, Models.RoleCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.RoleCreateModel> ProjectTo(IQueryable<Models.RoleUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.RoleCreateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.Role, Models.RoleReadModel>>]
internal sealed class RoleToRoleReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Role, Models.RoleReadModel>
{
    public override void Map(Entities.Role source, Models.RoleReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.RoleReadModel> ProjectTo(IQueryable<Entities.Role> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.RoleReadModel
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
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

[RegisterSingleton<IMapper<Entities.Role, Models.RoleUpdateModel>>]
internal sealed class RoleToRoleUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Role, Models.RoleUpdateModel>
{
    public override void Map(Entities.Role source, Models.RoleUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.RoleUpdateModel> ProjectTo(IQueryable<Entities.Role> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.RoleUpdateModel
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.RoleCreateModel, Entities.Role>>]
internal sealed class RoleCreateModelToRoleMapper : CommandQuery.Mapping.MapperBase<Models.RoleCreateModel, Entities.Role>
{
    public override void Map(Models.RoleCreateModel source, Entities.Role destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Entities.Role> ProjectTo(IQueryable<Models.RoleCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Role
            {
                #region Generated Query Properties
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.RoleUpdateModel, Entities.Role>>]
internal sealed class RoleUpdateModelToRoleMapper : CommandQuery.Mapping.MapperBase<Models.RoleUpdateModel, Entities.Role>
{
    public override void Map(Models.RoleUpdateModel source, Entities.Role destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Entities.Role> ProjectTo(IQueryable<Models.RoleUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.Role
            {
                #region Generated Query Properties
                Name = p.Name,
                Description = p.Description,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

