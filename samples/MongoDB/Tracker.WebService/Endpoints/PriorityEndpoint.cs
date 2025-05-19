using Arbiter.CommandQuery.Endpoints;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Endpoints;

[RegisterSingleton<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class PriorityEndpoint : EntityCommandEndpointBase<string, PriorityReadModel, PriorityReadModel, PriorityCreateModel, PriorityUpdateModel>
{
    public PriorityEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Priority")
    { }
}
