using System.Security.Claims;
using System.Security.Principal;

namespace Tracker.Extensions;

public static class ClaimsPrincipalExtensions
{
    public const string ObjectIdenttifier = "oid";

    public const string Subject = "sub";
    public const string NameClaim = "name";
    public const string EmailClaim = "email";
    public const string EmailsClaim = "emails";
    public const string ProviderClaim = "idp";
    public const string PreferredUserName = "preferred_username";

    public const string IdentityClaim = "http://schemas.microsoft.com/identity/claims/identityprovider";
    public const string IdentifierClaim = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    public static string? GetUserEmail(this IPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ClaimTypes.Email)
            ?? claimPrincipal?.FindFirst(EmailClaim)
            ?? claimPrincipal?.FindFirst(EmailsClaim);

        return claim?.Value;
    }

    public static string? GetIdentifier(this IPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

        return claim?.Value;
    }

    public static Guid? GetObjectId(this IPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(IdentifierClaim)
            ?? claimPrincipal?.FindFirst(ObjectIdenttifier)
            ?? claimPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

        return Guid.TryParse(claim?.Value, out var oid) ? oid : null;
    }

    public static string? GetName(this IPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(NameClaim)
            ?? claimPrincipal?.FindFirst(ClaimTypes.Name)
            ?? claimPrincipal?.FindFirst(Subject);

        return claim?.Value ?? claimPrincipal?.Identity?.Name;
    }

    public static string? GetProvider(this IPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ProviderClaim)
            ?? claimPrincipal?.FindFirst(IdentityClaim);

        return claim?.Value;
    }

    public static string? GetUserName(this IPrincipal principal)
    {
        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(PreferredUserName)
            ?? claimPrincipal?.FindFirst(ClaimTypes.Name)
            ?? claimPrincipal?.FindFirst(Subject);

        return claim?.Value;
    }
}
