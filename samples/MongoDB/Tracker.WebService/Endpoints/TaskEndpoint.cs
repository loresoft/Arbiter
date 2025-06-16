using System.Security.Claims;

using Arbiter.CommandQuery.Endpoints;
using Arbiter.CommandQuery.Models;
using Arbiter.Mediation;

using Microsoft.AspNetCore.Http.HttpResults;

using Microsoft.AspNetCore.Mvc;

using Tracker.WebService.Domain.Models;
using Tracker.WebService.Domain.Task.Commands;

namespace Tracker.WebService.Endpoints;

[RegisterSingleton<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class TaskEndpoint : EntityCommandEndpointBase<string, TaskReadModel, TaskReadModel, TaskCreateModel, TaskUpdateModel>
{
    public TaskEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Task")
    { }

    protected override void MapGroup(RouteGroupBuilder group)
    {
        base.MapGroup(group);

        group
            .MapGet("Process", GetProcessCommand)
            .WithEntityMetadata(EntityName)
            .WithName($"Get{EntityName}Process")
            .WithSummary("Get entity process action")
            .WithDescription("Get entity process action");

    }

    private async Task<Results<Ok<CompleteModel>, ProblemHttpResult>> GetProcessCommand(
       [FromServices] IMediator mediator,
       [FromQuery] string? action = null,
       ClaimsPrincipal? user = default,
       CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new TaskProcessCommand(user, action!);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);

            return TypedResults.Ok(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error GetQuery: {ErrorMessage}", ex.Message);

            var details = ex.ToProblemDetails();
            return TypedResults.Problem(details);
        }
    }
}
