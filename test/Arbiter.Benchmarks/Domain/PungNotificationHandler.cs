using Arbiter.Mediation;

namespace Arbiter.Benchmarks.Domain;

public class PungNotificationHandler(TextWriter writer) : INotificationHandler<Pinged>
{
    public async ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync(notification.Message + " Pung");
    }
}
