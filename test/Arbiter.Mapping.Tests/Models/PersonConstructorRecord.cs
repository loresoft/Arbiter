namespace Arbiter.CommandQuery.Tests.Models;

public record PersonConstructorRecord(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string FullName,
    int Age,
    string? DepartmentName,
    int AddressCount);
