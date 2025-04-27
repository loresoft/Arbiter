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
[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantCreateModel>>]
internal sealed partial class TenantReadModelToTenantCreateModelMapper : IMapper<Models.TenantReadModel, Models.TenantCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TenantCreateModel? Map(Models.TenantReadModel? source);

    public partial void Map(Models.TenantReadModel source, Models.TenantCreateModel destination);

    public partial IQueryable<Models.TenantCreateModel> ProjectTo(IQueryable<Models.TenantReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantReadModel, Models.TenantUpdateModel>>]
internal sealed partial class TenantReadModelToTenantUpdateModelMapper : IMapper<Models.TenantReadModel, Models.TenantUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TenantUpdateModel? Map(Models.TenantReadModel? source);

    public partial void Map(Models.TenantReadModel source, Models.TenantUpdateModel destination);

    public partial IQueryable<Models.TenantUpdateModel> ProjectTo(IQueryable<Models.TenantReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantCreateModel>>]
internal sealed partial class TenantUpdateModelToTenantCreateModelMapper : IMapper<Models.TenantUpdateModel, Models.TenantCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TenantCreateModel? Map(Models.TenantUpdateModel? source);

    public partial void Map(Models.TenantUpdateModel source, Models.TenantCreateModel destination);

    public partial IQueryable<Models.TenantCreateModel> ProjectTo(IQueryable<Models.TenantUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.TenantUpdateModel, Models.TenantReadModel>>]
internal sealed partial class TenantUpdateModelToTenantReadModelMapper : IMapper<Models.TenantUpdateModel, Models.TenantReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.TenantReadModel? Map(Models.TenantUpdateModel? source);

    public partial void Map(Models.TenantUpdateModel source, Models.TenantReadModel destination);

    public partial IQueryable<Models.TenantReadModel> ProjectTo(IQueryable<Models.TenantUpdateModel> source);
}

