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
    /// Gets or sets the idle duration after which a created subscription is automatically deleted.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="TimeSpan.MaxValue" />, which never auto-deletes. Set a finite value when each
    /// application instance creates its own uniquely named subscription, so subscriptions abandoned by restarted
    /// or scaled-in instances are reclaimed instead of accumulating against the namespace subscription limit.
    /// The minimum value supported by Azure Service Bus is five minutes.
    /// </remarks>
    public TimeSpan AutoDeleteOnIdle { get; set; } = TimeSpan.MaxValue;

    /// <summary>
    /// Gets or sets a value indicating whether created queues and subscriptions require sessions.
    /// </summary>
    /// <remarks>
    /// Enable for ordered, session-based processing. This value can only be applied when the entity is created;
    /// existing entities are not modified.
    /// </remarks>
    public bool RequiresSession { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether created queues and topics detect duplicate messages.
    /// </summary>
    /// <remarks>
    /// Duplicates are detected using the message identifier within <see cref="DuplicateDetectionHistoryTimeWindow" />.
    /// This value can only be applied when the entity is created; existing entities are not modified.
    /// </remarks>
    public bool RequiresDuplicateDetection { get; set; }

    /// <summary>
    /// Gets or sets the time window used for duplicate detection when <see cref="RequiresDuplicateDetection" /> is enabled.
    /// </summary>
    /// <remarks>
    /// Azure Service Bus supports values between 20 seconds and 7 days.
    /// </remarks>
    public TimeSpan DuplicateDetectionHistoryTimeWindow { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets or sets the maximum size, in megabytes, of created queues and topics.
    /// </summary>
    /// <remarks>
    /// Defaults to 1024 MB. This value can only be applied when the entity is created; existing entities are not modified.
    /// </remarks>
    public long MaxSizeInMegabytes { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the maximum message size, in kilobytes, for created queues and topics.
    /// </summary>
    /// <remarks>
    /// Supported only by Premium namespaces. Leave <see langword="null" /> for Standard namespaces, where setting a
    /// value causes entity creation to fail.
    /// </remarks>
    public long? MaxMessageSizeInKilobytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether created queues and topics are partitioned.
    /// </summary>
    /// <remarks>
    /// Partitioning increases throughput and availability. This value can only be applied when the entity is created;
    /// existing entities are not modified.
    /// </remarks>
    public bool EnablePartitioning { get; set; }


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
    /// Sets the idle duration after which a created subscription is automatically deleted.
    /// </summary>
    /// <param name="autoDeleteOnIdle">The idle duration before automatic deletion. Must be at least five minutes.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithAutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(autoDeleteOnIdle, TimeSpan.FromMinutes(5));

        AutoDeleteOnIdle = autoDeleteOnIdle;
        return this;
    }

    /// <summary>
    /// Sets whether created queues and subscriptions require sessions.
    /// </summary>
    /// <param name="requiresSession">A value indicating whether sessions are required.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithRequiresSession(bool requiresSession = true)
    {
        RequiresSession = requiresSession;
        return this;
    }

    /// <summary>
    /// Enables duplicate detection for created queues and topics using the specified history time window.
    /// </summary>
    /// <param name="duplicateDetectionHistoryTimeWindow">The duplicate detection history time window. Must be between 20 seconds and 7 days.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithDuplicateDetection(TimeSpan duplicateDetectionHistoryTimeWindow)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(duplicateDetectionHistoryTimeWindow, TimeSpan.FromSeconds(20));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(duplicateDetectionHistoryTimeWindow, TimeSpan.FromDays(7));

        RequiresDuplicateDetection = true;
        DuplicateDetectionHistoryTimeWindow = duplicateDetectionHistoryTimeWindow;
        return this;
    }

    /// <summary>
    /// Sets the maximum size, in megabytes, of created queues and topics.
    /// </summary>
    /// <param name="maxSizeInMegabytes">The maximum entity size in megabytes.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithMaxSizeInMegabytes(long maxSizeInMegabytes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSizeInMegabytes, 1);

        MaxSizeInMegabytes = maxSizeInMegabytes;
        return this;
    }

    /// <summary>
    /// Sets the maximum message size, in kilobytes, for created queues and topics. Premium namespaces only.
    /// </summary>
    /// <param name="maxMessageSizeInKilobytes">The maximum message size in kilobytes, or <see langword="null" /> to use the namespace default.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithMaxMessageSizeInKilobytes(long? maxMessageSizeInKilobytes)
    {
        if (maxMessageSizeInKilobytes is { } size)
            ArgumentOutOfRangeException.ThrowIfLessThan(size, 1, nameof(maxMessageSizeInKilobytes));

        MaxMessageSizeInKilobytes = maxMessageSizeInKilobytes;
        return this;
    }

    /// <summary>
    /// Sets whether created queues and topics are partitioned.
    /// </summary>
    /// <param name="enablePartitioning">A value indicating whether partitioning is enabled.</param>
    /// <returns>The current <see cref="ServiceBusOptions" /> instance for chaining.</returns>
    public ServiceBusOptions WithEnablePartitioning(bool enablePartitioning = true)
    {
        EnablePartitioning = enablePartitioning;
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
