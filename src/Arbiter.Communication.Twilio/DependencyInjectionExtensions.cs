// Ignore Spelling: Twilio

using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using SendGrid.Extensions.DependencyInjection;

namespace Arbiter.Communication.Twilio;

/// <summary>
/// Provides extension methods for registering SendGrid email and Twilio SMS delivery services with dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers SendGrid email delivery and related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options, such as sender information and API key.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSendGridEmailDeliver(
        this IServiceCollection services,
        Action<EmailConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEmailServices(configureOptions);
        services.TryAddSingleton<IEmailDeliveryService, SendGridEmailDeliveryService>();

        services.AddSendGrid((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IOptions<EmailConfiguration>>();
            options.ApiKey = configuration.Value.ServiceKey;
        });

        return services;
    }

    /// <summary>
    /// Registers SendGrid email delivery and related services in the dependency injection container using a direct API key.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="serviceKey">The SendGrid API key to use for authentication.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSendGridEmailDeliver(
        this IServiceCollection services,
        string serviceKey)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(serviceKey);

        return services.AddSendGridEmailDeliver(options => options.ServiceKey = serviceKey);
    }

    /// <summary>
    /// Registers Twilio SMS delivery and related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="SmsConfiguration"/> options, such as sender number and credentials.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTwilioSmsDeliver(
        this IServiceCollection services,
        Action<SmsConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSmsServices(configureOptions);
        services.TryAddSingleton<ISmsDeliveryService, TwilioSmsDeliveryService>();

        return services;
    }

    /// <summary>
    /// Registers Twilio SMS delivery and related services in the dependency injection container using direct credentials.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="accountSID">The Twilio Account SID used for authentication.</param>
    /// <param name="authenticationToken">The Twilio Auth Token used for authentication.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTwilioSmsDeliver(
        this IServiceCollection services,
        string accountSID,
        string authenticationToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(accountSID);
        ArgumentException.ThrowIfNullOrEmpty(authenticationToken);

        return services.AddTwilioSmsDeliver(options => {
            options.UserName = accountSID;
            options.Password = authenticationToken;
        });
    }
}
