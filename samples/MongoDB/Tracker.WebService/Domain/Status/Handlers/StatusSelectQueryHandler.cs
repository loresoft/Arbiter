using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class StatusSelectQueryHandler
    : EntitySelectQueryHandler<StatusRepository, Status, string, IReadOnlyCollection<StatusReadModel>>
{
    public StatusSelectQueryHandler(ILoggerFactory loggerFactory, StatusRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<IReadOnlyCollection<IReadOnlyCollection<StatusReadModel>>?> Handle(EntitySelectQuery<IReadOnlyCollection<StatusReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
