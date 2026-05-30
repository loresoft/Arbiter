namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides a builder for declaring the Azure Service Bus entities (queues, topics, and subscriptions)
/// to initialize and register senders for.
/// </summary>
public sealed class ServiceBusEntityBuilder
{
    private readonly ServiceBusOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusEntityBuilder" /> class.
    /// </summary>
    /// <param name="options">The options instance the builder configures.</param>
    public ServiceBusEntityBuilder(ServiceBusOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <summary>
    /// Adds a queue to initialize and register a sender for.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <returns>The current <see cref="ServiceBusEntityBuilder" /> instance for chaining.</returns>
    public ServiceBusEntityBuilder AddQueue(string queueName)
    {
        _options.AddQueue(queueName);
        return this;
    }

    /// <summary>
    /// Adds a topic to initialize and register a sender for.
    /// </summary>
    /// <param name="topicName">The topic name.</param>
    /// <returns>The current <see cref="ServiceBusEntityBuilder" /> instance for chaining.</returns>
    public ServiceBusEntityBuilder AddTopic(string topicName)
    {
        _options.AddTopic(topicName);
        return this;
    }

    /// <summary>
    /// Adds a topic to initialize and register a sender for, along with subscriptions to initialize for that topic.
    /// </summary>
    /// <param name="topicName">The topic name.</param>
    /// <param name="subscriptions">The subscription names to create for the topic.</param>
    /// <returns>The current <see cref="ServiceBusEntityBuilder" /> instance for chaining.</returns>
    public ServiceBusEntityBuilder AddTopic(string topicName, params IEnumerable<string> subscriptions)
    {
        _options.AddTopic(topicName, subscriptions);
        return this;
    }
}
