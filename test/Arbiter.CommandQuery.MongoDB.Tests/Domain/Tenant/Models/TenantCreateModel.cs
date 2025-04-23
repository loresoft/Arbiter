namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class TenantCreateModel
    : EntityCreateModel
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
