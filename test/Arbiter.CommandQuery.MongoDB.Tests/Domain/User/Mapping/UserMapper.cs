#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.Mapping;

using Entities = Arbiter.CommandQuery.MongoDB.Tests.Data.Entities;

namespace Arbiter.CommandQuery.MongoDB.Tests.Domain.Mapping;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserReadModelToUserCreateModelMapper
    : MapperProfile<Models.UserReadModel, Models.UserCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserReadModelToUserUpdateModelMapper
    : MapperProfile<Models.UserReadModel, Models.UserUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserUpdateModelToUserCreateModelMapper
    : MapperProfile<Models.UserUpdateModel, Models.UserCreateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserToUserReadModelMapper
    : MapperProfile<Entities.User, Models.UserReadModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserToUserUpdateModelMapper
    : MapperProfile<Entities.User, Models.UserUpdateModel>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserCreateModelToUserMapper
    : MapperProfile<Models.UserCreateModel, Entities.User>;

[GenerateMapper]
[RegisterSingleton]
internal sealed partial class UserUpdateModelToUserMapper
    : MapperProfile<Models.UserUpdateModel, Entities.User>;

