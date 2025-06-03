#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed partial class UserToUserReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.User, Models.UserReadModel>
{
    public override partial void Map(Entities.User source, Models.UserReadModel destination);

    public override partial IQueryable<Models.UserReadModel> ProjectTo(IQueryable<Entities.User> source);
}

[Mapper]
[RegisterSingleton<IMapper<Entities.User, Models.UserUpdateModel>>]
internal sealed partial class UserToUserUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Entities.User, Models.UserUpdateModel>
{
    [MapperIgnoreSource(nameof(Entities.User.Id))]
    [MapperIgnoreSource(nameof(Entities.User.Created))]
    [MapperIgnoreSource(nameof(Entities.User.CreatedBy))]
    public override partial void Map(Entities.User source, Models.UserUpdateModel destination);

    public override partial IQueryable<Models.UserUpdateModel> ProjectTo(IQueryable<Entities.User> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed partial class UserCreateModelToUserMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.UserCreateModel, Entities.User>
{
    [MapperIgnoreTarget(nameof(Entities.User.RowVersion))]
    public override partial void Map(Models.UserCreateModel source, Entities.User destination);

    public override partial IQueryable<Entities.User> ProjectTo(IQueryable<Models.UserCreateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserUpdateModel, Entities.User>>]
internal sealed partial class UserUpdateModelToUserMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Entities.User>
{
    [MapperIgnoreTarget(nameof(Entities.User.Id))]
    [MapperIgnoreTarget(nameof(Entities.User.Created))]
    [MapperIgnoreTarget(nameof(Entities.User.CreatedBy))]
    public override partial void Map(Models.UserUpdateModel source, Entities.User destination);

    public override partial IQueryable<Entities.User> ProjectTo(IQueryable<Models.UserUpdateModel> source);
}

