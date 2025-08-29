using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

namespace Arbiter.Benchmarks.Mapping;

public class PriorityManualMapper : IMapper<Priority, PriorityReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public PriorityReadModel? Map(Priority? source)
    {
        if (source == null)
            return null;

        return new PriorityReadModel
        {
            // Generated Mappings
            Name = source.Name,
            Description = source.Description,
            DisplayOrder = source.DisplayOrder,
            IsActive = source.IsActive,
            // Manual Mappings
            Id = source.Id,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }

    public void Map(Priority source, PriorityReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        destination.Id = source.Id;
        destination.Name = source.Name;
        destination.Description = source.Description;
        destination.DisplayOrder = source.DisplayOrder;
        destination.IsActive = source.IsActive;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
    }

    public IQueryable<PriorityReadModel> ProjectTo(IQueryable<Priority> source)
    {
        return source.Select(s => new PriorityReadModel
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            Created = s.Created,
            CreatedBy = s.CreatedBy,
            Updated = s.Updated,
            UpdatedBy = s.UpdatedBy,
        });
    }
}
