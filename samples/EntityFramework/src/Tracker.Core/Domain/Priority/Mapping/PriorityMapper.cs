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
[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityReadModel>>]
internal sealed partial class PriorityToPriorityReadModelMapper : IMapper<Entities.Priority, Models.PriorityReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.PriorityReadModel? Map(Entities.Priority? source);

    public partial void Map(Entities.Priority source, Models.PriorityReadModel destination);

    public partial IQueryable<Models.PriorityReadModel> ProjectTo(IQueryable<Entities.Priority> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Priority, Models.PriorityUpdateModel>>]
internal sealed partial class PriorityToPriorityUpdateModelMapper : IMapper<Entities.Priority, Models.PriorityUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.PriorityUpdateModel? Map(Entities.Priority? source);

    public partial void Map(Entities.Priority source, Models.PriorityUpdateModel destination);

    public partial IQueryable<Models.PriorityUpdateModel> ProjectTo(IQueryable<Entities.Priority> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityCreateModel, Entities.Priority>>]
internal sealed partial class PriorityCreateModelToPriorityMapper : IMapper<Models.PriorityCreateModel, Entities.Priority>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Priority? Map(Models.PriorityCreateModel? source);

    public partial void Map(Models.PriorityCreateModel source, Entities.Priority destination);

    public partial IQueryable<Entities.Priority> ProjectTo(IQueryable<Models.PriorityCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Entities.Priority>>]
internal sealed partial class PriorityUpdateModelToPriorityMapper : IMapper<Models.PriorityUpdateModel, Entities.Priority>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Priority? Map(Models.PriorityUpdateModel? source);

    public partial void Map(Models.PriorityUpdateModel source, Entities.Priority destination);

    public partial IQueryable<Entities.Priority> ProjectTo(IQueryable<Models.PriorityUpdateModel> source);
}

