using Arbiter.Mediation;
using Arbiter.Messaging.WebPubSub.Cache;

using Azure.Messaging.WebPubSub;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides opt-in extension methods for distributed cache expiration over Azure Web PubSub.
/// </summary>
/// <remarks>
/// The hub used for cache expiration must be declared in the <c>configureHubs</c> delegate of
/// <see cref="WebPubSubExtensions.AddWebPubSub(IServiceCollection, object, string, System.Action{WebPubSubBuilder}, System.Action{WebPubSubOptionsBuilder})"/>
/// so a <see cref="WebPubSubServiceClient"/> is registered for it.
/// <para>
/// Unlike Azure Service Bus, Web PubSub does not persist messages. An application that is disconnected when an
/// expiration message is published never receives it and may serve stale cache entries until they expire normally.
/// </para>
/// </remarks>
public static class WebPubSubCacheExpireExtensions
{
    /// <summary>
    /// Registers the publisher behavior that expires the local cache and publishes a
    /// <see cref="CacheExpireMessage"/> to the configured Web PubSub group.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The optional service key used to resolve registered Web PubSub clients and options.</param>
    /// <param name="hubName">The name of the Web PubSub hub to publish cache expiration messages to.</param>
    /// <param name="groupName">The name of the Web PubSub group to publish cache expiration messages to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSubCacheExpirePublisher(
        this IServiceCollection services,
        object? serviceName,
        string hubName,
        string groupName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        AddCacheExpireOptions(services, serviceName, hubName, groupName);

        services.AddWebPubSubPublisher<CacheExpirePublisher>(serviceName, hubName);

        // applies to all commands implementing ICacheExpire, mirroring HybridCacheExpireBehavior registration
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(WebPubSubCacheExpireBehavior<,>));

        return services;
    }

    /// <summary>
    /// Registers the hosted subscriber processor that receives <see cref="CacheExpireMessage"/> messages and expires
    /// the matching entries from the local cache.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The optional service key used to resolve registered Web PubSub clients and options.</param>
    /// <param name="hubName">The name of the Web PubSub hub to receive cache expiration messages from.</param>
    /// <param name="groupName">The name of the Web PubSub group to receive cache expiration messages from.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSubCacheExpireSubscriber(
        this IServiceCollection services,
        object? serviceName,
        string hubName,
        string groupName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        AddCacheExpireOptions(services, serviceName, hubName, groupName);

        services.AddWebPubSubProcessor<CacheExpireProcessor>(serviceName, hubName);

        return services;
    }

    /// <summary>
    /// Registers both the publisher behavior and the subscriber processor for distributed cache expiration.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="serviceName">The optional service key used to resolve registered Web PubSub clients and options.</param>
    /// <param name="hubName">The name of the Web PubSub hub used for cache expiration messages.</param>
    /// <param name="groupName">The name of the Web PubSub group used for cache expiration messages.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddWebPubSubCacheExpire(
        this IServiceCollection services,
        object? serviceName,
        string hubName,
        string groupName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        AddWebPubSubCacheExpirePublisher(services, serviceName, hubName, groupName);
        AddWebPubSubCacheExpireSubscriber(services, serviceName, hubName, groupName);

        return services;
    }

    private static void AddCacheExpireOptions(
        IServiceCollection services,
        object? serviceName,
        string hubName,
        string groupName)
    {
        // single per-process options instance shared by the publisher behavior and subscriber processor;
        // the declared hub and group names are stable keys, the suffix formatting is applied by
        // the resolved WebPubSubOptions when publishing or joining

        CacheExpireOptions instance = new()
        {
            ServiceKey = serviceName,
            HubName = hubName,
            GroupName = groupName,
        };
        services.TryAddSingleton(instance);
    }
}
