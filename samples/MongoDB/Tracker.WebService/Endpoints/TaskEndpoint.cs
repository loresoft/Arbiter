using Arbiter.CommandQuery.Endpoints;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Endpoints;

[RegisterSingleton<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class TaskEndpoint : EntityCommandEndpointBase<string, TaskReadModel, TaskReadModel, TaskCreateModel, TaskUpdateModel>
{
    public TaskEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Task")
    { }
}
