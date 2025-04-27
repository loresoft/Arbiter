#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.EntityFramework;

using Microsoft.Extensions.DependencyInjection;

using Context = Tracker.Data;
using Entities = Tracker.Data.Entities;
using Models = Tracker.Domain.Models;

namespace Tracker.Domain;

public static class PriorityServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Priority, int, Models.PriorityReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Priority, int, Models.PriorityReadModel, Models.PriorityCreateModel, Models.PriorityUpdateModel>();
        #endregion
    }
}
