#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Behaviors;
using Arbiter.CommandQuery.Handlers;
using Arbiter.CommandQuery.Models;
using Arbiter.Mediation;

using Tracker.WebService.Domain.Task.Commands;

namespace Tracker.WebService.Domain.Handlers;

[RegisterTransient<IRequestHandler<TaskProcessCommand, CompleteModel>>]
//[RegisterTransient<IPipelineBehavior<TaskProcessCommand, CompleteModel>, ValidateCommandBehavior<TaskProcessCommand, CompleteModel>>]
public class TaskProcessHandler : RequestHandlerBase<TaskProcessCommand, CompleteModel>
{
    public TaskProcessHandler(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    protected override ValueTask<CompleteModel?> Process(TaskProcessCommand request, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Processing Task: {action} for {principal}", request.Action, request.Principal?.Identity?.Name);

        var result = CompleteModel.Success($"Processing Task: {request.Action} for {request.Principal?.Identity?.Name}");

        return ValueTask.FromResult<CompleteModel?>(result);
    }
}
