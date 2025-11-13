using Microsoft.Extensions.DependencyInjection;

using Context = Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain;

public static class TaskServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        #region Generated Register
        services.AddEntityQueries<Context.TrackerContext, Entities.Task, int, Models.TaskReadModel>();
        services.AddEntityCommands<Context.TrackerContext, Entities.Task, int, Models.TaskReadModel, Models.TaskCreateModel, Models.TaskUpdateModel>();
        #endregion

        services.AddEntityKeyQuery<Context.TrackerContext, Entities.Task, Models.TaskReadModel>();
    }
}
