using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

public partial class PriorityProfile
    : AutoMapper.Profile
{
    public PriorityProfile()
    {
        CreateMap<Data.Entities.Priority, PriorityReadModel>();

        CreateMap<PriorityCreateModel, Data.Entities.Priority>();

        CreateMap<Data.Entities.Priority, PriorityUpdateModel>();

        CreateMap<PriorityUpdateModel, Data.Entities.Priority>();
    }

}
