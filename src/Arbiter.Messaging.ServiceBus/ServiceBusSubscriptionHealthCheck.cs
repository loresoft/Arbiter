using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Performs a health check for an Azure Service Bus subscription by reading runtime properties.
/// </summary>
public sealed partial class ServiceBusSubscriptionHealthCheck : IHealthCheck
{
    private readonly ServiceBusAdministrationClient _administrationClient;
    private readonly ILogger<ServiceBusSubscriptionHealthCheck> _logger;
    private readonly ServiceBusSender _queueSender;
    private readonly string _subscriptionName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusSubscriptionHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write health check diagnostics.</param>
    /// <param name="administrationClient">The Service Bus administration client.</param>
    /// <param name="queueSender">The Service Bus sender for the topic associated with the subscription.</param>
    /// <param name="subscriptionName">The subscription name to check.</param>
    public ServiceBusSubscriptionHealthCheck(
        ILogger<ServiceBusSubscriptionHealthCheck> logger,
        ServiceBusAdministrationClient administrationClient,
        ServiceBusSender queueSender,
        string subscriptionName)
    {
        _logger = logger;
        _administrationClient = administrationClient;
        _queueSender = queueSender;
        _subscriptionName = subscriptionName;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // using the sender's EntityPath to determine the topic name for health check
        var serviceBusNamespace = _queueSender.FullyQualifiedNamespace;
        var topicName = _queueSender.EntityPath;

        try
        {
            var response = await _administrationClient
                .GetSubscriptionRuntimePropertiesAsync(topicName, _subscriptionName, cancellationToken)
                .ConfigureAwait(false);

            var properties = response.Value;
            var logLevel = properties.DeadLetterMessageCount > 0 ? LogLevel.Warning : LogLevel.Information;

            LogSubscriptionHealth(
                _logger,
                logLevel,
                serviceBusNamespace,
                properties.SubscriptionName,
                properties.TopicName,
                properties.ActiveMessageCount,
                properties.DeadLetterMessageCount,
                properties.TotalMessageCount);

            var message = $"Service Bus '{serviceBusNamespace}' Subscription '{_subscriptionName}' on Topic '{topicName}'; " +
                $"Active: {properties.ActiveMessageCount},  " +
                $"Dead: {properties.DeadLetterMessageCount}, " +
                $"Total: {properties.TotalMessageCount}";

            if (properties.DeadLetterMessageCount > 0)
                return HealthCheckResult.Degraded(message);

            return HealthCheckResult.Healthy(message);
        }
        catch (Exception ex)
        {
            LogSubscriptionError(_logger, ex, serviceBusNamespace, _subscriptionName, topicName, ex.Message);

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Service Bus '{serviceBusNamespace}' Subscription '{_subscriptionName}' on Topic '{topicName}' health check failed: {ex.Message}",
                exception: ex);
        }
    }

    [LoggerMessage(Message = "Service Bus '{ServiceBusNamespace}' Subscription '{SubscriptionName}' on Topic '{TopicName}' health check; Active: {ActiveMessages}, Dead: {DeadLetterMessages}, Total: {TotalMessages}")]
    private static partial void LogSubscriptionHealth(
        ILogger logger,
        LogLevel logLevel,
        string serviceBusNamespace,
        string subscriptionName,
        string topicName,
        long activeMessages,
        long deadLetterMessages,
        long totalMessages);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error checking subscription health for namespace '{ServiceBusNamespace}', subscription '{SubscriptionName}' on Topic '{TopicName}': {ErrorMessage}")]
    private static partial void LogSubscriptionError(
        ILogger logger,
        Exception exception,
        string serviceBusNamespace,
        string subscriptionName,
        string topicName,
        string errorMessage);

}
