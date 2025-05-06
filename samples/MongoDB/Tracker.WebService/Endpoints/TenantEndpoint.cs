using Arbiter.CommandQuery.Endpoints;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Endpoints;

[RegisterSingleton<IFeatureEndpoint>(Duplicate = DuplicateStrategy.Append)]
public class TenantEndpoint : EntityCommandEndpointBase<string, TenantReadModel, TenantReadModel, TenantCreateModel, TenantUpdateModel>
{
    public TenantEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "Tenant")
    { }
}
