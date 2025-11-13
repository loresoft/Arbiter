using Microsoft.Extensions.DependencyInjection;

using Context = Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain;

public static class UserServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.User, int, Models.UserReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.User, int, Models.UserReadModel, Models.UserCreateModel, Models.UserUpdateModel>();
        #endregion

        services.AddEntityKeyQuery<Context.TrackerContext, Entities.User, Models.UserReadModel>();
    }
}
