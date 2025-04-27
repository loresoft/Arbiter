using Arbiter.CommandQuery.Definitions;

using Tracker.Domain.Models;

namespace Tracker.Client.Stores;

[RegisterScoped]
public class PriorityStore : Abstracts.StoreEditBase<PriorityReadModel, PriorityUpdateModel>
{
    public PriorityStore(ILoggerFactory loggerFactory, Services.DataService dataService, IMapper mapper)
        : base(loggerFactory, dataService, mapper)
    {
    }

}

