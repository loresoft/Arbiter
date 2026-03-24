using System.Linq.Expressions;

using Arbiter.Mapping;

namespace Arbiter.Benchmarks.Mapping;

#pragma warning disable CS0618 // Type or member is obsolete
public class PriorityArbiterMapper : MapperBase<Priority, PriorityReadModel>
#pragma warning restore CS0618 // Type or member is obsolete
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
