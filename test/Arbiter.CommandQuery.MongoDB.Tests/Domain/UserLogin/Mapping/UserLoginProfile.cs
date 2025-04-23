using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

public partial class UserLoginProfile
    : AutoMapper.Profile
{
    public UserLoginProfile()
    {
        CreateMap<Data.Entities.UserLogin, UserLoginReadModel>();
    }

}
