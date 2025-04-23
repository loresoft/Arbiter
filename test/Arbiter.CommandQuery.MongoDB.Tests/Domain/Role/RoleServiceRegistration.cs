using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Abstracts;

// ReSharper disable once CheckNamespace
namespace Arbiter.CommandQuery.MongoDB.Tests.Domain;

public static class RoleServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.Role>, Data.Entities.Role, string, RoleReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Data.Entities.Role>, Data.Entities.Role, string, RoleReadModel, RoleCreateModel, RoleUpdateModel>();
    }
}
