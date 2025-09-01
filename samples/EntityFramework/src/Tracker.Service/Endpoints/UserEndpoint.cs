using Arbiter.CommandQuery.Endpoints;

using Tracker.Domain.Models;

namespace Tracker.Service.Endpoints;

[RegisterTransient<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class UserEndpoint : EntityCommandEndpointBase<int, UserReadModel, UserReadModel, UserCreateModel, UserUpdateModel>
{
    public UserEndpoint(ILoggerFactory loggerFactory) : base(loggerFactory, "User")
    {

    }
}

