#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using E = Tracker.Data.Entities;
using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<E.User, M.UserReadModel>>]
internal sealed class UserToUserReadModelMapper
    : MapperBase<E.User, M.UserReadModel>
{
    protected override Expression<Func<E.User, M.UserReadModel>> CreateMapping()
    {
        return source => new M.UserReadModel
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

[RegisterSingleton<IMapper<E.User, M.UserUpdateModel>>]
internal sealed class UserToUserUpdateModelMapper
    : MapperBase<E.User, M.UserUpdateModel>
{
    protected override Expression<Func<E.User, M.UserUpdateModel>> CreateMapping()
    {
        return source => new M.UserUpdateModel
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

[RegisterSingleton<IMapper<M.UserCreateModel, E.User>>]
internal sealed class UserCreateModelToUserMapper
    : MapperBase<M.UserCreateModel, E.User>
{
    protected override Expression<Func<M.UserCreateModel, E.User>> CreateMapping()
    {
        return source => new E.User
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

[RegisterSingleton<IMapper<M.UserUpdateModel, E.User>>]
internal sealed class UserUpdateModelToUserMapper
    : MapperBase<M.UserUpdateModel, E.User>
{
    protected override Expression<Func<M.UserUpdateModel, E.User>> CreateMapping()
    {
        return source => new E.User
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

