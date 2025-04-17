using Arbiter.Mediation;

namespace Arbiter.Benchmarks.Domain;

public class InnerBehavior(TextWriter writer) : IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong?> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("Inner before");
        var response = await next(cancellationToken);
        await writer.WriteLineAsync("Inner after");

        return response;
    }
}
