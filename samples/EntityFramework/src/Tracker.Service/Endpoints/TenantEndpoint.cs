using Arbiter.CommandQuery.Endpoints;

using Tracker.Domain.Models;

namespace Tracker.Service.Endpoints;

[RegisterTransient<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class TenantEndpoint : EntityCommandEndpointBase<int, TenantReadModel, TenantReadModel, TenantCreateModel, TenantUpdateModel>
{
    public TenantEndpoint(ILoggerFactory loggerFactory) : base(loggerFactory, "Tenant")
    {

    }
}

