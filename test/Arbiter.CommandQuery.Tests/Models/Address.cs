namespace Arbiter.CommandQuery.Tests.Models;

public class Address
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }

    // Foreign key
    public int PersonId { get; set; }

    // Navigation property
    public Person Person { get; set; } = null!;
}
