using System.Security.Claims;

using Arbiter.CommandQuery.EntityFramework.Tests.Constants;

namespace Arbiter.CommandQuery.EntityFramework.Tests;

[RegisterSingleton<ITenantResolver<int>>]
public class MockTenantResolver : ITenantResolver<int>
{
    public ValueTask<int> GetTenantId(ClaimsPrincipal? principal)
    {
        var id = TenantConstants.Test;
        return ValueTask.FromResult(id);
    }
}
