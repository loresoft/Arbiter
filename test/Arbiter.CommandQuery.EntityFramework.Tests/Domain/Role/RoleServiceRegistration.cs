using Microsoft.Extensions.DependencyInjection;

using Context = Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain;

public static class RoleServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Role, int, Models.RoleReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Role, int, Models.RoleReadModel, Models.RoleCreateModel, Models.RoleUpdateModel>();
        #endregion
    }
}
