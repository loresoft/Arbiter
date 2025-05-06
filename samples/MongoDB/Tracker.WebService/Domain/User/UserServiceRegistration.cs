#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.MongoDB;

using MongoDB.Abstracts;

using Tracker.WebService.Data.Entities;
using Tracker.WebService.Domain.Models;

// ReSharper disable once CheckNamespace
namespace Tracker.WebService.Domain;

public static class UserServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<User>, User, string, UserReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<User>, User, string, UserReadModel, UserCreateModel, UserUpdateModel>();
    }
}
