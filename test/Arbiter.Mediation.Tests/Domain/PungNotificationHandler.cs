using Arbiter.Mediation;

namespace Arbiter.Tests.Domain;

public class PungNotificationHandler(Logger logger) : INotificationHandler<Pinged>
{
    public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        logger.Messages.Add(notification.Message + " Pung");
        return ValueTask.CompletedTask;
    }
}
