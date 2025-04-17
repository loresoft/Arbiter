using Arbiter.Mediation;

namespace Arbiter.Tests.Domain;

public class PingHandler(Logger logger) : IRequestHandler<Ping, Pong>
{
    public async ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken = default)
    {
        // Simulate some work
        await Task.Delay(100, cancellationToken);

        logger.Messages.Add("Ping Handler");

        return new Pong { Message = $"{request.Message} Pong" };
    }
}
