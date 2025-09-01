using Arbiter.CommandQuery.Endpoints;

using Tracker.Domain.Models;

namespace Tracker.Service.Endpoints;

[RegisterTransient<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class TaskEndpoint : EntityCommandEndpointBase<int, TaskReadModel, TaskReadModel, TaskCreateModel, TaskUpdateModel>
{
    public TaskEndpoint(ILoggerFactory loggerFactory) : base(loggerFactory, "Task")
    {

    }
}

