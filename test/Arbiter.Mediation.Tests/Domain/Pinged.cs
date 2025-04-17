using Arbiter.Mediation;

namespace Arbiter.Tests.Domain;

public class Pinged : INotification
{
    public string? Message { get; set; }
}
