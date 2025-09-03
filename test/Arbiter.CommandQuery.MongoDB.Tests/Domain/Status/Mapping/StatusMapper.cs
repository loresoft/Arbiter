#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusCreateModel>>]
internal sealed class StatusReadModelToStatusCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.StatusReadModel, Models.StatusCreateModel>
{
    protected override Expression<Func<Models.StatusReadModel, Models.StatusCreateModel>> CreateMapping()
    {
        return source => new Models.StatusCreateModel
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

[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusUpdateModel>>]
internal sealed class StatusReadModelToStatusUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.StatusReadModel, Models.StatusUpdateModel>
{
    protected override Expression<Func<Models.StatusReadModel, Models.StatusUpdateModel>> CreateMapping()
    {
        return source => new Models.StatusUpdateModel
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

[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusCreateModel>>]
internal sealed class StatusUpdateModelToStatusCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Models.StatusCreateModel>
{
    protected override Expression<Func<Models.StatusUpdateModel, Models.StatusCreateModel>> CreateMapping()
    {
        return source => new Models.StatusCreateModel
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

[RegisterSingleton<IMapper<Entities.Status, Models.StatusReadModel>>]
internal sealed class StatusToStatusReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.Status, Models.StatusReadModel>
{
    protected override Expression<Func<Entities.Status, Models.StatusReadModel>> CreateMapping()
    {
        return source => new Models.StatusReadModel
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

[RegisterSingleton<IMapper<Entities.Status, Models.StatusUpdateModel>>]
internal sealed class StatusToStatusUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.Status, Models.StatusUpdateModel>
{
    protected override Expression<Func<Entities.Status, Models.StatusUpdateModel>> CreateMapping()
    {
        return source => new Models.StatusUpdateModel
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

[RegisterSingleton<IMapper<Models.StatusCreateModel, Entities.Status>>]
internal sealed class StatusCreateModelToStatusMapper : CommandQuery.Mapping.MapperBase<Models.StatusCreateModel, Entities.Status>
{
    protected override Expression<Func<Models.StatusCreateModel, Entities.Status>> CreateMapping()
    {
        return source => new Entities.Status
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

[RegisterSingleton<IMapper<Models.StatusUpdateModel, Entities.Status>>]
internal sealed class StatusUpdateModelToStatusMapper : CommandQuery.Mapping.MapperBase<Models.StatusUpdateModel, Entities.Status>
{
    protected override Expression<Func<Models.StatusUpdateModel, Entities.Status>> CreateMapping()
    {
        return source => new Entities.Status
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

