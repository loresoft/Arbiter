using Microsoft.Extensions.DependencyInjection;


namespace Arbiter.Mediation.Infrastructure;

/// <summary>
/// Handler wrapper that resolves and executes request handlers with pipeline behaviors.
/// Made public to support source generator pre-registration in handler cache.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public readonly struct MediatorHandler<TRequest, TResponse> : IMediatorHandler<TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public async ValueTask<object?> Handle(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return await Handle((TRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask<TResponse?> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToArray();

        var pipeline = handler;

        // reverse the behaviors to maintain the order of execution
        for (var i = behaviors.Length - 1; i >= 0; i--)
            pipeline = new MediatorPipeline<TRequest, TResponse>(behaviors[i], pipeline);

        return pipeline.Handle((TRequest)request, cancellationToken);
    }
}
