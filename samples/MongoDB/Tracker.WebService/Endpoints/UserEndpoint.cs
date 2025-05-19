using Arbiter.CommandQuery.Endpoints;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Endpoints;

[RegisterSingleton<IEndpointRoute>(Duplicate = DuplicateStrategy.Append)]
public class UserEndpoint : EntityCommandEndpointBase<string, UserReadModel, UserReadModel, UserCreateModel, UserUpdateModel>
{
    public UserEndpoint(ILoggerFactory loggerFactory)
        : base(loggerFactory, "User")
    { }
}
