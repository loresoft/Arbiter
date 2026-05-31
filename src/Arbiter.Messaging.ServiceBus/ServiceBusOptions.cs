using Azure.Core;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Represents configuration options for Azure Service Bus clients and entities.
/// </summary>
public sealed class ServiceBusOptions
{
    /// <summary>
    /// Gets or sets the dependency injection key used to register the Service Bus client.
    /// </summary>
    public object? ServiceKey { get; set; }

    /// <summary>
    /// Gets or sets a Service Bus connection string, connection string name, or configuration key.
    /// When <see cref="Credential" /> is provided, this value is the fully qualified Service Bus namespace
    /// (for example, <c>my-namespace.servicebus.windows.net</c>).
    /// </summary>
    public string NameOrConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets the <see cref="TokenCredential" /> used for identity-based authentication.
    /// When set, <see cref="NameOrConnectionString" /> is treated as the fully qualified Service Bus namespace.
    /// When <see langword="null" />, connection-string based authentication is used.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// Gets or sets the suffix appended to queue and topic names when formatting entity names.
    /// </summary>
    public string? NameSuffix { get; set; }


    /// <summary>
    /// Gets or sets the queue names to initialize and register senders for.
    /// </summary>
    public IList<string> Queues { get; set; } = [];

    /// <summary>
    /// Gets or sets the topic names to initialize and register senders for.
    /// </summary>
    public IList<string> Topics { get; set; } = [];

    /// <summary>
    /// Gets or sets subscription names grouped by topic name.
    /// </summary>
    public IDictionary<string, IList<string>> Subscriptions { get; set; } = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Gets or sets the default time-to-live for messages in created entities.
    /// </summary>
    public TimeSpan DefaultMessageTimeToLive { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the maximum delivery count before a message is dead-lettered.
    /// </summary>
    public int MaxDeliveryCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets the lock duration for received messages.
    /// </summary>
    public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets a value indicating whether expired messages are moved to the dead-letter queue.
    /// </summary>
    public bool DeadLetteringOnMessageExpiration { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether batched operations are enabled for created entities.
    /// </summary>
    public bool EnableBatchedOperations { get; set; } = true;


    /// <summary>
    /// Sets the default message time-to-live for created entities.
    /// </summary>
    /// <param name="timeToLive">The default message time-to-live.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithDefaultMessageTimeToLive(TimeSpan timeToLive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeToLive, TimeSpan.Zero);

        DefaultMessageTimeToLive = timeToLive;
        return this;
    }

    /// <summary>
    /// Sets the maximum delivery count before a message is dead-lettered.
    /// </summary>
    /// <param name="maxDeliveryCount">The maximum delivery count.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithMaxDeliveryCount(int maxDeliveryCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDeliveryCount, 1);

        MaxDeliveryCount = maxDeliveryCount;
        return this;
    }

    /// <summary>
    /// Sets the lock duration for received messages.
    /// </summary>
    /// <param name="lockDuration">The message lock duration.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithLockDuration(TimeSpan lockDuration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lockDuration, TimeSpan.Zero);

        LockDuration = lockDuration;
        return this;
    }

    /// <summary>
    /// Sets whether expired messages are moved to the dead-letter queue.
    /// </summary>
    /// <param name="deadLetteringOnMessageExpiration">A value indicating whether expired messages are dead-lettered.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithDeadLetteringOnMessageExpiration(bool deadLetteringOnMessageExpiration)
    {
        DeadLetteringOnMessageExpiration = deadLetteringOnMessageExpiration;
        return this;
    }

    /// <summary>
    /// Sets whether batched operations are enabled for created entities.
    /// </summary>
    /// <param name="enableBatchedOperations">A value indicating whether batched operations are enabled.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithEnableBatchedOperations(bool enableBatchedOperations)
    {
        EnableBatchedOperations = enableBatchedOperations;
        return this;
    }

    /// <summary>
    /// Sets the suffix appended to queue and topic names when formatting entity names.
    /// </summary>
    /// <param name="nameSuffix">The name suffix to append.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithNameSuffix(string? nameSuffix)
    {
        NameSuffix = nameSuffix;
        return this;
    }


    /// <summary>
    /// Adds a queue to initialize and register a sender for.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions AddQueue(string queueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueName);

        Queues.Add(queueName);
        return this;
    }

    /// <summary>
    /// Adds a topic to initialize and register a sender for.
    /// </summary>
    /// <param name="topicName">The topic name.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions AddTopic(string topicName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);

        Topics.Add(topicName);
        return this;
    }

    /// <summary>
    /// Adds a topic to initialize and register a sender for, along with subscriptions to initialize for that topic.
    /// </summary>
    /// <param name="topicName">The topic name.</param>
    /// <param name="subscriptions">The subscription names to create for the topic.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions AddTopic(string topicName, params IEnumerable<string> subscriptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentNullException.ThrowIfNull(subscriptions);

        Topics.Add(topicName);
        Subscriptions[topicName] = [.. subscriptions];

        return this;
    }


    /// <summary>
    /// Formats an entity name by appending the configured suffix when one is provided.
    /// </summary>
    /// <param name="baseName">The base queue or topic name.</param>
    /// <returns>The formatted entity name.</returns>
    public string FormatName(string baseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);

        return string.IsNullOrWhiteSpace(NameSuffix) ? baseName : $"{baseName}-{NameSuffix}";
    }
}
