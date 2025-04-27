using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Tracker.Client.Components.Abstracts;

public abstract class PrincipalBase : ComponentBase
{
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }

    protected async Task<ClaimsPrincipal?> GetUser()
    {
        if (AuthenticationState is null)
            return null;

        var user = await AuthenticationState;
        return user?.User;
    }
}
