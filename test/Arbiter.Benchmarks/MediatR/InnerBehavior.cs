namespace Arbiter.Benchmarks.MediatR;

public class InnerBehavior(TextWriter writer) : global::MediatR.IPipelineBehavior<Ping, Pong>
{
    public async Task<Pong> Handle(Ping request, global::MediatR.RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("Inner before");
        var response = await next(cancellationToken);
        await writer.WriteLineAsync("Inner after");

        return response;
    }
}
