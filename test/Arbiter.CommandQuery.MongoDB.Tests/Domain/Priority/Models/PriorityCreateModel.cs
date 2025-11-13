namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class PriorityCreateModel
    : EntityCreateModel
{
    public Guid Key { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
