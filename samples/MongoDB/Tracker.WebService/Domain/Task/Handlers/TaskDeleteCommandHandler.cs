#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Commands;
using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.MongoDB.Handlers;

using Tracker.WebService.Data.Repositories;
using Tracker.WebService.Domain.Models;

using Task = Tracker.WebService.Data.Entities.Task;

namespace Tracker.WebService.Domain.Handlers;

[RegisterSingleton]
public class TaskDeleteCommandHandler
    : EntityDeleteCommandHandler<TaskRepository, Task, string, TaskReadModel>
{
    public TaskDeleteCommandHandler(ILoggerFactory loggerFactory, TaskRepository repository, IMapper mapper)
        : base(loggerFactory, repository, mapper)
    {

    }

    public override ValueTask<TaskReadModel?> Handle(EntityDeleteCommand<string, TaskReadModel> request, CancellationToken cancellationToken = default)
    {
        return base.Handle(request, cancellationToken);
    }
}
