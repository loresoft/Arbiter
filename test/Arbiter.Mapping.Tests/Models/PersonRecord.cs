namespace Arbiter.CommandQuery.Tests.Models;

public record PersonRecord
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public int Age { get; init; }
    public string? DepartmentName { get; init; }
    public int AddressCount { get; init; }
}
