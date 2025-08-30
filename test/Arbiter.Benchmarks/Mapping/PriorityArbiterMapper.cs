using System.Linq.Expressions;

using Arbiter.CommandQuery.Mapping;

namespace Arbiter.Benchmarks.Mapping;

public class PriorityArbiterMapper : MapperBase<Priority, PriorityReadModel>
{
    protected override Expression<Func<Priority, PriorityReadModel>> CreateMapping()
    {
        return source => new PriorityReadModel
        {
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            Id = source.Id,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}
