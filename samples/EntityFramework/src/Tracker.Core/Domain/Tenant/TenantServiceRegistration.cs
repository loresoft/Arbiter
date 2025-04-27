#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.EntityFramework;

using Microsoft.Extensions.DependencyInjection;

using Context = Tracker.Data;
using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain;

public static class TenantServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Tenant, int, Models.TenantReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Tenant, int, Models.TenantReadModel, Models.TenantCreateModel, Models.TenantUpdateModel>();
        #endregion
    }
}
