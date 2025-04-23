using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

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
