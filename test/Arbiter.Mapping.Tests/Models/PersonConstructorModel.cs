namespace Arbiter.CommandQuery.Tests.Models;

public class PersonConstructorModel(
    int id,
    string firstName,
    string lastName,
    string email,
    string fullName,
    int age,
    string? departmentName,
    int addressCount)
{
    public int Id { get; } = id;
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string Email { get; } = email;
    public string FullName { get; } = fullName;
    public int Age { get; } = age;
    public string? DepartmentName { get; } = departmentName;
    public int AddressCount { get; } = addressCount;
}
