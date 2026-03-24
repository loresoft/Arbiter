namespace Arbiter.CommandQuery.Tests.Models;

public class PersonModel
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }

    // Navigation properties
    public string? DepartmentName { get; set; }
    public int AddressCount { get; set; }

}
