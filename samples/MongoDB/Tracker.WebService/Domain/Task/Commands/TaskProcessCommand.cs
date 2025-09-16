using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Models;

namespace Tracker.WebService.Domain.Commands;

public record TaskProcessCommand : PrincipalCommandBase
{
    public TaskProcessCommand(ClaimsPrincipal? principal, string action) : base(principal)
    {
        Action = action;
    }

    public string Action { get; }
}
