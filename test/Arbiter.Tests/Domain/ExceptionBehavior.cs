namespace Arbiter.Tests.Domain;

public class ExceptionBehavior(Logger logger) : IPipelineBehavior<Ping, Pong>
{
    public async ValueTask<Pong> Handle(Ping request, RequestHandlerDelegate<Pong> next, CancellationToken cancellationToken)
    {
        logger.Messages.Add("Exception before");
        var response = await next(cancellationToken);

        if (request.Message == "Throw")
            throw new InvalidOperationException("Exception in behavior");

        logger.Messages.Add("Exception after");
        return response;
    }
}
