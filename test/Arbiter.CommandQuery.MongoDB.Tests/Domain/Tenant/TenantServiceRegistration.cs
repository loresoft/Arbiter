using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Abstracts;

// ReSharper disable once CheckNamespace
namespace Arbiter.CommandQuery.MongoDB.Tests.Domain;

public static class TenantServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.Tenant>, Data.Entities.Tenant, string, TenantReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Data.Entities.Tenant>, Data.Entities.Tenant, string, TenantReadModel, TenantCreateModel, TenantUpdateModel>();
    }
}
