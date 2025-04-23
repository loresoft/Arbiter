using Arbiter.CommandQuery.MongoDB.Tests.Domain.Handlers;
using Arbiter.CommandQuery.MongoDB.Tests.Domain.Models;
using Arbiter.CommandQuery.Notifications;
using Arbiter.Mediation;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Abstracts;

// ReSharper disable once CheckNamespace
namespace Arbiter.CommandQuery.MongoDB.Tests.Domain;

public static class TaskServiceRegistration
{
    [RegisterServices]
    public static void Register(IServiceCollection services)
    {
        services.AddEntityQueries<IMongoEntityRepository<Data.Entities.Task>, Data.Entities.Task, string, TaskReadModel>();
        services.AddEntityCommands<IMongoEntityRepository<Data.Entities.Task>, Data.Entities.Task, string, TaskReadModel, TaskCreateModel, TaskUpdateModel>();

        services.AddTransient<INotificationHandler<EntityChangeNotification<TaskReadModel>>, TaskChangedNotificationHandler>();
    }
}
