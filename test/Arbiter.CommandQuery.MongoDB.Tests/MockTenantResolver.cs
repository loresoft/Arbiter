using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Tests.Constants;

namespace Arbiter.CommandQuery.MongoDB.Tests;

[RegisterSingleton<ITenantResolver<string>>]
public class MockTenantResolver : ITenantResolver<string>
{
    public ValueTask<string?> GetTenantId(ClaimsPrincipal? principal)
    {
        var id = TenantConstants.Test.Id;
        return ValueTask.FromResult<string?>(id);
    }
}
