namespace Arbiter.Mediation;

/// <summary>
/// An <see langword="interface"/> for a notification handler
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled</typeparam>
/// <remarks>
/// There can be multiple handlers for a notification.
/// </remarks>
/// <seealso cref="INotification"/>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification of type <typeparamref name="TNotification"/>
    /// </summary>
    /// <param name="notification">The notification being sent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Awaitable task for notification operation</returns>
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken = default);
}
