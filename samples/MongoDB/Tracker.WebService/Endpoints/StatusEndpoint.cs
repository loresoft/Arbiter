using Arbiter.CommandQuery.Endpoints;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Endpoints;

[RegisterSingleton<IFeatureEndpoint>(Duplicate = DuplicateStrategy.Append)]
public class StatusEndpoint : EntityCommandEndpointBase<string, StatusReadModel, StatusReadModel, StatusCreateModel, StatusUpdateModel>
{
    public StatusEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Status")
    { }
}
