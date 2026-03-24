namespace Arbiter.CommandQuery.Tests.Models;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public decimal Budget { get; set; }

    // Foreign key
    public int? CompanyId { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
    public ICollection<Person> Employees { get; set; } = new List<Person>();
}
