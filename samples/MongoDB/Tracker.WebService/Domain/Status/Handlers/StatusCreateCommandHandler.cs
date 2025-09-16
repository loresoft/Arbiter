using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class StatusCreateCommandHandler
    : EntityCreateCommandHandler<StatusRepository, Status, string, StatusCreateModel, StatusReadModel>
{
    public StatusCreateCommandHandler(ILoggerFactory loggerFactory, StatusRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<StatusReadModel?> Handle(EntityCreateCommand<StatusCreateModel, StatusReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
