#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.MongoDB;

using MongoDB.Abstracts;

using Tracker.WebService.Domain.Models;

// ReSharper disable once CheckNamespace
namespace Tracker.WebService.Domain;

public static class TenantServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.Tenant>, Data.Entities.Tenant, string, TenantReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Data.Entities.Tenant>, Data.Entities.Tenant, string, TenantReadModel, TenantCreateModel, TenantUpdateModel>();
    }
}
