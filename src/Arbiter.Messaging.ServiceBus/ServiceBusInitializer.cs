using Azure;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Initializes configured Azure Service Bus queues, topics, and subscriptions when the host starts.
/// </summary>
public sealed class ServiceBusInitializer : IHostedService
{
    private readonly ILogger<ServiceBusInitializer> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusInitializer" /> class.
    /// </summary>
    /// <param name="logger">The logger used to write initialization messages.</param>
    /// <param name="serviceProvider">The service provider used to resolve configured options and dependencies.</param>
    public ServiceBusInitializer(ILogger<ServiceBusInitializer> logger, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _logger = logger;
        _serviceProvider = serviceProvider;
    }


    /// <summary>
    /// Starts Service Bus resource initialization.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel initialization.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = _serviceProvider.GetServices<ServiceBusOptions>();
        if (options?.Any() != true)
        {
            _logger.LogDebug("No Azure Service Bus resources configured for initialization.");
            return;
        }

        foreach (var option in options)
        {
            try
            {
                await Initialize(option, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Azure Service Bus resources for '{NameOrConnectionString}': {ErrorMessage}", option.NameOrConnectionString, ex.Message);
            }
        }
    }

    /// <summary>
    /// Stops the hosted service.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the stop operation.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    private async Task Initialize(
        ServiceBusOptions option,
        CancellationToken cancellationToken)
    {
        var adminClient = _serviceProvider.GetRequiredKeyedService<ServiceBusAdministrationClient>(option.ServiceKey);

        foreach (var queue in option.Queues)
            await InitializeQueue(adminClient, option, queue, cancellationToken).ConfigureAwait(false);

        foreach (var topic in option.Topics)
            await InitializeTopic(adminClient, option, topic, cancellationToken).ConfigureAwait(false);
    }

    private async Task InitializeQueue(
        ServiceBusAdministrationClient adminClient,
        ServiceBusOptions option,
        string queueName,
        CancellationToken cancellationToken)
    {
        var queue = option.FormatName(queueName);

        var queueExists = await adminClient.QueueExistsAsync(queue, cancellationToken).ConfigureAwait(false);
        if (queueExists.Value)
            return;

        var options = new CreateQueueOptions(queue)
        {
            DefaultMessageTimeToLive = option.DefaultMessageTimeToLive,
            MaxDeliveryCount = option.MaxDeliveryCount,
            LockDuration = option.LockDuration,
            DeadLetteringOnMessageExpiration = option.DeadLetteringOnMessageExpiration,
            EnableBatchedOperations = option.EnableBatchedOperations,
        };

        _logger.LogInformation("Creating Queue '{QueueName}'", queue);

        try
        {
            await adminClient.CreateQueueAsync(options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            _logger.LogDebug(ex, "Queue '{QueueName}' already exists", queue);
        }
    }

    private async Task InitializeTopic(
        ServiceBusAdministrationClient adminClient,
        ServiceBusOptions option,
        string topicName,
        CancellationToken cancellationToken)
    {
        var topic = option.FormatName(topicName);

        var topicExists = await adminClient.TopicExistsAsync(topic, cancellationToken).ConfigureAwait(false);
        if (!topicExists.Value)
        {
            var topicOptions = new CreateTopicOptions(topic)
            {
                DefaultMessageTimeToLive = option.DefaultMessageTimeToLive,
                EnableBatchedOperations = option.EnableBatchedOperations,
            };

            _logger.LogInformation("Creating Topic '{TopicName}'", topic);

            try
            {
                await adminClient.CreateTopicAsync(topicOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                _logger.LogDebug(ex, "Topic '{TopicName}' already exists", topic);
            }
        }

        if (!option.Subscriptions.TryGetValue(topicName, out var subscriptions))
            return;

        foreach (var subscriptionName in subscriptions)
        {
            var subscriptionExists = await adminClient.SubscriptionExistsAsync(topic, subscriptionName, cancellationToken).ConfigureAwait(false);
            if (subscriptionExists.Value)
                continue;

            var subscriptionOptions = new CreateSubscriptionOptions(topic, subscriptionName)
            {
                DefaultMessageTimeToLive = option.DefaultMessageTimeToLive,
                MaxDeliveryCount = option.MaxDeliveryCount,
                LockDuration = option.LockDuration,
                DeadLetteringOnMessageExpiration = option.DeadLetteringOnMessageExpiration,
                EnableBatchedOperations = option.EnableBatchedOperations,
            };

            _logger.LogInformation("Creating Subscription '{SubscriptionName}' for Topic '{TopicName}'", subscriptionName, topic);

            try
            {
                await adminClient.CreateSubscriptionAsync(subscriptionOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                _logger.LogDebug(ex, "Subscription '{SubscriptionName}' for Topic '{TopicName}' already exists", subscriptionName, topic);
            }
        }
    }
}
