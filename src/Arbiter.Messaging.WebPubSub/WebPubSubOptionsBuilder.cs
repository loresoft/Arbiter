namespace Arbiter.Messaging.WebPubSub;

/// <summary>
/// Provides a builder for configuring Azure Web PubSub runtime options using values resolved from the
/// <see cref="IServiceProvider" />, such as the current environment name.
/// </summary>
public sealed class WebPubSubOptionsBuilder
{
    private readonly WebPubSubOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebPubSubOptionsBuilder" /> class.
    /// </summary>
    /// <param name="options">The options instance the builder configures.</param>
    /// <param name="services">The service provider used to resolve runtime configuration values.</param>
    public WebPubSubOptionsBuilder(WebPubSubOptions options, IServiceProvider services)
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
    /// Sets the suffix appended to hub names when formatting hub names.
    /// </summary>
    /// <param name="nameSuffix">The name suffix to append.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    public WebPubSubOptionsBuilder WithHubSuffix(string? nameSuffix)
    {
        if (nameSuffix != null)
            _options.WithHubSuffix(nameSuffix.ToLowerInvariant());

        return this;
    }

    /// <summary>
    /// Sets the suffix appended to hub names using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the name suffix.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public WebPubSubOptionsBuilder WithHubSuffix(Func<IServiceProvider, string?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var nameSuffix = factory(Services);

        if (nameSuffix != null)
            _options.WithHubSuffix(nameSuffix.ToLowerInvariant());

        return this;
    }

    /// <summary>
    /// Sets the suffix appended to group names when formatting group names.
    /// </summary>
    /// <param name="groupSuffix">The group name suffix to append.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    public WebPubSubOptionsBuilder WithGroupSuffix(string? groupSuffix)
    {
        if (groupSuffix != null)
            _options.WithGroupSuffix(groupSuffix.ToLowerInvariant());

        return this;
    }

    /// <summary>
    /// Sets the suffix appended to group names using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the group name suffix.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public WebPubSubOptionsBuilder WithGroupSuffix(Func<IServiceProvider, string?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var groupSuffix = factory(Services);

        if (groupSuffix != null)
            _options.WithGroupSuffix(groupSuffix.ToLowerInvariant());

        return this;
    }

    /// <summary>
    /// Sets the lifetime of generated client access tokens.
    /// </summary>
    /// <param name="expiration">The token lifetime. Must be greater than zero.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    public WebPubSubOptionsBuilder WithClientAccessTokenExpiration(TimeSpan expiration)
    {
        _options.WithClientAccessTokenExpiration(expiration);
        return this;
    }

    /// <summary>
    /// Sets the lifetime of generated client access tokens using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve the token lifetime.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public WebPubSubOptionsBuilder WithClientAccessTokenExpiration(Func<IServiceProvider, TimeSpan> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var expiration = factory(Services);
        _options.WithClientAccessTokenExpiration(expiration);
        return this;
    }

    /// <summary>
    /// Sets whether listener clients automatically reconnect when a connection drops.
    /// </summary>
    /// <param name="autoReconnect">A value indicating whether automatic reconnect is enabled.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    public WebPubSubOptionsBuilder WithAutoReconnect(bool autoReconnect = true)
    {
        _options.WithAutoReconnect(autoReconnect);
        return this;
    }

    /// <summary>
    /// Sets whether listener clients automatically reconnect using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether automatic reconnect is enabled.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public WebPubSubOptionsBuilder WithAutoReconnect(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var autoReconnect = factory(Services);
        _options.WithAutoReconnect(autoReconnect);
        return this;
    }

    /// <summary>
    /// Sets whether listener clients automatically rejoin their groups after reconnecting.
    /// </summary>
    /// <param name="autoRejoinGroups">A value indicating whether automatic group rejoin is enabled.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    public WebPubSubOptionsBuilder WithAutoRejoinGroups(bool autoRejoinGroups = true)
    {
        _options.WithAutoRejoinGroups(autoRejoinGroups);
        return this;
    }

    /// <summary>
    /// Sets whether listener clients automatically rejoin their groups using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether automatic group rejoin is enabled.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public WebPubSubOptionsBuilder WithAutoRejoinGroups(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var autoRejoinGroups = factory(Services);
        _options.WithAutoRejoinGroups(autoRejoinGroups);
        return this;
    }

    /// <summary>
    /// Sets whether listener clients use the reliable JSON subprotocol.
    /// </summary>
    /// <param name="useReliableProtocol">A value indicating whether the reliable protocol is used.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    public WebPubSubOptionsBuilder WithReliableProtocol(bool useReliableProtocol = true)
    {
        _options.WithReliableProtocol(useReliableProtocol);
        return this;
    }

    /// <summary>
    /// Sets whether listener clients use the reliable JSON subprotocol using a value resolved from the <see cref="Services" /> provider.
    /// </summary>
    /// <param name="factory">The factory used to resolve whether the reliable protocol is used.</param>
    /// <returns>The current <see cref="WebPubSubOptionsBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is <see langword="null" />.</exception>
    public WebPubSubOptionsBuilder WithReliableProtocol(Func<IServiceProvider, bool> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var useReliableProtocol = factory(Services);
        _options.WithReliableProtocol(useReliableProtocol);
        return this;
    }
}
