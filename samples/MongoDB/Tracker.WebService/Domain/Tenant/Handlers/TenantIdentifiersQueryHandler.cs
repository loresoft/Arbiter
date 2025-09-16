using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;


namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class TenantIdentifiersQueryHandler
    : EntityIdentifiersQueryHandler<TenantRepository, Tenant, string, IReadOnlyCollection<TenantReadModel>>
{
    public TenantIdentifiersQueryHandler(ILoggerFactory loggerFactory, TenantRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<IReadOnlyCollection<IReadOnlyCollection<TenantReadModel>>?> Handle(EntityIdentifiersQuery<string, IReadOnlyCollection<TenantReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
