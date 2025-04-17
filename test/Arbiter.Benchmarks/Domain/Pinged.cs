using Arbiter.Mediation;

namespace Arbiter.Benchmarks.Domain;

public class Pinged : INotification
{
    public string? Message { get; set; }
}
