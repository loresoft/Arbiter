using System.Collections.Concurrent;

namespace Arbiter.Communication.Email;

/// <summary>
/// Provides an in-memory implementation of <see cref="IEmailDeliveryService"/> for storing and simulating email delivery.
/// </summary>
/// <remarks>
/// This service is useful for testing and development scenarios where actual email delivery is not required.
/// Sent emails are stored in a memory queue up to the specified <see cref="Capacity"/>.
/// </remarks>
public sealed class MemoryEmailDeliverService : IEmailDeliveryService
{
    /// <summary>
    /// Gets or sets the maximum number of email messages to retain in memory.
    /// When the capacity is exceeded, the oldest messages are discarded.
    /// </summary>
    public int Capacity { get; set; } = 100;

    /// <summary>
    /// Gets an enumerable collection of all email messages currently stored in memory.
    /// </summary>
    public ConcurrentQueue<EmailMessage> Messages { get; } = new();

    /// <summary>
    /// Simulates sending an email by enqueuing the message in memory.
    /// If the queue exceeds <see cref="Capacity"/>, the oldest messages are removed.
    /// </summary>
    /// <param name="emailMessage">The email message to store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests (not used).</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a successful <see cref="EmailResult"/>.
    /// </returns>
    public Task<EmailResult> Send(EmailMessage emailMessage, CancellationToken cancellationToken = default)
    {
        Messages.Enqueue(emailMessage);

        // ensure capacity
        while (Messages.Count > Capacity)
        {
            if (!Messages.TryDequeue(out _))
                break;
        }

        return Task.FromResult(EmailResult.Success());
    }
}
