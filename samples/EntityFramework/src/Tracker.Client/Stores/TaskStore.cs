using Arbiter.CommandQuery.Definitions;

using Tracker.Domain.Models;

namespace Tracker.Client.Stores;

[RegisterScoped]
public class TaskStore : Abstracts.StoreEditBase<TaskReadModel, TaskUpdateModel>
{
    public TaskStore(ILoggerFactory loggerFactory, Services.DataService dataService, IMapper mapper)
        : base(loggerFactory, dataService, mapper)
    {
    }

}

