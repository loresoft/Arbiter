namespace Arbiter.Benchmarks.MediatR;

public class PongNotificationHandler(TextWriter writer) : global::MediatR.INotificationHandler<Pinged>
{
    public async Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync(notification.Message + " Pong");
    }
}
