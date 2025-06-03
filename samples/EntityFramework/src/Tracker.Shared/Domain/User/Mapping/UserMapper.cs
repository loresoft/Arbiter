#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable RMG012 // Source member was not found for target member
#pragma warning disable RMG020 // Source member is not mapped to any target member

using Arbiter.CommandQuery.Definitions;

using Riok.Mapperly.Abstractions;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[Mapper]
[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserCreateModel>>]
internal sealed partial class UserReadModelToUserCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.UserReadModel, Models.UserCreateModel>
{
    [MapperIgnoreSource(nameof(Models.UserReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.UserReadModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.UserCreateModel.Id))]
    public override partial void Map(Models.UserReadModel source, Models.UserCreateModel destination);

    public override partial IQueryable<Models.UserCreateModel> ProjectTo(IQueryable<Models.UserReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserUpdateModel>>]
internal sealed partial class UserReadModelToUserUpdateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.UserReadModel, Models.UserUpdateModel>
{
    [MapperIgnoreSource(nameof(Models.UserReadModel.Id))]
    [MapperIgnoreSource(nameof(Models.UserReadModel.Created))]
    [MapperIgnoreSource(nameof(Models.UserReadModel.CreatedBy))]
    public override partial void Map(Models.UserReadModel source, Models.UserUpdateModel destination);

    public override partial IQueryable<Models.UserUpdateModel> ProjectTo(IQueryable<Models.UserReadModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserCreateModel>>]
internal sealed partial class UserUpdateModelToUserCreateModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Models.UserCreateModel>
{
    [MapperIgnoreSource(nameof(Models.UserUpdateModel.RowVersion))]
    [MapperIgnoreTarget(nameof(Models.UserCreateModel.Id))]
    [MapperIgnoreTarget(nameof(Models.UserCreateModel.Created))]
    [MapperIgnoreTarget(nameof(Models.UserCreateModel.CreatedBy))]
    public override partial void Map(Models.UserUpdateModel source, Models.UserCreateModel destination);

    public override partial IQueryable<Models.UserCreateModel> ProjectTo(IQueryable<Models.UserUpdateModel> source);
}

[Mapper]
[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserReadModel>>]
internal sealed partial class UserUpdateModelToUserReadModelMapper : Arbiter.CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Models.UserReadModel>
{
    [MapperIgnoreTarget(nameof(Models.UserReadModel.Id))]
    [MapperIgnoreTarget(nameof(Models.UserReadModel.Created))]
    [MapperIgnoreTarget(nameof(Models.UserReadModel.CreatedBy))]
    public override partial void Map(Models.UserUpdateModel source, Models.UserReadModel destination);

    public override partial IQueryable<Models.UserReadModel> ProjectTo(IQueryable<Models.UserUpdateModel> source);
}

