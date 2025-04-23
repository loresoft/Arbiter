using System.Security.Claims;

namespace Arbiter.CommandQuery.EntityFramework.Tests;

public static class MockPrincipal
{
    static MockPrincipal()
    {
        Default = CreatePrincipal("william.adama@battlestar.com", "William Adama"/*, UserConstants.WilliamAdama.Id, TenantConstants.Test.Id*/);
    }

    public static ClaimsPrincipal Default { get; }

    public static ClaimsPrincipal CreatePrincipal(string email, string name, string? userId = null, string? tenantId = null)
    {
        var claimsIdentity = new ClaimsIdentity("Identity.Application", ClaimTypes.Name, ClaimTypes.Role);
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, name));
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

        if (!string.IsNullOrEmpty(userId))
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        if (!string.IsNullOrEmpty(tenantId))
            claimsIdentity.AddClaim(new Claim("tenant_id", tenantId));

        return new ClaimsPrincipal(claimsIdentity);
    }
}
