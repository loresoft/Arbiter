#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;
using Arbiter.CommandQuery.Queries;

using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

using Task = Tracker.WebService.Data.Entities.Task;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class TaskIdentifierQueryHandler
    : EntityIdentifierQueryHandler<TaskRepository, Task, string, TaskReadModel>
{
    public TaskIdentifierQueryHandler(ILoggerFactory loggerFactory, TaskRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<TaskReadModel?> Handle(EntityIdentifierQuery<string, TaskReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
