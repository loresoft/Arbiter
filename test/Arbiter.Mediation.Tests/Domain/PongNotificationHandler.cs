using Arbiter.Mediation;

namespace Arbiter.Tests.Domain;

public class PongNotificationHandler(Logger logger) : INotificationHandler<Pinged>
{
    public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        logger.Messages.Add(notification.Message + " Pong");
        return ValueTask.CompletedTask;
    }
}
