using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class PriorityPagedQueryHandler
    : EntityPagedQueryHandler<PriorityRepository, Priority, string, IReadOnlyCollection<PriorityReadModel>>
{
    public PriorityPagedQueryHandler(ILoggerFactory loggerFactory, PriorityRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<EntityPagedResult<IReadOnlyCollection<PriorityReadModel>>?> Handle(EntityPagedQuery<IReadOnlyCollection<PriorityReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
