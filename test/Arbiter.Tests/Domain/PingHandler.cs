namespace Arbiter.Tests.Domain;

public class PingHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new Pong { Message = request.Message + " Pong" });
    }
}
