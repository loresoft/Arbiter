using Azure;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Initializes configured Azure Service Bus queues, topics, and subscriptions when the host starts.
/// </summary>
public sealed partial class ServiceBusInitializer : IHostedService
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
        var registrations = _serviceProvider.GetServices<ServiceBusRegistration>();
        if (registrations?.Any() != true)
        {
            LogNoResourcesConfigured(_logger);
            return;
        }

        var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<ServiceBusOptions>>();

        foreach (var registration in registrations)
        {
            var option = optionsMonitor.Get(registration.OptionsName);
            try
            {
                await Initialize(option, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogErrorInitializingResources(_logger, ex, option.NameOrConnectionString);
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

        LogCreatingQueue(_logger, queue);

        try
        {
            await adminClient.CreateQueueAsync(options, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            LogQueueAlreadyExists(_logger, ex, queue);
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

            LogCreatingTopic(_logger, topic);

            try
            {
                await adminClient.CreateTopicAsync(topicOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                LogTopicAlreadyExists(_logger, ex, topic);
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

            LogCreatingSubscription(_logger, subscriptionName, topic);

            try
            {
                await adminClient.CreateSubscriptionAsync(subscriptionOptions, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                LogSubscriptionAlreadyExists(_logger, ex, subscriptionName, topic);
            }
        }
    }


    [LoggerMessage(Level = LogLevel.Debug, Message = "No Azure Service Bus resources configured for initialization.")]
    private static partial void LogNoResourcesConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error initializing Azure Service Bus resources for '{NameOrConnectionString}'")]
    private static partial void LogErrorInitializingResources(ILogger logger, Exception exception, string nameOrConnectionString);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Queue '{QueueName}'")]
    private static partial void LogCreatingQueue(ILogger logger, string queueName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Queue '{QueueName}' already exists")]
    private static partial void LogQueueAlreadyExists(ILogger logger, Exception exception, string queueName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Topic '{TopicName}'")]
    private static partial void LogCreatingTopic(ILogger logger, string topicName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Topic '{TopicName}' already exists")]
    private static partial void LogTopicAlreadyExists(ILogger logger, Exception exception, string topicName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Subscription '{SubscriptionName}' for Topic '{TopicName}'")]
    private static partial void LogCreatingSubscription(ILogger logger, string subscriptionName, string topicName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Subscription '{SubscriptionName}' for Topic '{TopicName}' already exists")]
    private static partial void LogSubscriptionAlreadyExists(ILogger logger, Exception exception, string subscriptionName, string topicName);
}
