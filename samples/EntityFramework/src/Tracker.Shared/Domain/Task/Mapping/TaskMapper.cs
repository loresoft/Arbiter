#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using System;
using System.Diagnostics.CodeAnalysis;

using Arbiter.CommandQuery.Definitions;

using Injectio.Attributes;
using Riok.Mapperly.Abstractions;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

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

