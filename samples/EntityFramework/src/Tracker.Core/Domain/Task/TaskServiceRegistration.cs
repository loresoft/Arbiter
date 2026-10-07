#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.EntityFramework;

using Microsoft.Extensions.DependencyInjection;

using Context = Tracker.Data;
using Entities = Tracker.Data.Entities;

namespace Tracker.Domain;

public static class TaskServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Task, int, Models.TaskReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Task, int, Models.TaskReadModel, Models.TaskCreateModel, Models.TaskUpdateModel>();
        #endregion
    }
}
