using Arbiter.Dispatcher.Client;
using Arbiter.Mediation;

namespace Arbiter.Components.Tests.Services;

/// <summary>
/// An <see cref="IDispatcher"/> that fails when it is used, for tests that never send a request.
/// </summary>
internal sealed class FakeDispatcher : IDispatcher
{
    public ValueTask<TResponse?> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        => throw new NotSupportedException();

    public ValueTask<TResponse?> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}
