namespace Arbiter.Benchmarks.MediatR;

public class PingHandler(TextWriter writer) : global::MediatR.IRequestHandler<Ping, Pong>
{
    public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync($"Handler: {request.Message} Pong");

        return new Pong { Message = $"{request.Message} Pong" };
    }
}
