namespace Arbiter.Tests.Domain;

public class OuterBehavior(Logger logger) : IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        logger.Messages.Add("Outer before");
        var response = await next(cancellationToken);
        logger.Messages.Add("Outer after");

        return response;
    }
}
