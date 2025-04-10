namespace Arbiter.Tests.Domain;

public class PingHandler : IRequestHandler<Ping, Pong>
{
    public async ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken = default)
    {
        // Simulate some work
        await Task.Delay(100, cancellationToken);

        return new Pong { Message = $"{request.Message} Pong" };
    }
}
