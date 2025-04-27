#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using System;
using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Injectio.Attributes;
using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed partial class TaskToTaskReadModelMapper : IMapper<Entities.Task, Models.TaskReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TaskReadModel? Map(Entities.Task? source);

    public partial void Map(Entities.Task source, Models.TaskReadModel destination);

    public partial IQueryable<Models.TaskReadModel> ProjectTo(IQueryable<Entities.Task> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Task, Models.TaskUpdateModel>>]
internal sealed partial class TaskToTaskUpdateModelMapper : IMapper<Entities.Task, Models.TaskUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TaskUpdateModel? Map(Entities.Task? source);

    public partial void Map(Entities.Task source, Models.TaskUpdateModel destination);

    public partial IQueryable<Models.TaskUpdateModel> ProjectTo(IQueryable<Entities.Task> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskCreateModel, Entities.Task>>]
internal sealed partial class TaskCreateModelToTaskMapper : IMapper<Models.TaskCreateModel, Entities.Task>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Task? Map(Models.TaskCreateModel? source);

    public partial void Map(Models.TaskCreateModel source, Entities.Task destination);

    public partial IQueryable<Entities.Task> ProjectTo(IQueryable<Models.TaskCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskUpdateModel, Entities.Task>>]
internal sealed partial class TaskUpdateModelToTaskMapper : IMapper<Models.TaskUpdateModel, Entities.Task>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Task? Map(Models.TaskUpdateModel? source);

    public partial void Map(Models.TaskUpdateModel source, Entities.Task destination);

    public partial IQueryable<Entities.Task> ProjectTo(IQueryable<Models.TaskUpdateModel> source);
}

