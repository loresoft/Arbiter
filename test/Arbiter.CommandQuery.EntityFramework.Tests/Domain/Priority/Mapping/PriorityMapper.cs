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
[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityCreateModel>>]
internal sealed partial class PriorityReadModelToPriorityCreateModelMapper : IMapper<Models.PriorityReadModel, Models.PriorityCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.PriorityCreateModel? Map(Models.PriorityReadModel? source);

    public partial void Map(Models.PriorityReadModel source, Models.PriorityCreateModel destination);

    public partial IQueryable<Models.PriorityCreateModel> ProjectTo(IQueryable<Models.PriorityReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityReadModel, Models.PriorityUpdateModel>>]
internal sealed partial class PriorityReadModelToPriorityUpdateModelMapper : IMapper<Models.PriorityReadModel, Models.PriorityUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.PriorityUpdateModel? Map(Models.PriorityReadModel? source);

    public partial void Map(Models.PriorityReadModel source, Models.PriorityUpdateModel destination);

    public partial IQueryable<Models.PriorityUpdateModel> ProjectTo(IQueryable<Models.PriorityReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityCreateModel>>]
internal sealed partial class PriorityUpdateModelToPriorityCreateModelMapper : IMapper<Models.PriorityUpdateModel, Models.PriorityCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.PriorityCreateModel? Map(Models.PriorityUpdateModel? source);

    public partial void Map(Models.PriorityUpdateModel source, Models.PriorityCreateModel destination);

    public partial IQueryable<Models.PriorityCreateModel> ProjectTo(IQueryable<Models.PriorityUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.PriorityUpdateModel, Models.PriorityReadModel>>]
internal sealed partial class PriorityUpdateModelToPriorityReadModelMapper : IMapper<Models.PriorityUpdateModel, Models.PriorityReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.PriorityReadModel? Map(Models.PriorityUpdateModel? source);

    public partial void Map(Models.PriorityUpdateModel source, Models.PriorityReadModel destination);

    public partial IQueryable<Models.PriorityReadModel> ProjectTo(IQueryable<Models.PriorityUpdateModel> source);
}

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


