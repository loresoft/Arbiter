using Arbiter.Mediation;

namespace Arbiter.Tests.Domain;

public class InnerBehavior(Logger logger) : IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong?> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        logger.Messages.Add("Inner before");
        var response = await next(cancellationToken);
        logger.Messages.Add("Inner after");

        return response;
    }
}
