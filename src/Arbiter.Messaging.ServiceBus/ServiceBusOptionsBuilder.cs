namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides a builder for configuring Azure Service Bus runtime options using values resolved from the
/// <see cref="IServiceProvider" />, such as the current environment name.
/// </summary>
public sealed class ServiceBusOptionsBuilder
{
    private readonly ServiceBusOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusOptionsBuilder" /> class.
    /// </summary>
    /// <param name="options">The options instance the builder configures.</param>
    /// <param name="services">The service provider used to resolve runtime configuration values.</param>
    public ServiceBusOptionsBuilder(ServiceBusOptions options, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(services);

        _options = options;
        Services = services;
    }

    /// <summary>
    /// Gets the service provider used to resolve runtime configuration values.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Sets the suffix appended to queue and topic names when formatting entity names.
    /// </summary>
    /// <param name="nameSuffix">The name suffix to append.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithNameSuffix(string? nameSuffix)
    {
        _options.WithNameSuffix(nameSuffix);
        return this;
    }

    /// <summary>
    /// Sets the default message time-to-live for created entities.
    /// </summary>
    /// <param name="timeToLive">The default message time-to-live.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithDefaultMessageTimeToLive(TimeSpan timeToLive)
    {
        _options.WithDefaultMessageTimeToLive(timeToLive);
        return this;
    }

    /// <summary>
    /// Sets the maximum delivery count before a message is dead-lettered.
    /// </summary>
    /// <param name="maxDeliveryCount">The maximum delivery count.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithMaxDeliveryCount(int maxDeliveryCount)
    {
        _options.WithMaxDeliveryCount(maxDeliveryCount);
        return this;
    }

    /// <summary>
    /// Sets the lock duration for received messages.
    /// </summary>
    /// <param name="lockDuration">The message lock duration.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithLockDuration(TimeSpan lockDuration)
    {
        _options.WithLockDuration(lockDuration);
        return this;
    }

    /// <summary>
    /// Sets whether expired messages are moved to the dead-letter queue.
    /// </summary>
    /// <param name="deadLetteringOnMessageExpiration">A value indicating whether expired messages are dead-lettered.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithDeadLetteringOnMessageExpiration(bool deadLetteringOnMessageExpiration)
    {
        _options.WithDeadLetteringOnMessageExpiration(deadLetteringOnMessageExpiration);
        return this;
    }

    /// <summary>
    /// Sets whether batched operations are enabled for created entities.
    /// </summary>
    /// <param name="enableBatchedOperations">A value indicating whether batched operations are enabled.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithEnableBatchedOperations(bool enableBatchedOperations)
    {
        _options.WithEnableBatchedOperations(enableBatchedOperations);
        return this;
    }

    /// <summary>
    /// Sets the idle duration after which a created subscription is automatically deleted.
    /// </summary>
    /// <param name="autoDeleteOnIdle">The idle duration before automatic deletion. Must be at least five minutes.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithAutoDeleteOnIdle(TimeSpan autoDeleteOnIdle)
    {
        _options.WithAutoDeleteOnIdle(autoDeleteOnIdle);
        return this;
    }

    /// <summary>
    /// Sets whether created queues and subscriptions require sessions.
    /// </summary>
    /// <param name="requiresSession">A value indicating whether sessions are required.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithRequiresSession(bool requiresSession = true)
    {
        _options.WithRequiresSession(requiresSession);
        return this;
    }

    /// <summary>
    /// Enables duplicate detection for created queues and topics using the specified history time window.
    /// </summary>
    /// <param name="duplicateDetectionHistoryTimeWindow">The duplicate detection history time window. Must be between 20 seconds and 7 days.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithDuplicateDetection(TimeSpan duplicateDetectionHistoryTimeWindow)
    {
        _options.WithDuplicateDetection(duplicateDetectionHistoryTimeWindow);
        return this;
    }

    /// <summary>
    /// Sets the maximum size, in megabytes, of created queues and topics.
    /// </summary>
    /// <param name="maxSizeInMegabytes">The maximum entity size in megabytes.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithMaxSizeInMegabytes(long maxSizeInMegabytes)
    {
        _options.WithMaxSizeInMegabytes(maxSizeInMegabytes);
        return this;
    }

    /// <summary>
    /// Sets the maximum message size, in kilobytes, for created queues and topics. Premium namespaces only.
    /// </summary>
    /// <param name="maxMessageSizeInKilobytes">The maximum message size in kilobytes, or <see langword="null" /> to use the namespace default.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithMaxMessageSizeInKilobytes(long? maxMessageSizeInKilobytes)
    {
        _options.WithMaxMessageSizeInKilobytes(maxMessageSizeInKilobytes);
        return this;
    }

    /// <summary>
    /// Sets whether created queues and topics are partitioned.
    /// </summary>
    /// <param name="enablePartitioning">A value indicating whether partitioning is enabled.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithEnablePartitioning(bool enablePartitioning = true)
    {
        _options.WithEnablePartitioning(enablePartitioning);
        return this;
    }
}
