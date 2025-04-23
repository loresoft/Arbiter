namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class AuditReadModel
    : EntityReadModel
{
    public DateTime Date { get; set; }

    public string? UserId { get; set; }

    public string? TaskId { get; set; }

    public string Content { get; set; } = null!;

    public string Username { get; set; } = null!;
}
