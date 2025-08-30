namespace Arbiter.CommandQuery.Tests.Models;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public DateTime FoundedDate { get; set; }
    public int EmployeeCount { get; set; }
    public string Website { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}
