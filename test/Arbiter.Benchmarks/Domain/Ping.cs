using Arbiter.Mediation;

namespace Arbiter.Benchmarks.Domain;

public class Ping : IRequest<Pong>
{
    public string? Message { get; set; }
}
