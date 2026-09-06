using System.Security.Claims;

using Arbiter.CommandQuery.Extensions;
using Arbiter.CommandQuery.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Arbiter.Components.Abstracts;

/// <summary>
/// Provides a base class for components that need access to the current user, their claims, roles and
/// authorization policies.
/// </summary>
/// <remarks>
/// The current user is read from the cascading <see cref="AuthenticationState"/> supplied by
/// <c>CascadingAuthenticationState</c>. When that cascading value is not available, every member reports the
/// user as unknown rather than throwing, so a component derived from this class can still render outside an
/// authenticated context.
/// </remarks>
public abstract class PrincipalComponentBase : ComponentBase
{
    private ILogger? _logger;

    /// <summary>
    /// Gets or sets the cascading authentication state for the current user.
    /// </summary>
    /// <value>
    /// The task that resolves the current authentication state, or <see langword="null"/> when no
    /// <c>CascadingAuthenticationState</c> is present above this component.
    /// </value>
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationState { get; set; }

    /// <summary>
    /// Gets or sets the service used to evaluate authorization policies.
    /// </summary>
    [Inject]
    protected IAuthorizationService AuthorizationService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the factory used to create the <see cref="Logger"/> instance.
    /// </summary>
    [Inject]
    protected ILoggerFactory LoggerFactory { get; set; } = default!;

    /// <summary>
    /// Gets the logger available to this component and derived pages.
    /// </summary>
    /// <value>
    /// A logger whose category is the runtime type of the component, so log entries can be filtered by the
    /// derived component rather than by <see cref="PrincipalComponentBase"/>.
    /// </value>
    protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The <see cref="ClaimsPrincipal"/> for the current user, or <see langword="null"/> when the authentication state is
    /// not available
    /// </returns>
    /// <remarks>
    /// The returned principal may represent an anonymous user; check
    /// <see cref="ClaimsIdentity.IsAuthenticated"/> when that distinction matters.
    /// </remarks>
    protected async Task<ClaimsPrincipal?> GetUser()
    {
        if (AuthenticationState is null)
            return null;

        var user = await AuthenticationState;
        return user?.User;
    }

    /// <summary>
    /// Gets the identifier of the currently authenticated user.
    /// </summary>
    /// <typeparam name="TKey">The type of the user identifier</typeparam>
    /// <returns>
    /// The value of the user identifier claim converted to <typeparamref name="TKey"/>, or the default value
    /// when the authentication state is not available, there is no user, or the claim is missing
    /// </returns>
    /// <remarks>
    /// The value is read from the <see cref="ClaimNames.UserId"/> claim, named <c>user_id</c>, and is parsed
    /// using <see cref="IParsable{TSelf}"/>. The claim must be issued by the identity provider or added when
    /// the principal is built; the standard <c>sub</c> and <c>NameIdentifier</c> claims are not used.
    /// </remarks>
    protected async Task<TKey?> GetUserId<TKey>()
        where TKey : IParsable<TKey>
    {
        var user = await GetUser();
        if (user is null)
            return default;

        return user.GetValue<TKey>(ClaimNames.UserId);
    }

    /// <summary>
    /// Determines whether the currently authenticated user satisfies the specified authorization policy.
    /// </summary>
    /// <param name="policyName">The name of the authorization policy to evaluate</param>
    /// <returns>
    /// <see langword="true"/> when the user satisfies the policy; otherwise <see langword="false"/>. Also returns <see langword="false"/> when
    /// the authentication state is not available or there is no user.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="policyName"/> is <see langword="null"/>, empty, or white space
    /// </exception>
    protected async Task<bool> HasPolicy(string policyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var user = await GetUser();
        if (user is null)
            return false;

        var result = await AuthorizationService.AuthorizeAsync(user, policyName);

        return result.Succeeded;
    }

    /// <summary>
    /// Determines whether the currently authenticated user is a member of the specified role.
    /// </summary>
    /// <param name="roleName">The name of the role to check</param>
    /// <returns>
    /// <see langword="true"/> when the user is a member of the role; otherwise <see langword="false"/>. Also returns <see langword="false"/>
    /// when the authentication state is not available or there is no user.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="roleName"/> is <see langword="null"/>, empty, or white space
    /// </exception>
    /// <remarks>
    /// Prefer <see cref="HasPolicy(string)"/> where a policy is defined, as policies keep authorization rules
    /// out of the component.
    /// </remarks>
    protected async Task<bool> HasRole(string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName);

        var user = await GetUser();
        if (user is null)
            return false;

        return user.IsInRole(roleName);
    }
}
