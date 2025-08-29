#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using M = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<M.UserReadModel, M.UserCreateModel>>]
internal sealed class UserReadModelToUserCreateModelMapper
    : MapperBase<M.UserReadModel, M.UserCreateModel>
{
    protected override Expression<Func<M.UserReadModel, M.UserCreateModel>> CreateMapping()
    {
        return source => new M.UserCreateModel
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

[RegisterSingleton<IMapper<M.UserReadModel, M.UserUpdateModel>>]
internal sealed class UserReadModelToUserUpdateModelMapper
    : MapperBase<M.UserReadModel, M.UserUpdateModel>
{
    protected override Expression<Func<M.UserReadModel, M.UserUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<M.UserUpdateModel, M.UserCreateModel>>]
internal sealed class UserUpdateModelToUserCreateModelMapper
    : MapperBase<M.UserUpdateModel, M.UserCreateModel>
{
    protected override Expression<Func<M.UserUpdateModel, M.UserCreateModel>> CreateMapping()
    {
        return source => new M.UserCreateModel
        {
            #region Generated Mappings
            DisplayName = source.DisplayName,
            EmailAddress = source.EmailAddress,
            IsDeleted = source.IsDeleted,
            #endregion

            // Manual Mappings
            Updated = source.Updated,
            UpdatedBy = source.UpdatedBy,
        };
    }
}

