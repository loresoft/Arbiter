namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class TenantReadModel
    : EntityReadModel
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
