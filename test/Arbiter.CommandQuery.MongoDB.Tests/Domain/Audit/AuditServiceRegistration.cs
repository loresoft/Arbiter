using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Abstracts;

// ReSharper disable once CheckNamespace
namespace Arbiter.CommandQuery.MongoDB.Tests.Domain;

public static class AuditServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.Audit>, Data.Entities.Audit, string, AuditReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Data.Entities.Audit>, Data.Entities.Audit, string, AuditReadModel, AuditCreateModel, AuditUpdateModel>();
    }
}
