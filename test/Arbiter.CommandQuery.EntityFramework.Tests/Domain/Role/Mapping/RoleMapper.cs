#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;
using System.Linq.Expressions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.RoleReadModel, Models.RoleCreateModel>>]
internal sealed class RoleReadModelToRoleCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.RoleReadModel, Models.RoleCreateModel>
{
    protected override Expression<Func<Models.RoleReadModel, Models.RoleCreateModel>> CreateMapping()
    {
        return source => new Models.RoleCreateModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.RoleReadModel, Models.RoleUpdateModel>>]
internal sealed class RoleReadModelToRoleUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.RoleReadModel, Models.RoleUpdateModel>
{
    protected override Expression<Func<Models.RoleReadModel, Models.RoleUpdateModel>> CreateMapping()
    {
        return source => new Models.RoleUpdateModel
        {
            Name = source.Name,
            Description = source.Description,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.RoleUpdateModel, Models.RoleCreateModel>>]
internal sealed class RoleUpdateModelToRoleCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.RoleUpdateModel, Models.RoleCreateModel>
{
    protected override Expression<Func<Models.RoleUpdateModel, Models.RoleCreateModel>> CreateMapping()
    {
        return source => new Models.RoleCreateModel
        {
            Name = source.Name,
            Description = source.Description,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.Role, Models.RoleReadModel>>]
internal sealed class RoleToRoleReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Role, Models.RoleReadModel>
{
    protected override Expression<Func<Entities.Role, Models.RoleReadModel>> CreateMapping()
    {
        return source => new Models.RoleReadModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Entities.Role, Models.RoleUpdateModel>>]
internal sealed class RoleToRoleUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Role, Models.RoleUpdateModel>
{
    protected override Expression<Func<Entities.Role, Models.RoleUpdateModel>> CreateMapping()
    {
        return source => new Models.RoleUpdateModel
        {
            Name = source.Name,
            Description = source.Description,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.RoleCreateModel, Entities.Role>>]
internal sealed class RoleCreateModelToRoleMapper : CommandQuery.Mapping.MapperBase<Models.RoleCreateModel, Entities.Role>
{
    protected override Expression<Func<Models.RoleCreateModel, Entities.Role>> CreateMapping()
    {
        return source => new Entities.Role
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.RoleUpdateModel, Entities.Role>>]
internal sealed class RoleUpdateModelToRoleMapper : CommandQuery.Mapping.MapperBase<Models.RoleUpdateModel, Entities.Role>
{
    protected override Expression<Func<Models.RoleUpdateModel, Entities.Role>> CreateMapping()
    {
        return source => new Entities.Role
        {
            Name = source.Name,
            Description = source.Description,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

