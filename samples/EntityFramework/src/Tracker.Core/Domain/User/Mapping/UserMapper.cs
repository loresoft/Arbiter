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

