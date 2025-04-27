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
[RegisterSingleton<IMapper<Entities.Status, Models.StatusReadModel>>]
internal sealed partial class StatusToStatusReadModelMapper : IMapper<Entities.Status, Models.StatusReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.StatusReadModel? Map(Entities.Status? source);

    public partial void Map(Entities.Status source, Models.StatusReadModel destination);

    public partial IQueryable<Models.StatusReadModel> ProjectTo(IQueryable<Entities.Status> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Status, Models.StatusUpdateModel>>]
internal sealed partial class StatusToStatusUpdateModelMapper : IMapper<Entities.Status, Models.StatusUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.StatusUpdateModel? Map(Entities.Status? source);

    public partial void Map(Entities.Status source, Models.StatusUpdateModel destination);

    public partial IQueryable<Models.StatusUpdateModel> ProjectTo(IQueryable<Entities.Status> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusCreateModel, Entities.Status>>]
internal sealed partial class StatusCreateModelToStatusMapper : IMapper<Models.StatusCreateModel, Entities.Status>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Status? Map(Models.StatusCreateModel? source);

    public partial void Map(Models.StatusCreateModel source, Entities.Status destination);

    public partial IQueryable<Entities.Status> ProjectTo(IQueryable<Models.StatusCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusUpdateModel, Entities.Status>>]
internal sealed partial class StatusUpdateModelToStatusMapper : IMapper<Models.StatusUpdateModel, Entities.Status>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Status? Map(Models.StatusUpdateModel? source);

    public partial void Map(Models.StatusUpdateModel source, Entities.Status destination);

    public partial IQueryable<Entities.Status> ProjectTo(IQueryable<Models.StatusUpdateModel> source);
}

