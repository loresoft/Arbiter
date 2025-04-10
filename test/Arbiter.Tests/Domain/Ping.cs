namespace Arbiter.Tests.Domain;

public class Ping : IRequest<Pong>
{
    public string? Message { get; set; }
}
