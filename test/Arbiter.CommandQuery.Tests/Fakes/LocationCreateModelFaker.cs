using Arbiter.CommandQuery.Tests.Models;

using Bogus;

namespace Arbiter.CommandQuery.Tests.Fakes;

public class LocationCreateModelFaker : Faker<LocationCreateModel>
{
    public LocationCreateModelFaker()
    {
        RuleFor(p => p.Id, f => f.IndexFaker + 1);
        RuleFor(p => p.Name, f => f.Company.CompanyName());
        RuleFor(p => p.Description, f => f.Lorem.Sentence());
        RuleFor(p => p.AddressLine1, f => f.Address.StreetAddress());
        RuleFor(p => p.AddressLine2, f => f.Address.SecondaryAddress());
        RuleFor(p => p.AddressLine3, f => f.Address.StreetAddress());
        RuleFor(p => p.City, f => f.Address.City());
        RuleFor(p => p.StateProvince, f => f.Address.State());
        RuleFor(p => p.PostalCode, f => f.Address.ZipCode());
        RuleFor(p => p.Latitude, f => f.Address.Latitude());
        RuleFor(p => p.Longitude, f => f.Address.Longitude());
        RuleFor(p => p.Created, f => f.Date.Past());
        RuleFor(p => p.CreatedBy, f => f.Internet.UserName());
        RuleFor(p => p.Updated, f => f.Date.Past());
        RuleFor(p => p.UpdatedBy, f => f.Internet.UserName());
    }
}
