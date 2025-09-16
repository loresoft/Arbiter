#pragma warning disable IDE0130 // Namespace does not match folder structure

using Arbiter.CommandQuery.Notifications;

using Tracker.WebService.Domain.Models;

namespace Tracker.WebService.Domain.Handlers;

public class TaskChangedNotificationHandler
{
    private readonly ILogger<TaskChangedNotificationHandler> _logger;

    public TaskChangedNotificationHandler(ILogger<TaskChangedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask Handle(EntityChangeNotification<TaskReadModel> notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Task Changed: {id} {operation}", notification.Model?.Id, notification.Operation);

        return ValueTask.CompletedTask;
    }
}
