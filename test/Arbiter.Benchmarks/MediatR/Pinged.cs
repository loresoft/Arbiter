namespace Arbiter.Benchmarks.MediatR;

public class Pinged : global::MediatR.INotification
{
    public string? Message { get; set; }
}
