namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class PriorityReadModel
    : EntityReadModel
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
