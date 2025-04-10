namespace Arbiter.Benchmarks.Domain;

public class Ping : Arbiter.IRequest<Pong>
{
    public string? Message { get; set; }
}
