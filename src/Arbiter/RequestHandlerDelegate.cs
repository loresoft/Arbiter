namespace Arbiter;

/// <summary>
/// A <see langword="delegate"/> representing an async continuation for the next task to execute in the pipeline
/// </summary>
/// <typeparam name="TResponse"> The type of response from the piple</typeparam>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>Awaitable task returning a <typeparamref name="TResponse"/></returns>
public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);
