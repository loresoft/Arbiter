#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.MongoDB;

using MongoDB.Abstracts;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Domain.Models;

// ReSharper disable once CheckNamespace
namespace Tracker.WebService.Domain;

public static class StatusServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Status>, Status, string, StatusReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Status>, Status, string, StatusReadModel, StatusCreateModel, StatusUpdateModel>();
    }
}
