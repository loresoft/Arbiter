using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class UserIdentifiersQueryHandler
    : EntityIdentifiersQueryHandler<UserRepository, User, string, IReadOnlyCollection<UserReadModel>>
{
    public UserIdentifiersQueryHandler(ILoggerFactory loggerFactory, UserRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<IReadOnlyCollection<IReadOnlyCollection<UserReadModel>>?> Handle(EntityIdentifiersQuery<string, IReadOnlyCollection<UserReadModel>> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
