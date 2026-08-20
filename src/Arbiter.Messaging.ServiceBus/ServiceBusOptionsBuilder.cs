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
    /// Sets the suffix appended to queue and topic names using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the name suffix.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithNameSuffix(Func<IServiceProvider, string?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithNameSuffix(factory(Services));
        return this;
    }

    /// <summary>
    /// Sets the suffix appended to subscription names when formatting subscription names.
    /// </summary>
    /// <param name="subscriptionSuffix">The subscription name suffix to append.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    public ServiceBusOptionsBuilder WithSubscriptionSuffix(string? subscriptionSuffix)
    {
        _options.WithSubscriptionSuffix(subscriptionSuffix);
        return this;
    }

    /// <summary>
    /// Sets the suffix appended to subscription names using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the subscription name suffix.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithSubscriptionSuffix(Func<IServiceProvider, string?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithSubscriptionSuffix(factory(Services));
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
    /// Sets the default message time-to-live using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the default message time-to-live.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithDefaultMessageTimeToLive(Func<IServiceProvider, TimeSpan> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithDefaultMessageTimeToLive(factory(Services));
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
    /// Sets the maximum delivery count using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the maximum delivery count.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithMaxDeliveryCount(Func<IServiceProvider, int> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithMaxDeliveryCount(factory(Services));
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
    /// Sets the message lock duration using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the message lock duration.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithLockDuration(Func<IServiceProvider, TimeSpan> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithLockDuration(factory(Services));
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
    /// Sets whether expired messages are dead-lettered using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether expired messages are dead-lettered.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithDeadLetteringOnMessageExpiration(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithDeadLetteringOnMessageExpiration(factory(Services));
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
    /// Sets whether batched operations are enabled using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether batched operations are enabled.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithEnableBatchedOperations(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithEnableBatchedOperations(factory(Services));
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
    /// Sets the idle duration before automatic deletion using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the idle duration before automatic deletion.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithAutoDeleteOnIdle(Func<IServiceProvider, TimeSpan> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithAutoDeleteOnIdle(factory(Services));
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
    /// Sets whether sessions are required using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether sessions are required.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithRequiresSession(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithRequiresSession(factory(Services));
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
    /// Enables duplicate detection using a history time window resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the duplicate detection history time window.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithDuplicateDetection(Func<IServiceProvider, TimeSpan> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithDuplicateDetection(factory(Services));
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
    /// Sets the maximum entity size, in megabytes, using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the maximum entity size in megabytes.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithMaxSizeInMegabytes(Func<IServiceProvider, long> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithMaxSizeInMegabytes(factory(Services));
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
    /// Sets the maximum message size, in kilobytes, using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the maximum message size in kilobytes.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithMaxMessageSizeInKilobytes(Func<IServiceProvider, long?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithMaxMessageSizeInKilobytes(factory(Services));
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

    /// <summary>
    /// Sets whether partitioning is enabled using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether partitioning is enabled.</param>
    /// <returns>The current <see cref="ServiceBusOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public ServiceBusOptionsBuilder WithEnablePartitioning(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _options.WithEnablePartitioning(factory(Services));
        return this;
    }
}
