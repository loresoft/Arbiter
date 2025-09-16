using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class StatusIdentifiersQueryHandler
    : EntityIdentifiersQueryHandler<StatusRepository, Status, string, IReadOnlyCollection<StatusReadModel>>
{
    public StatusIdentifiersQueryHandler(ILoggerFactory loggerFactory, StatusRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<IReadOnlyCollection<IReadOnlyCollection<StatusReadModel>>?> Handle(EntityIdentifiersQuery<string, IReadOnlyCollection<StatusReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
