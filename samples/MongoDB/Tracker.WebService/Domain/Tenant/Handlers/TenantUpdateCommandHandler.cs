using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;


namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class TenantUpdateCommandHandler
    : EntityUpdateCommandHandler<TenantRepository, Tenant, string, TenantUpdateModel, TenantReadModel>
{
    public TenantUpdateCommandHandler(ILoggerFactory loggerFactory, TenantRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<TenantReadModel?> Handle(EntityUpdateCommand<string, TenantUpdateModel, TenantReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
