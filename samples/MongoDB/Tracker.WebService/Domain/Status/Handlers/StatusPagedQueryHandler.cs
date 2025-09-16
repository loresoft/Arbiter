using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class StatusPagedQueryHandler
    : EntityPagedQueryHandler<StatusRepository, Status, string, IReadOnlyCollection<StatusReadModel>>
{
    public StatusPagedQueryHandler(ILoggerFactory loggerFactory, StatusRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<EntityPagedResult<IReadOnlyCollection<StatusReadModel>>?> Handle(EntityPagedQuery<IReadOnlyCollection<StatusReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
