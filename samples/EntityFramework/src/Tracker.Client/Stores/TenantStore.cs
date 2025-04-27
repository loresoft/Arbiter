using Arbiter.CommandQuery.Definitions;

using Tracker.Domain.Models;

namespace Tracker.Client.Stores;

[RegisterScoped]
public class TenantStore : Abstracts.StoreEditBase<TenantReadModel, TenantUpdateModel>
{
    public TenantStore(ILoggerFactory loggerFactory, Services.DataService dataService, IMapper mapper)
        : base(loggerFactory, dataService, mapper)
    {
    }

}

