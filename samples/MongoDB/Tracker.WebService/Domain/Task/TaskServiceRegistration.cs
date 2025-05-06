#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.MongoDB;
using Arbiter.CommandQuery.Notifications;
using Arbiter.Mediation;

using MongoDB.Abstracts;

using Tracker.WebService.Domain.Handlers;
using Tracker.WebService.Domain.Models;

// ReSharper disable once CheckNamespace
namespace Tracker.WebService.Domain;

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
