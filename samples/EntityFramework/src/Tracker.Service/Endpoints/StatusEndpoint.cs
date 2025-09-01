using Arbiter.CommandQuery.Endpoints;

using Tracker.Domain.Models;

namespace Tracker.Service.Endpoints;

[RegisterTransient<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class StatusEndpoint : EntityCommandEndpointBase<int, StatusReadModel, StatusReadModel, StatusCreateModel, StatusUpdateModel>
{
    public StatusEndpoint(ILoggerFactory loggerFactory) : base(loggerFactory, "Status")
    {

    }
}

