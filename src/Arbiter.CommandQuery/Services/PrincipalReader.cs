using System.Security.Claims;
using System.Security.Principal;
using System.Xml.Linq;

using Arbiter.CommandQuery.Definitions;

using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.Services;

/// <summary>
/// A service to read claims from a principal.
/// </summary>
public sealed partial class PrincipalReader : IPrincipalReader
{
    private readonly ILogger<PrincipalReader> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrincipalReader"/> class.
    /// </summary>
    /// <param name="logger">The logger for this service</param>
    public PrincipalReader(ILogger<PrincipalReader> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string? GetEmail(IPrincipal? principal)
    {
        if (principal is null)
            return null;

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ClaimTypes.Email)
            ?? claimPrincipal?.FindFirst(ClaimNames.EmailClaim)
            ?? claimPrincipal?.FindFirst(ClaimNames.EmailsClaim);

        var email = claim?.Value;

        LogPrincipal(_logger, "Email", email);

        return email;
    }

    /// <inheritdoc />
    public string? GetIdentifier(IPrincipal? principal)
    {
        if (principal is null)
            return null;

        var name = principal?.Identity?.Name;

        LogPrincipal(_logger, "Identifier", name);

        return name;
    }

    /// <inheritdoc />
    public string? GetName(IPrincipal? principal)
    {
        if (principal is null)
            return null;

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ClaimNames.NameClaim)
            ?? claimPrincipal?.FindFirst(ClaimTypes.Name)
            ?? claimPrincipal?.FindFirst(ClaimNames.Subject);

        var name = claim?.Value ?? principal.Identity?.Name;

        LogPrincipal(_logger, "Name", name);

        return name;
    }

    /// <inheritdoc />
    public string? GetDisplayName(ClaimsPrincipal? claimsPrincipal)
    {
        var claim = claimsPrincipal?.FindFirst(ClaimNames.DisplayName)
            ?? claimsPrincipal?.FindFirst(ClaimNames.NameClaim)
            ?? claimsPrincipal?.FindFirst(ClaimTypes.Name)
            ?? claimsPrincipal?.FindFirst(ClaimNames.PreferredUserName)
            ?? claimsPrincipal?.FindFirst(ClaimNames.Subject);

        return claim?.Value ?? claimsPrincipal?.Identity?.Name;
    }

    /// <inheritdoc />
    public Guid? GetObjectId(IPrincipal? principal)
    {
        if (principal is null)
            return null;

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ClaimNames.IdentifierClaim)
            ?? claimPrincipal?.FindFirst(ClaimNames.ObjectIdentifier)
            ?? claimPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

        return Guid.TryParse(claim?.Value, out var oid) ? oid : null;
    }

    /// <inheritdoc />
    public string? GetTenantId(IPrincipal? principal)
    {
        if (principal is null)
            return null;

        var claimPrincipal = principal as ClaimsPrincipal;
        var claim = claimPrincipal?.FindFirst(ClaimNames.TenantId);

        return claim?.Value;
    }


    [LoggerMessage(1, LogLevel.Trace, "Resolved principal claim {Type}: {Value}")]
    static partial void LogPrincipal(ILogger logger, string type, string? value);
}
