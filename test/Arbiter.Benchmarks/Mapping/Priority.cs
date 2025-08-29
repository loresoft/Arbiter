using Arbiter.CommandQuery.Definitions;

namespace Arbiter.Benchmarks.Mapping;

public class Priority : IHaveIdentifier<string>, ITrackCreated, ITrackUpdated
{
    public string Id { get; set; } = Guid.CreateVersion7().ToString();

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;
}
