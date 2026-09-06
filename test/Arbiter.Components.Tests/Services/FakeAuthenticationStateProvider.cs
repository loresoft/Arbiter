using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace Arbiter.Components.Tests.Services;

/// <summary>
/// An <see cref="AuthenticationStateProvider"/> that returns a supplied state, or throws to mimic a provider that
/// only supplies the state while a component is rendering.
/// </summary>
internal sealed class FakeAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal? _user;
    private readonly bool _throwWhenRequested;

    public FakeAuthenticationStateProvider(ClaimsPrincipal? user, bool throwWhenRequested = false)
    {
        _user = user;
        _throwWhenRequested = throwWhenRequested;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_throwWhenRequested)
            throw new InvalidOperationException("The authentication state is only available while rendering.");

        var principal = _user ?? new ClaimsPrincipal(new ClaimsIdentity());

        return Task.FromResult(new AuthenticationState(principal));
    }
}
