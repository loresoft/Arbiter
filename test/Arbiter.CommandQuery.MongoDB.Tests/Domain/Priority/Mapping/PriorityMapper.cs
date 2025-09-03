#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;
using System.Linq.Expressions;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityCreateModel>>]
internal sealed class PriorityReadModelToPriorityCreateModelMapper
    : CommandQuery.Mapping.MapperBase<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    protected override Expression<Func<Models.PriorityReadModel, Models.PriorityCreateModel>> CreateMapping()
    {
        return source => new Models.PriorityCreateModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityUpdateModel>>]
internal sealed class PriorityReadModelToPriorityUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    protected override Expression<Func<Models.PriorityReadModel, Models.PriorityUpdateModel>> CreateMapping()
    {
        return source => new Models.PriorityUpdateModel
        {
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityCreateModel>>]
internal sealed class PriorityUpdateModelToPriorityCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    protected override Expression<Func<Models.PriorityUpdateModel, Models.PriorityCreateModel>> CreateMapping()
    {
        return source => new Models.PriorityCreateModel
        {
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityReadModel>>]
internal sealed class PriorityToPriorityReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Priority, Models.PriorityReadModel>
{
    protected override Expression<Func<Entities.Priority, Models.PriorityReadModel>> CreateMapping()
    {
        return source => new Models.PriorityReadModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityUpdateModel>>]
internal sealed class PriorityToPriorityUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Priority, Models.PriorityUpdateModel>
{
    protected override Expression<Func<Entities.Priority, Models.PriorityUpdateModel>> CreateMapping()
    {
        return source => new Models.PriorityUpdateModel
        {
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.PriorityCreateModel, Entities.Priority>>]
internal sealed class PriorityCreateModelToPriorityMapper : CommandQuery.Mapping.MapperBase<Models.PriorityCreateModel, Entities.Priority>
{
    protected override Expression<Func<Models.PriorityCreateModel, Entities.Priority>> CreateMapping()
    {
        return source => new Entities.Priority
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Entities.Priority>>]
internal sealed class PriorityUpdateModelToPriorityMapper : CommandQuery.Mapping.MapperBase<Models.PriorityUpdateModel, Entities.Priority>
{
    protected override Expression<Func<Models.PriorityUpdateModel, Entities.Priority>> CreateMapping()
    {
        return source => new Entities.Priority
        {
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

