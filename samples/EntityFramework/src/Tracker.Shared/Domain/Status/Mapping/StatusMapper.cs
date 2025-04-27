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
[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusCreateModel>>]
internal sealed partial class StatusReadModelToStatusCreateModelMapper : IMapper<Models.StatusReadModel, Models.StatusCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.StatusCreateModel? Map(Models.StatusReadModel? source);

    public partial void Map(Models.StatusReadModel source, Models.StatusCreateModel destination);

    public partial IQueryable<Models.StatusCreateModel> ProjectTo(IQueryable<Models.StatusReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusReadModel, Models.StatusUpdateModel>>]
internal sealed partial class StatusReadModelToStatusUpdateModelMapper : IMapper<Models.StatusReadModel, Models.StatusUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.StatusUpdateModel? Map(Models.StatusReadModel? source);

    public partial void Map(Models.StatusReadModel source, Models.StatusUpdateModel destination);

    public partial IQueryable<Models.StatusUpdateModel> ProjectTo(IQueryable<Models.StatusReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusCreateModel>>]
internal sealed partial class StatusUpdateModelToStatusCreateModelMapper : IMapper<Models.StatusUpdateModel, Models.StatusCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.StatusCreateModel? Map(Models.StatusUpdateModel? source);

    public partial void Map(Models.StatusUpdateModel source, Models.StatusCreateModel destination);

    public partial IQueryable<Models.StatusCreateModel> ProjectTo(IQueryable<Models.StatusUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.StatusUpdateModel, Models.StatusReadModel>>]
internal sealed partial class StatusUpdateModelToStatusReadModelMapper : IMapper<Models.StatusUpdateModel, Models.StatusReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.StatusReadModel? Map(Models.StatusUpdateModel? source);

    public partial void Map(Models.StatusUpdateModel source, Models.StatusReadModel destination);

    public partial IQueryable<Models.StatusReadModel> ProjectTo(IQueryable<Models.StatusUpdateModel> source);
}

