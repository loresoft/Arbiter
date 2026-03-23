namespace Arbiter.Dispatcher.Client.Tests.Models;

public record PersonRecord(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string FullName,
    int Age,
    string? DepartmentName,
    int AddressCount
);
