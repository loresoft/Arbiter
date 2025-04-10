namespace Arbiter.Benchmarks.MediatR;

public class Ping : global::MediatR.IRequest<Pong>
{
    public string? Message { get; set; }
}
