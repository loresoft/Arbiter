using System.Security.Claims;

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Models;

namespace Tracker.WebService.Domain.Task.Commands;

public record TaskProcessCommand : PrincipalCommandBase<CompleteModel>
{
    public TaskProcessCommand(ClaimsPrincipal? principal, string action) : base(principal)
    {
        Action = action;
    }

    public string Action { get; }
}
