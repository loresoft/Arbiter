#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;
using System.Linq.Expressions;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantCreateModel>>]
internal sealed class TenantReadModelToTenantCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TenantReadModel, Models.TenantCreateModel>
{
    protected override Expression<Func<Models.TenantReadModel, Models.TenantCreateModel>> CreateMapping()
    {
        return source => new Models.TenantCreateModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantUpdateModel>>]
internal sealed class TenantReadModelToTenantUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.TenantReadModel, Models.TenantUpdateModel>
{
    protected override Expression<Func<Models.TenantReadModel, Models.TenantUpdateModel>> CreateMapping()
    {
        return source => new Models.TenantUpdateModel
        {
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantCreateModel>>]
internal sealed class TenantUpdateModelToTenantCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Models.TenantCreateModel>
{
    protected override Expression<Func<Models.TenantUpdateModel, Models.TenantCreateModel>> CreateMapping()
    {
        return source => new Models.TenantCreateModel
        {
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantReadModel>>]
internal sealed class TenantToTenantReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Tenant, Models.TenantReadModel>
{
    protected override Expression<Func<Entities.Tenant, Models.TenantReadModel>> CreateMapping()
    {
        return source => new Models.TenantReadModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

[RegisterSingleton<IMapper<Entities.Tenant, Models.TenantUpdateModel>>]
internal sealed class TenantToTenantUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Tenant, Models.TenantUpdateModel>
{
    protected override Expression<Func<Entities.Tenant, Models.TenantUpdateModel>> CreateMapping()
    {
        return source => new Models.TenantUpdateModel
        {
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

[RegisterSingleton<IMapper<Models.TenantCreateModel, Entities.Tenant>>]
internal sealed class TenantCreateModelToTenantMapper : CommandQuery.Mapping.MapperBase<Models.TenantCreateModel, Entities.Tenant>
{
    protected override Expression<Func<Models.TenantCreateModel, Entities.Tenant>> CreateMapping()
    {
        return source => new Entities.Tenant
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.TenantUpdateModel, Entities.Tenant>>]
internal sealed class TenantUpdateModelToTenantMapper : CommandQuery.Mapping.MapperBase<Models.TenantUpdateModel, Entities.Tenant>
{
    protected override Expression<Func<Models.TenantUpdateModel, Entities.Tenant>> CreateMapping()
    {
        return source => new Entities.Tenant
        {
            Name = source.Name,
            Description = source.Description,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

