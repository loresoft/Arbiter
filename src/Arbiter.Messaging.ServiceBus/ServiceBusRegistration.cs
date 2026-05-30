namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Represents a registered Azure Service Bus configuration, used to enumerate configured instances and resolve their named options.
/// </summary>
/// <param name="ServiceKey">The dependency injection key used to register the Service Bus client.</param>
/// <param name="OptionsName">The named options instance used to resolve the configured <see cref="ServiceBusOptions" />.</param>
internal sealed record ServiceBusRegistration(
    object? ServiceKey,
    string OptionsName
);
