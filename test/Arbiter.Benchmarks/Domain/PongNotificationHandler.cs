using Arbiter.Mediation;

namespace Arbiter.Benchmarks.Domain;

public class PongNotificationHandler(TextWriter writer) : INotificationHandler<Pinged>
{
    public async ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync(notification.Message + " Pong");
    }
}
