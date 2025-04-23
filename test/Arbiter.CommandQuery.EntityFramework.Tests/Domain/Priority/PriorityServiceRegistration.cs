using Microsoft.Extensions.DependencyInjection;

using Context = Arbiter.CommandQuery.EntityFramework.Tests.Data;
using Entities = Arbiter.CommandQuery.EntityFramework.Tests.Data.Entities;
using Models = Arbiter.CommandQuery.EntityFramework.Tests.Domain.Models;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Arbiter.CommandQuery.EntityFramework.Tests.Domain;

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
