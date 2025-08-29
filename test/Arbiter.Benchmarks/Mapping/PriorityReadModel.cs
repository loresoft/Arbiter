namespace Arbiter.Benchmarks.Mapping;

public partial class PriorityReadModel
    : Arbiter.CommandQuery.Models.EntityReadModel<string>
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
