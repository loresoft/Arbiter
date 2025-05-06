#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.MongoDB;

using MongoDB.Abstracts;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Domain.Models;

// ReSharper disable once CheckNamespace
namespace Tracker.WebService.Domain;

public static class PriorityServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Priority>, Priority, string, PriorityReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Priority>, Priority, string, PriorityReadModel, PriorityCreateModel, PriorityUpdateModel>();
    }
}
