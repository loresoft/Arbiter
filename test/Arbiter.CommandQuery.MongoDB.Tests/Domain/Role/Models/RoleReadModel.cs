namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class RoleReadModel
    : EntityReadModel
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
