using Arbiter.CommandQuery.Definitions;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

public class TaskNameModel : IHaveIdentifier<string>
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
}
