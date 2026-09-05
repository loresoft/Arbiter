using System.Security.Claims;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Extensions;

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
    public string? GetEmail(ClaimsPrincipal? principal)
    {
        var email = principal.GetValue(ClaimTypes.Email)
            ?? principal.GetValue(ClaimNames.EmailClaim)
            ?? principal.GetValue(ClaimNames.EmailsClaim);

        LogPrincipal(_logger, "Email", email);

        return email;
    }

    /// <inheritdoc />
    public string? GetIdentifier(ClaimsPrincipal? principal)
    {
        var name = principal?.Identity?.Name;

        LogPrincipal(_logger, "Identifier", name);

        return name;
    }

    /// <inheritdoc />
    public string? GetName(ClaimsPrincipal? principal)
    {
        var name = principal.GetValue(ClaimNames.NameClaim)
            ?? principal.GetValue(ClaimTypes.Name)
            ?? principal.GetValue(ClaimNames.Subject)
            ?? principal?.Identity?.Name;

        LogPrincipal(_logger, "Name", name);

        return name;
    }

    /// <inheritdoc />
    public string? GetDisplayName(ClaimsPrincipal? principal)
    {
        var displayName = principal.GetValue(ClaimNames.DisplayName)
            ?? principal.GetValue(ClaimNames.NameClaim)
            ?? principal.GetValue(ClaimTypes.Name)
            ?? principal.GetValue(ClaimNames.PreferredUserName)
            ?? principal.GetValue(ClaimNames.Subject)
            ?? principal?.Identity?.Name;

        LogPrincipal(_logger, "DisplayName", displayName);

        return displayName;
    }

    /// <inheritdoc />
    public Guid? GetObjectId(ClaimsPrincipal? principal)
    {
        var value = principal.GetValue(ClaimNames.IdentifierClaim)
            ?? principal.GetValue(ClaimNames.ObjectIdentifier)
            ?? principal.GetValue(ClaimTypes.NameIdentifier);

        LogPrincipal(_logger, "ObjectId", value);

        return Guid.TryParse(value, out var objectId) ? objectId : null;
    }

    /// <inheritdoc />
    public string? GetUserId(ClaimsPrincipal? principal)
    {
        var userId = principal.GetValue(ClaimNames.UserId);

        LogPrincipal(_logger, "UserId", userId);

        return userId;
    }

    /// <inheritdoc />
    public string? GetTenantId(ClaimsPrincipal? principal)
    {
        var tenantId = principal.GetValue(ClaimNames.TenantId);

        LogPrincipal(_logger, "TenantId", tenantId);

        return tenantId;
    }


    [LoggerMessage(1, LogLevel.Trace, "Resolved principal claim {Type}: {Value}")]
    static partial void LogPrincipal(ILogger logger, string type, string? value);

}
