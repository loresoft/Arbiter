using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;


namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class TenantPagedQueryHandler
    : EntityPagedQueryHandler<TenantRepository, Tenant, string, IReadOnlyCollection<TenantReadModel>>
{
    public TenantPagedQueryHandler(ILoggerFactory loggerFactory, TenantRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<EntityPagedResult<IReadOnlyCollection<TenantReadModel>>?> Handle(EntityPagedQuery<IReadOnlyCollection<TenantReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
