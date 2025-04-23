#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using System;
using System.Diagnostics.CodeAnalysis;

using Injectio.Attributes;
using Riok.Mapperly.Abstractions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskCreateModel>>]
internal sealed partial class TaskReadModelToTaskCreateModelMapper : IMapper<Models.TaskReadModel, Models.TaskCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TaskCreateModel? Map(Models.TaskReadModel? source);

    public partial void Map(Models.TaskReadModel source, Models.TaskCreateModel destination);

    public partial IQueryable<Models.TaskCreateModel> ProjectTo(IQueryable<Models.TaskReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskReadModel, Models.TaskUpdateModel>>]
internal sealed partial class TaskReadModelToTaskUpdateModelMapper : IMapper<Models.TaskReadModel, Models.TaskUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TaskUpdateModel? Map(Models.TaskReadModel? source);

    public partial void Map(Models.TaskReadModel source, Models.TaskUpdateModel destination);

    public partial IQueryable<Models.TaskUpdateModel> ProjectTo(IQueryable<Models.TaskReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskCreateModel>>]
internal sealed partial class TaskUpdateModelToTaskCreateModelMapper : IMapper<Models.TaskUpdateModel, Models.TaskCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TaskCreateModel? Map(Models.TaskUpdateModel? source);

    public partial void Map(Models.TaskUpdateModel source, Models.TaskCreateModel destination);

    public partial IQueryable<Models.TaskCreateModel> ProjectTo(IQueryable<Models.TaskUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TaskUpdateModel, Models.TaskReadModel>>]
internal sealed partial class TaskUpdateModelToTaskReadModelMapper : IMapper<Models.TaskUpdateModel, Models.TaskReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TaskReadModel? Map(Models.TaskUpdateModel? source);

    public partial void Map(Models.TaskUpdateModel source, Models.TaskReadModel destination);

    public partial IQueryable<Models.TaskReadModel> ProjectTo(IQueryable<Models.TaskUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Task, Models.TaskReadModel>>]
internal sealed partial class TaskToTaskReadModelMapper : IMapper<Entities.Task, Models.TaskReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    [MapProperty("Status.Name", "StatusName")]
    [MapProperty("Priority.Name", "PriorityName")]
    [MapProperty("AssignedUser.DisplayName", "AssignedName")]
    [MapProperty("Tenant.Name", "TenantName")]
    public partial Models.TaskReadModel? Map(Entities.Task? source);

    [MapProperty("Status.Name", "StatusName")]
    [MapProperty("Priority.Name", "PriorityName")]
    [MapProperty("AssignedUser.DisplayName", "AssignedName")]
    [MapProperty("Tenant.Name", "TenantName")]
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


