using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;


namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class TenantCreateCommandHandler
    : EntityCreateCommandHandler<TenantRepository, Tenant, string, TenantCreateModel, TenantReadModel>
{
    public TenantCreateCommandHandler(ILoggerFactory loggerFactory, TenantRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<TenantReadModel?> Handle(EntityCreateCommand<TenantCreateModel, TenantReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
