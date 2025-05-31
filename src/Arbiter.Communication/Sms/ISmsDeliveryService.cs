namespace Arbiter.Communication.Sms;

/// <summary>
/// Provides a service for delivering SMS messages.
/// </summary>
public interface ISmsDeliveryService
{
    /// <summary>
    /// Sends an SMS message.
    /// </summary>
    /// <param name="message">The SMS message to send.</param>
    /// <param name="cancellationToken"> A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an <see cref="SmsResult"/> indicating whether the SMS was sent successfully,
    /// including any error message or exception if the operation failed.
    /// </returns>
    Task<SmsResult> Send(SmsMessage message, CancellationToken cancellationToken = default);
}
