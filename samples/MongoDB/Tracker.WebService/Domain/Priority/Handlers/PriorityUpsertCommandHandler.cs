using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class PriorityUpsertCommandHandler
    : EntityUpsertCommandHandler<PriorityRepository, Priority, string, PriorityUpdateModel, PriorityReadModel>
{
    public PriorityUpsertCommandHandler(ILoggerFactory loggerFactory, PriorityRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<PriorityReadModel?> Handle(EntityUpsertCommand<string, PriorityUpdateModel, PriorityReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
