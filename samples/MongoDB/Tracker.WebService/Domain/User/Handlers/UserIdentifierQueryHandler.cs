using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class UserIdentifierQueryHandler
    : EntityIdentifierQueryHandler<UserRepository, User, string, UserReadModel>
{
    public UserIdentifierQueryHandler(ILoggerFactory loggerFactory, UserRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<UserReadModel?> Handle(EntityIdentifierQuery<string, UserReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
