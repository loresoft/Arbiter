using Arbiter.CommandQuery.Definitions;

using Tracker.Domain.Models;

namespace Tracker.Client.Stores;

[RegisterScoped]
public class UserStore : Abstracts.StoreEditBase<UserReadModel, UserUpdateModel>
{
    public UserStore(ILoggerFactory loggerFactory, Services.DataService dataService, IMapper mapper)
        : base(loggerFactory, dataService, mapper)
    {
    }

}

