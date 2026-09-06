using System.Security.Claims;

using Arbiter.Components.Abstracts;

namespace Arbiter.Components.Tests.Abstracts;

/// <summary>
/// A concrete <see cref="PrincipalComponentBase"/> exposing the protected members to the tests.
/// </summary>
public class TestPrincipalPage : PrincipalComponentBase
{
    public Task<ClaimsPrincipal?> PublicGetUser() => GetUser();

    public Task<int> PublicGetUserId() => GetUserId<int>();

    public Task<bool> PublicHasPolicy(string policyName) => HasPolicy(policyName);

    public Task<bool> PublicHasRole(string roleName) => HasRole(roleName);

    public bool HasLogger => Logger is not null;
}
