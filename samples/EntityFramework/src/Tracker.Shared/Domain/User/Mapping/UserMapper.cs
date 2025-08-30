#pragma warning disable IDE0130 // Namespace does not match folder structure

using System.Linq.Expressions;

using Arbiter.CommandQuery.Definitions;
using Arbiter.CommandQuery.Mapping;

using Models = Tracker.Domain.Models;

namespace Tracker.Domain.Mapping;

[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserCreateModel>>]
internal sealed class UserReadModelToUserCreateModelMapper
    : MapperBase<Models.UserReadModel, Models.UserCreateModel>
{
    protected override Expression<Func<Models.UserReadModel, Models.UserCreateModel>> CreateMapping()
    {
        return source => new Models.UserCreateModel
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

[RegisterSingleton<IMapper<Models.UserReadModel, Models.UserUpdateModel>>]
internal sealed class UserReadModelToUserUpdateModelMapper
    : MapperBase<Models.UserReadModel, Models.UserUpdateModel>
{
    protected override Expression<Func<Models.UserReadModel, Models.UserUpdateModel>> CreateMapping()
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

[RegisterSingleton<IMapper<Models.UserUpdateModel, Models.UserCreateModel>>]
internal sealed class UserUpdateModelToUserCreateModelMapper
    : MapperBase<Models.UserUpdateModel, Models.UserCreateModel>
{
    protected override Expression<Func<Models.UserUpdateModel, Models.UserCreateModel>> CreateMapping()
    {
        return source => new Models.UserCreateModel
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

