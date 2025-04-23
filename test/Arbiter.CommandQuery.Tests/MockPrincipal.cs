using System.Security.Claims;

namespace Arbiter.CommandQuery.Tests;

public static class MockPrincipal
{
    static MockPrincipal()
    {
        Default = CreatePrincipal("william.adama@battlestar.com", "William Adama");
    }

    public static ClaimsPrincipal Default { get; }


    public static ClaimsPrincipal CreatePrincipal(string email, string name)
    {
        var claimsIdentity = new ClaimsIdentity("Identity.Application", ClaimTypes.Name, ClaimTypes.Role);
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, name));
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

        return new ClaimsPrincipal(claimsIdentity);
    }
}
