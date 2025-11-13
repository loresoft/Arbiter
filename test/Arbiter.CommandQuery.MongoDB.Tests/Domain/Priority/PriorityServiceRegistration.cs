using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Abstracts;

// ReSharper disable once CheckNamespace
namespace Arbiter.CommandQuery.MongoDB.Tests.Domain;

public static class PriorityServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.Priority>, Data.Entities.Priority, string, PriorityReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Data.Entities.Priority>, Data.Entities.Priority, string, PriorityReadModel, PriorityCreateModel, PriorityUpdateModel>();
        services.AddEntityKeyQuery<IMongoEntityRepository<Data.Entities.Priority>, Data.Entities.Priority, string, PriorityReadModel>();
    }
}
