namespace Arbiter.Benchmarks.Domain;

public class OuterBehavior(TextWriter writer) : Arbiter.IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("Outer before");
        var response = await next(cancellationToken);
        await writer.WriteLineAsync("Outer after");

        return response;
    }
}
