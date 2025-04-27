using Arbiter.CommandQuery.Definitions;

using Tracker.Domain.Models;

namespace Tracker.Client.Stores;

[RegisterScoped]
public class StatusStore : Abstracts.StoreEditBase<StatusReadModel, StatusUpdateModel>
{
    public StatusStore(ILoggerFactory loggerFactory, Services.DataService dataService, IMapper mapper)
        : base(loggerFactory, dataService, mapper)
    {
    }

}

