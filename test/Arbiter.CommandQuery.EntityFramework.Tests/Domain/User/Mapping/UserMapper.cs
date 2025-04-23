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
[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserCreateModel>>]
internal sealed partial class UserReadModelToUserCreateModelMapper : IMapper<Models.UserReadModel, Models.UserCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserCreateModel? Map(Models.UserReadModel? source);

    public partial void Map(Models.UserReadModel source, Models.UserCreateModel destination);

    public partial IQueryable<Models.UserCreateModel> ProjectTo(IQueryable<Models.UserReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserUpdateModel>>]
internal sealed partial class UserReadModelToUserUpdateModelMapper : IMapper<Models.UserReadModel, Models.UserUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserUpdateModel? Map(Models.UserReadModel? source);

    public partial void Map(Models.UserReadModel source, Models.UserUpdateModel destination);

    public partial IQueryable<Models.UserUpdateModel> ProjectTo(IQueryable<Models.UserReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserCreateModel>>]
internal sealed partial class UserUpdateModelToUserCreateModelMapper : IMapper<Models.UserUpdateModel, Models.UserCreateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserCreateModel? Map(Models.UserUpdateModel? source);

    public partial void Map(Models.UserUpdateModel source, Models.UserCreateModel destination);

    public partial IQueryable<Models.UserCreateModel> ProjectTo(IQueryable<Models.UserUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserReadModel>>]
internal sealed partial class UserUpdateModelToUserReadModelMapper : IMapper<Models.UserUpdateModel, Models.UserReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserReadModel? Map(Models.UserUpdateModel? source);

    public partial void Map(Models.UserUpdateModel source, Models.UserReadModel destination);

    public partial IQueryable<Models.UserReadModel> ProjectTo(IQueryable<Models.UserUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed partial class UserToUserReadModelMapper : IMapper<Entities.User, Models.UserReadModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserReadModel? Map(Entities.User? source);

    public partial void Map(Entities.User source, Models.UserReadModel destination);

    public partial IQueryable<Models.UserReadModel> ProjectTo(IQueryable<Entities.User> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.User, Models.UserUpdateModel>>]
internal sealed partial class UserToUserUpdateModelMapper : IMapper<Entities.User, Models.UserUpdateModel>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Models.UserUpdateModel? Map(Entities.User? source);

    public partial void Map(Entities.User source, Models.UserUpdateModel destination);

    public partial IQueryable<Models.UserUpdateModel> ProjectTo(IQueryable<Entities.User> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed partial class UserCreateModelToUserMapper : IMapper<Models.UserCreateModel, Entities.User>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.User? Map(Models.UserCreateModel? source);

    public partial void Map(Models.UserCreateModel source, Entities.User destination);

    public partial IQueryable<Entities.User> ProjectTo(IQueryable<Models.UserCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserUpdateModel, Entities.User>>]
internal sealed partial class UserUpdateModelToUserMapper : IMapper<Models.UserUpdateModel, Entities.User>
{
    [return: NotNullIfNotNull(nameof(source))]
    public partial Entities.User? Map(Models.UserUpdateModel? source);

    public partial void Map(Models.UserUpdateModel source, Entities.User destination);

    public partial IQueryable<Entities.User> ProjectTo(IQueryable<Models.UserUpdateModel> source);
}


