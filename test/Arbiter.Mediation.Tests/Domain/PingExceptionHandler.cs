namespace Arbiter.Tests.Domain;

public class PingExceptionHandler : IRequestHandler<Ping, Pong>
{
    public ValueTask<Pong?> Handle(Ping request, CancellationToken cancellationToken)
    {
        if (request.Message == "Throw")
            throw new InvalidOperationException("Ping Exception");

        return ValueTask.FromResult<Pong?>(new Pong { Message = request.Message + " Pong" });
    }
}
