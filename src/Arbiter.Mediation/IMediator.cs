using System.Diagnostics.CodeAnalysis;

namespace Arbiter.Mediation;

/// <summary>
/// An <see langword="interface"/> defining a mediator to encapsulate request/response and publishing patterns.
/// </summary>
/// <example>
/// Example usage of <see cref="IMediator"/>:
/// <code>
/// // Define a request and response
/// public record HelloRequest(string Name) : IRequest&lt;string&gt;;
///
/// public class HelloRequestHandler : IRequestHandler&lt;HelloRequest, string&gt;
/// {
///     public ValueTask&lt;string&gt; Handle(HelloRequest request, CancellationToken cancellationToken)
///     {
///         return ValueTask.FromResult($"Hello, {request.Name}!");
///     }
/// }
///
/// // Define a notification
/// public record HelloNotification(string Message) : INotification;
///
/// public class HelloNotificationHandler : INotificationHandler&lt;HelloNotification&gt;
/// {
///     public ValueTask Handle(HelloNotification notification, CancellationToken cancellationToken)
///     {
///         Console.WriteLine($"Notification received: {notification.Message}");
///         return ValueTask.CompletedTask;
///     }
/// }
///
/// // Usage
/// var services = new ServiceCollection();
/// // Register Mediator services
/// services.AddMediator();
///
/// // Register Handlers
/// services.AddScoped&lt;IRequestHandler&lt;HelloRequest, string&gt;, HelloRequestHandler&gt;();
/// services.AddScoped&lt;INotificationHandler&lt;HelloNotification&gt;, HelloNotificationHandler&gt;();
///
/// var provider = services.BuildServiceProvider();
///
/// var mediator = provider.GetRequiredService&lt;IMediator&gt;();
///
/// // Sending a request
/// var response = await mediator.Send(new HelloRequest("World"));
/// Console.WriteLine(response); // Output: Hello, World!
///
/// // Publishing a notification
/// await mediator.Publish(new HelloNotification("This is a test notification"));
/// </code>
/// </example>
public interface IMediator
{
    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of request being sent.</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    /// <param name="request">The request to send to the handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    ValueTask<TResponse?> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response from the handler.</typeparam>
    /// <param name="request">The request to send to the handler.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task returning the <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    [RequiresUnreferencedCode("This overload relies on reflection over types that may be removed when trimming.")]
    ValueTask<TResponse?> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request to the appropriate handler and returns the response.
    /// </summary>
    /// <param name="request">The request to send to the handler. The request object must implement <see cref="IRequest{TResponse}"/> <see langword="interface"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task returning the handler response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="request"/> does not implement <see cref="IRequest{TResponse}"/> interface.</exception>
    [RequiresUnreferencedCode("This overload relies on reflection over types that may be removed when trimming.")]
    ValueTask<object?> Send(
        object request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification to multiple handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being sent.</typeparam>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Awaitable task for the notification operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
