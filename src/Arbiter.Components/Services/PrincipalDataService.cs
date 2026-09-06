using System.Security.Claims;

using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Client;

using Microsoft.AspNetCore.Components.Authorization;

namespace Arbiter.Components.Services;

/// <summary>
/// A <see cref="DispatcherDataService"/> that resolves the current user from the Blazor authentication state.
/// </summary>
/// <remarks>
/// <para>
/// The base data service stamps outgoing commands with the user that issued them. This implementation supplies
/// that user from the Blazor authentication state, so the same service works for both server interactive and
/// WebAssembly rendering.
/// </para>
/// <para>
/// The authentication state task is preferred when one is registered, because it is the same state cascaded to
/// the components by <c>CascadingAuthenticationState</c> and is safe to read outside of a rendering context.
/// Otherwise the <see cref="AuthenticationStateProvider"/> is used, which some providers only allow while a
/// component is rendering; a provider that refuses the request is treated as an unknown user rather than
/// failing the command.
/// </para>
/// </remarks>
public class PrincipalDataService : DispatcherDataService
{
    private readonly AuthenticationStateProvider? _authenticationStateProvider;
    private readonly Task<AuthenticationState>? _authenticationState;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrincipalDataService"/> class.
    /// </summary>
    /// <param name="dispatcher">The dispatcher used to send the requests</param>
    /// <param name="authenticationStateProvider">
    /// The provider the current user is read from when <paramref name="authenticationState"/> is not available
    /// </param>
    /// <param name="authenticationState">
    /// The authentication state the current user is preferred from, or <see langword="null"/> when it is not registered
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// When both <paramref name="authenticationStateProvider"/> and <paramref name="authenticationState"/> are
    /// <see langword="null"/>
    /// </exception>
    public PrincipalDataService(
        IDispatcher dispatcher,
        AuthenticationStateProvider? authenticationStateProvider = null,
        Task<AuthenticationState>? authenticationState = null)
        : base(dispatcher)
    {
        if (authenticationStateProvider is null && authenticationState is null)
            throw new ArgumentNullException(nameof(authenticationStateProvider), "An authentication state provider or an authentication state is required.");

        _authenticationStateProvider = authenticationStateProvider;
        _authenticationState = authenticationState;
    }

    /// <inheritdoc />
    /// <remarks>
    /// The returned principal may represent an anonymous user; check
    /// <see cref="System.Security.Principal.IIdentity.IsAuthenticated"/> when that distinction matters.
    /// </remarks>
    public override async ValueTask<ClaimsPrincipal?> GetUser(CancellationToken cancellationToken = default)
    {
        // prefer the registered state, it is the same state cascaded to the components
        if (_authenticationState != null)
        {
            var cascadedState = await _authenticationState.ConfigureAwait(false);
            return cascadedState?.User;
        }

        if (_authenticationStateProvider is null)
            return null;

        try
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync().ConfigureAwait(false);
            return state?.User;
        }
        catch (InvalidOperationException)
        {
            // the provider only supplies the state while a component is rendering; treat as an unknown user
            return null;
        }
    }
}
