using Tracker.WebService.Data.Entities;

namespace Tracker.WebService.Constants;

public static class TenantConstants
{
    ///<summary>Battlestar Galactica</summary>
    public static readonly Tenant Battlestar = new() { Id = "607a27dde412d2a66dd558fb", Name = "Battlestar Galactica", Description = "Battlestar Galactica", IsActive = true };
    ///<summary>Cylons</summary>
    public static readonly Tenant Cylons = new() { Id = "681a2ff92f6a1d7e2957ae5b", Name = "Cylons", Description = "Cylons", IsActive = true };
}
