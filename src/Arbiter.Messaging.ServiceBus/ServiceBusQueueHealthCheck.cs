using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Performs a health check for an Azure Service Bus queue by reading runtime properties.
/// </summary>
public sealed partial class ServiceBusQueueHealthCheck : IHealthCheck
{
    private readonly ServiceBusAdministrationClient _administrationClient;
    private readonly ILogger<ServiceBusQueueHealthCheck> _logger;
    private readonly ServiceBusSender _queueSender;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceBusQueueHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger used to write health check diagnostics.</param>
    /// <param name="administrationClient">The Service Bus administration client.</param>
    /// <param name="queueSender">The Service Bus sender for the queue being checked.</param>
    public ServiceBusQueueHealthCheck(
        ILogger<ServiceBusQueueHealthCheck> logger,
        ServiceBusAdministrationClient administrationClient,
        ServiceBusSender queueSender)
    {
        _logger = logger;
        _administrationClient = administrationClient;
        _queueSender = queueSender;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // using the sender's EntityPath to determine the queue name for health check
        var serviceBusNamespace = _queueSender.FullyQualifiedNamespace;
        var queueName = _queueSender.EntityPath;

        try
        {
            var response = await _administrationClient
                .GetQueueRuntimePropertiesAsync(queueName, cancellationToken)
                .ConfigureAwait(false);

            var properties = response.Value;
            var logLevel = properties.DeadLetterMessageCount > 0 ? LogLevel.Warning : LogLevel.Information;

            LogQueueHealth(
                _logger,
                logLevel,
                serviceBusNamespace,
                properties.Name,
                properties.ActiveMessageCount,
                properties.DeadLetterMessageCount,
                properties.TotalMessageCount);

            var message = $"Service Bus '{serviceBusNamespace}' Queue '{queueName}'; " +
                $"Active: {properties.ActiveMessageCount},  " +
                $"Dead: {properties.DeadLetterMessageCount}, " +
                $"Total: {properties.TotalMessageCount}";

            if (properties.DeadLetterMessageCount > 0)
                return HealthCheckResult.Degraded(message);

            return HealthCheckResult.Healthy(message);
        }
        catch (Exception ex)
        {
            LogQueueError(_logger, ex, serviceBusNamespace, queueName, ex.Message);

            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: $"Service Bus '{serviceBusNamespace}' Queue '{queueName}' health check failed: {ex.Message}",
                exception: ex);
        }
    }


    [LoggerMessage(Message = "Service Bus '{ServiceBusNamespace}' Queue '{QueueName}' health check; Active: {ActiveMessages}, Dead: {DeadLetterMessages}, Total: {TotalMessages}")]
    private static partial void LogQueueHealth(
        ILogger logger,
        LogLevel logLevel,
        string serviceBusNamespace,
        string queueName,
        long activeMessages,
        long deadLetterMessages,
        long totalMessages);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error checking queue health for namespace '{ServiceBusNamespace}', queue '{QueueName}': {ErrorMessage}")]
    private static partial void LogQueueError(
        ILogger logger,
        Exception exception,
        string serviceBusNamespace,
        string queueName,
        string errorMessage);
}
