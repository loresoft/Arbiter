using Arbiter.CommandQuery.Models;

namespace Arbiter.CommandQuery.Tests.Models;

public class LocationCreateModel : EntityCreateModel<int>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}


