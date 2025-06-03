#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Definitions;

using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain.Mapping;

[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserCreateModel>>]
internal sealed class UserReadModelToUserCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.UserReadModel, Models.UserCreateModel>
{
    public override void Map(Models.UserReadModel source, Models.UserCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.UserCreateModel> ProjectTo(IQueryable<Models.UserReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.UserCreateModel
            {
                #region Generated Query Properties
                Id = p.Id,
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserUpdateModel>>]
internal sealed class UserReadModelToUserUpdateModelMapper : CommandQuery.Mapping.MapperBase<Models.UserReadModel, Models.UserUpdateModel>
{
    public override void Map(Models.UserReadModel source, Models.UserUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.UserUpdateModel> ProjectTo(IQueryable<Models.UserReadModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.UserUpdateModel
            {
                #region Generated Query Properties
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserCreateModel>>]
internal sealed class UserUpdateModelToUserCreateModelMapper : CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Models.UserCreateModel>
{
    public override void Map(Models.UserUpdateModel source, Models.UserCreateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Models.UserCreateModel> ProjectTo(IQueryable<Models.UserUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.UserCreateModel
            {
                #region Generated Query Properties
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed class UserToUserReadModelMapper : CommandQuery.Mapping.MapperBase<Entities.User, Models.UserReadModel>
{
    public override void Map(Entities.User source, Models.UserReadModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.UserReadModel> ProjectTo(IQueryable<Entities.User> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.UserReadModel
            {
                #region Generated Query Properties
                Id = p.Id,
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Entities.User, Models.UserUpdateModel>>]
internal sealed class UserToUserUpdateModelMapper : CommandQuery.Mapping.MapperBase<Entities.User, Models.UserUpdateModel>
{
    public override void Map(Entities.User source, Models.UserUpdateModel destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Models.UserUpdateModel> ProjectTo(IQueryable<Entities.User> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Models.UserUpdateModel
            {
                #region Generated Query Properties
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed class UserCreateModelToUserMapper : CommandQuery.Mapping.MapperBase<Models.UserCreateModel, Entities.User>
{
    public override void Map(Models.UserCreateModel source, Entities.User destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.Id = source.Id;
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Created = source.Created;
        destination.CreatedBy = source.CreatedBy;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        #endregion
    }

    public override IQueryable<Entities.User> ProjectTo(IQueryable<Models.UserCreateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.User
            {
                #region Generated Query Properties
                Id = p.Id,
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Created = p.Created,
                CreatedBy = p.CreatedBy,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                #endregion
            }
        );
    }
}

[RegisterSingleton<IMapper<Models.UserUpdateModel, Entities.User>>]
internal sealed class UserUpdateModelToUserMapper : CommandQuery.Mapping.MapperBase<Models.UserUpdateModel, Entities.User>
{
    public override void Map(Models.UserUpdateModel source, Entities.User destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        #region Generated Copied Properties
        destination.EmailAddress = source.EmailAddress;
        destination.IsEmailAddressConfirmed = source.IsEmailAddressConfirmed;
        destination.DisplayName = source.DisplayName;
        destination.PasswordHash = source.PasswordHash;
        destination.ResetHash = source.ResetHash;
        destination.InviteHash = source.InviteHash;
        destination.AccessFailedCount = source.AccessFailedCount;
        destination.LockoutEnabled = source.LockoutEnabled;
        destination.LockoutEnd = source.LockoutEnd;
        destination.LastLogin = source.LastLogin;
        destination.IsDeleted = source.IsDeleted;
        destination.Updated = source.Updated;
        destination.UpdatedBy = source.UpdatedBy;
        destination.RowVersion = source.RowVersion;
        #endregion
    }

    public override IQueryable<Entities.User> ProjectTo(IQueryable<Models.UserUpdateModel> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select(p =>
            new Entities.User
            {
                #region Generated Query Properties
                EmailAddress = p.EmailAddress,
                IsEmailAddressConfirmed = p.IsEmailAddressConfirmed,
                DisplayName = p.DisplayName,
                PasswordHash = p.PasswordHash,
                ResetHash = p.ResetHash,
                InviteHash = p.InviteHash,
                AccessFailedCount = p.AccessFailedCount,
                LockoutEnabled = p.LockoutEnabled,
                LockoutEnd = p.LockoutEnd,
                LastLogin = p.LastLogin,
                IsDeleted = p.IsDeleted,
                Updated = p.Updated,
                UpdatedBy = p.UpdatedBy,
                RowVersion = p.RowVersion,
                #endregion
            }
        );
    }
}

