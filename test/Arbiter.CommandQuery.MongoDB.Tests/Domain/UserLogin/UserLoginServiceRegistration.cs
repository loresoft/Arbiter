using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Abstracts;

// ReSharper disable once CheckNamespace
namespace Arbiter.CommandQuery.MongoDB.Tests.Domain;

public static class UserLoginServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.UserLogin>, Data.Entities.UserLogin, string, UserLoginReadModel>();
    }
}
