using System.Security.Claims;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Models;

using MessagePack;

namespace Arbiter.CommandQuery.Commands;

/// <summary>
/// Represents a command that requests a health report for the current system.
/// </summary>
[MessagePackObject(true)]
public record HealthCheckCommand : PrincipalCommandBase<HealthReportModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckCommand"/> class with the specified principal.
    /// </summary>
    /// <param name="principal">
    /// The security principal associated with the command, or <see langword="null"/> when no principal is provided.
    /// </param>
    public HealthCheckCommand(ClaimsPrincipal? principal) : base(principal)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckCommand"/> class for serialization.
    /// </summary>
    [JsonConstructor]
    [SerializationConstructor]
    public HealthCheckCommand()
        : this(principal: null)
    {
    }
}
