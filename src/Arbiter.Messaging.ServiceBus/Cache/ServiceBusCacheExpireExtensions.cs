using Arbiter.Mediation;
using Arbiter.Messaging.ServiceBus.Cache;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Messaging.ServiceBus;

/// <summary>
/// Provides opt-in extension methods for distributed cache expiration over Azure Service Bus.
/// </summary>
/// <remarks>
/// The topic and subscription used for cache expiration must be declared in the <c>configureBus</c> delegate of
/// <see cref="ServiceBusExtensions.AddServiceBus(IServiceCollection, object, string, System.Action{ServiceBusEntityBuilder}, System.Action{ServiceBusOptionsBuilder})"/>
/// so they are provisioned by the initializer and a sender is registered for the topic.
/// </remarks>
public static class ServiceBusCacheExpireExtensions
{
    /// <summary>
    /// Registers the publisher behavior that expires the local cache and publishes a
    /// <see cref="CacheExpireMessage"/> to the configured Service Bus topic.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="topicName">The name of the Service Bus topic to publish cache expiration messages to.</param>
    /// <param name="configureOptions">An optional delegate to configure the <see cref="CacheExpireOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusCacheExpirePublisher(
        this IServiceCollection services,
        string topicName,
        Action<CacheExpireOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);

        AddCacheExpireOptions(services, topicName, configureOptions);

        // applies to all commands implementing ICacheExpire, mirroring HybridCacheExpireBehavior registration
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ServiceBusCacheExpireBehavior<,>));

        return services;
    }

    /// <summary>
    /// Registers the subscriber processor that receives <see cref="CacheExpireMessage"/> messages and expires
    /// the matching entries from the local cache.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered <see cref="ServiceBusClient"/> and options.</param>
    /// <param name="topicName">The name of the Service Bus topic to receive cache expiration messages from.</param>
    /// <param name="subscriptionName">The name of the topic subscription to receive messages from.</param>
    /// <param name="configureOptions">An optional delegate to configure the <see cref="CacheExpireOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusCacheExpireSubscriber(
        this IServiceCollection services,
        object? serviceName,
        string topicName,
        string subscriptionName,
        Action<CacheExpireOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        AddCacheExpireOptions(services, topicName, configureOptions);

        services.AddServiceBusProcessor<CacheExpireProcessor>(serviceName, topicName, subscriptionName);

        return services;
    }

    /// <summary>
    /// Registers both the publisher behavior and the subscriber processor for distributed cache expiration.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The name used to resolve the registered <see cref="ServiceBusClient"/> and options.</param>
    /// <param name="topicName">The name of the Service Bus topic used for cache expiration messages.</param>
    /// <param name="subscriptionName">The name of the topic subscription to receive messages from.</param>
    /// <param name="configureOptions">An optional delegate to configure the <see cref="CacheExpireOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddServiceBusCacheExpire(
        this IServiceCollection services,
        object? serviceName,
        string topicName,
        string subscriptionName,
        Action<CacheExpireOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicName);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionName);

        AddServiceBusCacheExpirePublisher(services, topicName, configureOptions);
        AddServiceBusCacheExpireSubscriber(services, serviceName, topicName, subscriptionName, configureOptions);

        return services;
    }

    private static void AddCacheExpireOptions(
        IServiceCollection services,
        string topicName,
        Action<CacheExpireOptions>? configureOptions)
    {
        // single per-process options instance shared by the publisher behavior and subscriber processor
        services.TryAddSingleton(_ =>
        {
            var options = new CacheExpireOptions { TopicName = topicName };
            configureOptions?.Invoke(options);
            return options;
        });
    }
}
