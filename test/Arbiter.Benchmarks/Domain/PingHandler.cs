using Arbiter.Mediation;

namespace Arbiter.Benchmarks.Domain;

public class PingHandler(TextWriter writer) : IRequestHandler<Ping, Pong>
{
    public async ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync($"Handler: {request.Message} Pong");

        return new Pong { Message = $"{request.Message} Pong" };
    }
}
