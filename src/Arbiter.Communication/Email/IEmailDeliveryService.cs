namespace Arbiter.Communication.Email;

/// <summary>
/// Provides a service for delivering email messages.
/// </summary>
public interface IEmailDeliveryService
{
    /// <summary>
    /// Sends the specified <see cref="EmailMessage"/> asynchronously.
    /// </summary>
    /// <param name="emailMessage">The email message to send, including sender, recipients, content, headers, and attachments.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="EmailResult"/> indicating whether the email was sent successfully,
    /// including any error message or exception if the operation failed.
    /// </returns>
    Task<EmailResult> Send(
        EmailMessage emailMessage,
        CancellationToken cancellationToken = default);
}
