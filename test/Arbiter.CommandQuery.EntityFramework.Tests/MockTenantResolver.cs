using System.Security.Principal;

using Arbiter.CommandQuery.EntityFramework.Tests.Constants;

namespace Arbiter.CommandQuery.EntityFramework.Tests;

[RegisterSingleton<ITenantResolver<int>>]
public class MockTenantResolver : ITenantResolver<int>
{
    public ValueTask<int> GetTenantId(IPrincipal? principal)
    {
        var id = TenantConstants.Test;
        return ValueTask.FromResult(id);
    }
}
