#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserCreateModel>>]
internal sealed class UserReadModelToUserCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.UserReadModel, Models.UserCreateModel>
{
    protected override Expression<Func<Models.UserReadModel, Models.UserCreateModel>> CreateMapping()
    {
        return source => new Models.UserCreateModel
        {
            Id = source.Id,
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserUpdateModel>>]
internal sealed class UserReadModelToUserUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.UserReadModel, Models.UserUpdateModel>
{
    protected override Expression<Func<Models.UserReadModel, Models.UserUpdateModel>> CreateMapping()
    {
        return source => new Models.UserUpdateModel
        {
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserCreateModel>>]
internal sealed class UserUpdateModelToUserCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Models.UserCreateModel>
{
    protected override Expression<Func<Models.UserUpdateModel, Models.UserCreateModel>> CreateMapping()
    {
        return source => new Models.UserCreateModel
        {
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed class UserToUserReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.User, Models.UserReadModel>
{
    protected override Expression<Func<Entities.User, Models.UserReadModel>> CreateMapping()
    {
        return source => new Models.UserReadModel
        {
            Id = source.Id,
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Entities.User, Models.UserUpdateModel>>]
internal sealed class UserToUserUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.User, Models.UserUpdateModel>
{
    protected override Expression<Func<Entities.User, Models.UserUpdateModel>> CreateMapping()
    {
        return source => new Models.UserUpdateModel
        {
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed class UserCreateModelToUserMapper : CommandQuery.Mapping.MapperBase<Models.UserCreateModel, Entities.User>
{
    protected override Expression<Func<Models.UserCreateModel, Entities.User>> CreateMapping()
    {
        return source => new Entities.User
        {
            Id = source.Id,
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy
        };
    }
}

[RegisterSingleton<IMapper<Models.UserUpdateModel, Entities.User>>]
internal sealed class UserUpdateModelToUserMapper : CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Entities.User>
{
    protected override Expression<Func<Models.UserUpdateModel, Entities.User>> CreateMapping()
    {
        return source => new Entities.User
        {
            EmailAddress = source.EmailAddress,
            IsEmailAddressConfirmed = source.IsEmailAddressConfirmed,
            DisplayName = source.DisplayName,
            PasswordHash = source.PasswordHash,
            ResetHash = source.ResetHash,
            InviteHash = source.InviteHash,
            AccessFailedCount = source.AccessFailedCount,
            LockoutEnabled = source.LockoutEnabled,
            LockoutEnd = source.LockoutEnd,
            LastLogin = source.LastLogin,
            IsDeleted = source.IsDeleted,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion
        };
    }
}

