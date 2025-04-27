#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.EntityFramework;

using Microsoft.Extensions.DependencyInjection;

using Context = Tracker.Data;
using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain;

public static class StatusServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Status, int, Models.StatusReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Status, int, Models.StatusReadModel, Models.StatusCreateModel, Models.StatusUpdateModel>();
        #endregion
    }
}
