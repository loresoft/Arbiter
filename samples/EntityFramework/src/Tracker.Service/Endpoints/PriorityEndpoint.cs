using Arbiter.CommandQuery.Endpoints;

using Tracker.Domain.Models;

namespace Tracker.Service.Endpoints;

[RegisterTransient<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class PriorityEndpoint : EntityCommandEndpointBase<int, PriorityReadModel, PriorityReadModel, PriorityCreateModel, PriorityUpdateModel>
{
    public PriorityEndpoint(ILoggerFactory loggerFactory) : base(loggerFactory, "Priority")
    {

    }
}

