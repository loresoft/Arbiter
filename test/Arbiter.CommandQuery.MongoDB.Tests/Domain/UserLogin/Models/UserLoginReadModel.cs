namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public partial class UserLoginReadModel
    : EntityReadModel
{
    public string? EmailAddress { get; set; }

    public string? UserId { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public bool IsSuccessful { get; set; }

    public string? FailureMessage { get; set; }
}
