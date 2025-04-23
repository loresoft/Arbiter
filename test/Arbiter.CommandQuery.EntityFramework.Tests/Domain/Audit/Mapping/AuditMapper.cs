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
[RegisterSingleton<IMapper<Models.AuditReadModel, Models.AuditCreateModel>>]
internal sealed partial class AuditReadModelToAuditCreateModelMapper : IMapper<Models.AuditReadModel, Models.AuditCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.AuditCreateModel? Map(Models.AuditReadModel? source);

    public partial void Map(Models.AuditReadModel source, Models.AuditCreateModel destination);

    public partial IQueryable<Models.AuditCreateModel> ProjectTo(IQueryable<Models.AuditReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.AuditReadModel, Models.AuditUpdateModel>>]
internal sealed partial class AuditReadModelToAuditUpdateModelMapper : IMapper<Models.AuditReadModel, Models.AuditUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.AuditUpdateModel? Map(Models.AuditReadModel? source);

    public partial void Map(Models.AuditReadModel source, Models.AuditUpdateModel destination);

    public partial IQueryable<Models.AuditUpdateModel> ProjectTo(IQueryable<Models.AuditReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.AuditUpdateModel, Models.AuditCreateModel>>]
internal sealed partial class AuditUpdateModelToAuditCreateModelMapper : IMapper<Models.AuditUpdateModel, Models.AuditCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.AuditCreateModel? Map(Models.AuditUpdateModel? source);

    public partial void Map(Models.AuditUpdateModel source, Models.AuditCreateModel destination);

    public partial IQueryable<Models.AuditCreateModel> ProjectTo(IQueryable<Models.AuditUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.AuditUpdateModel, Models.AuditReadModel>>]
internal sealed partial class AuditUpdateModelToAuditReadModelMapper : IMapper<Models.AuditUpdateModel, Models.AuditReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.AuditReadModel? Map(Models.AuditUpdateModel? source);

    public partial void Map(Models.AuditUpdateModel source, Models.AuditReadModel destination);

    public partial IQueryable<Models.AuditReadModel> ProjectTo(IQueryable<Models.AuditUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Audit, Models.AuditReadModel>>]
internal sealed partial class AuditToAuditReadModelMapper : IMapper<Entities.Audit, Models.AuditReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.AuditReadModel? Map(Entities.Audit? source);

    public partial void Map(Entities.Audit source, Models.AuditReadModel destination);

    public partial IQueryable<Models.AuditReadModel> ProjectTo(IQueryable<Entities.Audit> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Audit, Models.AuditUpdateModel>>]
internal sealed partial class AuditToAuditUpdateModelMapper : IMapper<Entities.Audit, Models.AuditUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.AuditUpdateModel? Map(Entities.Audit? source);

    public partial void Map(Entities.Audit source, Models.AuditUpdateModel destination);

    public partial IQueryable<Models.AuditUpdateModel> ProjectTo(IQueryable<Entities.Audit> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.AuditCreateModel, Entities.Audit>>]
internal sealed partial class AuditCreateModelToAuditMapper : IMapper<Models.AuditCreateModel, Entities.Audit>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Audit? Map(Models.AuditCreateModel? source);

    public partial void Map(Models.AuditCreateModel source, Entities.Audit destination);

    public partial IQueryable<Entities.Audit> ProjectTo(IQueryable<Models.AuditCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.AuditUpdateModel, Entities.Audit>>]
internal sealed partial class AuditUpdateModelToAuditMapper : IMapper<Models.AuditUpdateModel, Entities.Audit>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Audit? Map(Models.AuditUpdateModel? source);

    public partial void Map(Models.AuditUpdateModel source, Entities.Audit destination);

    public partial IQueryable<Entities.Audit> ProjectTo(IQueryable<Models.AuditUpdateModel> source);
}


