#pragma warning disable IDE0130 // Namespace does not match folder structure

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Mapping;

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
