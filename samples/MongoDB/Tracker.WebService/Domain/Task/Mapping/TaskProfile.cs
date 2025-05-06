#pragma warning disable IDE0130 // Namespace does not match folder structure

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Mapping;

public partial class TaskProfile
    : AutoMapper.Profile
{
    public TaskProfile()
    {
        CreateMap<Data.Entities.Task, TaskReadModel>();

        CreateMap<TaskCreateModel, Data.Entities.Task>();

        CreateMap<TaskReadModel, TaskUpdateModel>();

        CreateMap<Data.Entities.Task, TaskUpdateModel>();

        CreateMap<TaskUpdateModel, Data.Entities.Task>();
    }

}
