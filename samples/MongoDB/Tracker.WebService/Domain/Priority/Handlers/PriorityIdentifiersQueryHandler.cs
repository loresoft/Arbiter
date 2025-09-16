using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class PriorityIdentifiersQueryHandler
    : EntityIdentifiersQueryHandler<PriorityRepository, Priority, string, IReadOnlyCollection<PriorityReadModel>>
{
    public PriorityIdentifiersQueryHandler(ILoggerFactory loggerFactory, PriorityRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<IReadOnlyCollection<IReadOnlyCollection<PriorityReadModel>>?> Handle(EntityIdentifiersQuery<string, IReadOnlyCollection<PriorityReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
