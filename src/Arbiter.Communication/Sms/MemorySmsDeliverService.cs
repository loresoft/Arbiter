using System.Collections.Concurrent;

namespace Arbiter.Communication.Sms;

/// <summary>
/// Provides an in-memory implementation of <see cref="ISmsDeliveryService"/> for storing and simulating SMS delivery.
/// </summary>
/// <remarks>
/// This service is useful for testing and development scenarios where actual SMS delivery is not required.
/// Sent messages are stored in a memory queue up to the specified <see cref="Capacity"/>.
/// </remarks>
public sealed class MemorySmsDeliverService : ISmsDeliveryService
{
    /// <summary>
    /// Gets or sets the maximum number of SMS messages to retain in memory.
    /// When the capacity is exceeded, the oldest messages are discarded.
    /// </summary>
    public int Capacity { get; set; } = 100;

    /// <summary>
    /// Gets an enumerable collection of all SMS messages currently stored in memory.
    /// </summary>
    public ConcurrentQueue<SmsMessage> Messages { get; } = new();

    /// <summary>
    /// Simulates sending an SMS by enqueuing the message in memory.
    /// If the queue exceeds <see cref="Capacity"/>, the oldest messages are removed.
    /// </summary>
    /// <param name="message">The SMS message to store.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests (not used).</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to a successful <see cref="SmsResult"/>.
    /// </returns>
    public Task<SmsResult> Send(SmsMessage message, CancellationToken cancellationToken = default)
    {
        Messages.Enqueue(message);

        // ensure capacity
        while (Messages.Count > Capacity)
        {
            if (!Messages.TryDequeue(out _))
                break;
        }

        return Task.FromResult(SmsResult.Success());
    }
}
