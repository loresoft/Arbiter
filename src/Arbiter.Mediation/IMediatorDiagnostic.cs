using System.Runtime.CompilerServices;

namespace Arbiter.Mediation;

/// <summary>
/// Represents an interface for logging diagnostic information and activities related to requests and notifications.
/// </summary>
public interface IMediatorDiagnostic
{
    /// <summary>
    /// Starts a diagnostic activity for sending a request and receiving a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being sent.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected.</typeparam>
    /// <returns>
    /// An <see cref="IDisposable"/> that represents the activity. Dispose to stop the activity.
    /// </returns>
    IDisposable? StartSend<TRequest, TResponse>();

    /// <summary>
    /// Starts a diagnostic activity for sending a request and receiving a response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response expected.</typeparam>
    /// <param name="request">The request being sent, used for tagging diagnostic information.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that represents the activity. Dispose to stop the activity.
    /// </returns>
    IDisposable? StartSend<TResponse>(IRequest<TResponse> request);

    /// <summary>
    /// Starts a diagnostic activity for sending a request.
    /// </summary>
    /// <param name="request">The request being sent, used for tagging diagnostic information.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that represents the activity. Dispose to stop the activity.
    /// </returns>
    IDisposable? StartSend(object request);

    /// <summary>
    /// Starts a diagnostic activity for publishing a notification.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification being published.</typeparam>
    /// <returns>
    /// An <see cref="IDisposable"/> that represents the activity. Dispose to stop the activity.
    /// </returns>
    IDisposable? StartPublish<TNotification>();

    /// <summary>
    /// Logs an error for a specific activity.
    /// </summary>
    /// <param name="activity">The activity during which the error occurred.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="request">The request or notification that caused the error.</param>
    /// <param name="memberName">The name of the member where the error occurred. Automatically provided by the compiler.</param>
    void ActivityError(
        IDisposable? activity,
        Exception? exception,
        object? request,
        [CallerMemberName] string memberName = "");
}
