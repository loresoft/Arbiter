namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Represents a registered Azure Web PubSub configuration, used to enumerate configured instances and resolve their named options.
/// </summary>
/// <param name="ServiceKey">The dependency injection key used to register the Web PubSub clients.</param>
/// <param name="OptionsName">The named options instance used to resolve the configured <see cref="WebPubSubOptions" />.</param>
internal sealed record WebPubSubRegistration(
    object? ServiceKey,
    string OptionsName
);
