using System.Security.Claims;

using Arbiter.CommandQuery.EntityFramework.Tests.Constants;

namespace Arbiter.CommandQuery.EntityFramework.Tests;

public static class MockPrincipal
{
    static MockPrincipal()
    {
        Default = CreatePrincipal("william.adama@battlestar.com", "William Adama", UserConstants.WilliamAdama, TenantConstants.Test);
    }

    public static ClaimsPrincipal Default { get; }

    public static ClaimsPrincipal CreatePrincipal(string email, string name, int? userId = null, int? tenantId = null)
    {
        var claimsIdentity = new ClaimsIdentity("Identity.Application", ClaimTypes.Name, ClaimTypes.Role);
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, name));
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

        if (userId.HasValue)
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        if (tenantId.HasValue)
            claimsIdentity.AddClaim(new Claim("tenant_id", tenantId.Value.ToString()));

        return new ClaimsPrincipal(claimsIdentity);
    }
}
