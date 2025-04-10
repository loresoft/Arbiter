namespace Arbiter.Benchmarks.MediatR;

public class OuterBehavior(TextWriter writer) : global::MediatR.IPipelineBehavior<Ping, Pong>
{
    public async Task<Pong> Handle(Ping request, global::MediatR.RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        await writer.WriteLineAsync("Outer before");
        var response = await next(cancellationToken);
        await writer.WriteLineAsync("Outer after");

        return response;
    }
}
