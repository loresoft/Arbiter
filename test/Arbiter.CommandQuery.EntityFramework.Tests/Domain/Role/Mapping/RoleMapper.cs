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
[RegisterSingleton<IMapper<Models.RoleReadModel, Models.RoleCreateModel>>]
internal sealed partial class RoleReadModelToRoleCreateModelMapper : IMapper<Models.RoleReadModel, Models.RoleCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.RoleCreateModel? Map(Models.RoleReadModel? source);

    public partial void Map(Models.RoleReadModel source, Models.RoleCreateModel destination);

    public partial IQueryable<Models.RoleCreateModel> ProjectTo(IQueryable<Models.RoleReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.RoleReadModel, Models.RoleUpdateModel>>]
internal sealed partial class RoleReadModelToRoleUpdateModelMapper : IMapper<Models.RoleReadModel, Models.RoleUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.RoleUpdateModel? Map(Models.RoleReadModel? source);

    public partial void Map(Models.RoleReadModel source, Models.RoleUpdateModel destination);

    public partial IQueryable<Models.RoleUpdateModel> ProjectTo(IQueryable<Models.RoleReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.RoleUpdateModel, Models.RoleCreateModel>>]
internal sealed partial class RoleUpdateModelToRoleCreateModelMapper : IMapper<Models.RoleUpdateModel, Models.RoleCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.RoleCreateModel? Map(Models.RoleUpdateModel? source);

    public partial void Map(Models.RoleUpdateModel source, Models.RoleCreateModel destination);

    public partial IQueryable<Models.RoleCreateModel> ProjectTo(IQueryable<Models.RoleUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.RoleUpdateModel, Models.RoleReadModel>>]
internal sealed partial class RoleUpdateModelToRoleReadModelMapper : IMapper<Models.RoleUpdateModel, Models.RoleReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.RoleReadModel? Map(Models.RoleUpdateModel? source);

    public partial void Map(Models.RoleUpdateModel source, Models.RoleReadModel destination);

    public partial IQueryable<Models.RoleReadModel> ProjectTo(IQueryable<Models.RoleUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Role, Models.RoleReadModel>>]
internal sealed partial class RoleToRoleReadModelMapper : IMapper<Entities.Role, Models.RoleReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.RoleReadModel? Map(Entities.Role? source);

    public partial void Map(Entities.Role source, Models.RoleReadModel destination);

    public partial IQueryable<Models.RoleReadModel> ProjectTo(IQueryable<Entities.Role> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.Role, Models.RoleUpdateModel>>]
internal sealed partial class RoleToRoleUpdateModelMapper : IMapper<Entities.Role, Models.RoleUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.RoleUpdateModel? Map(Entities.Role? source);

    public partial void Map(Entities.Role source, Models.RoleUpdateModel destination);

    public partial IQueryable<Models.RoleUpdateModel> ProjectTo(IQueryable<Entities.Role> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.RoleCreateModel, Entities.Role>>]
internal sealed partial class RoleCreateModelToRoleMapper : IMapper<Models.RoleCreateModel, Entities.Role>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Role? Map(Models.RoleCreateModel? source);

    public partial void Map(Models.RoleCreateModel source, Entities.Role destination);

    public partial IQueryable<Entities.Role> ProjectTo(IQueryable<Models.RoleCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.RoleUpdateModel, Entities.Role>>]
internal sealed partial class RoleUpdateModelToRoleMapper : IMapper<Models.RoleUpdateModel, Entities.Role>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.Role? Map(Models.RoleUpdateModel? source);

    public partial void Map(Models.RoleUpdateModel source, Entities.Role destination);

    public partial IQueryable<Entities.Role> ProjectTo(IQueryable<Models.RoleUpdateModel> source);
}


