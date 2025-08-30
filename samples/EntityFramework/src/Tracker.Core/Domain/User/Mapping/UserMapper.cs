#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Entities.User, Models.UserReadModel>>]
internal sealed class UserToUserReadModelMapper
    : MapperBase<Entities.User, Models.UserReadModel>
{
    protected override Expression<Func<Entities.User, Models.UserReadModel>> CreateMapping()
    {
        return source => new Models.UserReadModel
        {
            #region Generated Mappings
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            #endregion

            // Manual Mappings
            Id = source.Id,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<Entities.User, Models.UserUpdateModel>>]
internal sealed class UserToUserUpdateModelMapper
    : MapperBase<Entities.User, Models.UserUpdateModel>
{
    protected override Expression<Func<Entities.User, Models.UserUpdateModel>> CreateMapping()
    {
        return source => new Models.UserUpdateModel
        {
            #region Generated Mappings
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

[RegisterSingleton<IMapper<Models.UserCreateModel, Entities.User>>]
internal sealed class UserCreateModelToUserMapper
    : MapperBase<Models.UserCreateModel, Entities.User>
{
    protected override Expression<Func<Models.UserCreateModel, Entities.User>> CreateMapping()
    {
        return source => new Entities.User
        {
            #region Generated Mappings
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            #endregion

            // Manual Mappings
            Id = source.Id,
            Created = source.Created,
            CreatedBy = source.CreatedBy,
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

[RegisterSingleton<IMapper<Models.UserUpdateModel, Entities.User>>]
internal sealed class UserUpdateModelToUserMapper
    : MapperBase<Models.UserUpdateModel, Entities.User>
{
    protected override Expression<Func<Models.UserUpdateModel, Entities.User>> CreateMapping()
    {
        return source => new Entities.User
        {
            #region Generated Mappings
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
            RowVersion = source.RowVersion,
        };
    }
}

