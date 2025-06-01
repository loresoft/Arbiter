using Arbiter.Communication.Email;
using Arbiter.Communication.Sms;

using Azure.Communication.Email;
using Azure.Communication.Sms;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Communication.Azure;

/// <summary>
/// Provides extension methods for registering Azure-based email and SMS delivery services with dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers Azure Communication Services email delivery and related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="nameOrConnectionString">
    /// The connection string for Azure Communication Services, or the name of a configuration entry or connection string.
    /// </param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAzureEmailDeliver(
        this IServiceCollection services,
        string nameOrConnectionString,
        Action<EmailConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEmailServices(configureOptions);
        services.TryAddSingleton<IEmailDeliveryService, AzureEmailDeliveryService>();

        services.TryAddSingleton(sp =>
        {
            var connectionString = ResolveConnectionString(sp, nameOrConnectionString);
            return new EmailClient(connectionString);
        });

        return services;
    }

    /// <summary>
    /// Registers Azure Communication Services SMS delivery and related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="nameOrConnectionString">
    /// The connection string for Azure Communication Services, or the name of a configuration entry or connection string.
    /// </param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="SmsConfiguration"/> options.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddAzureSmsDeliver(
        this IServiceCollection services,
        string nameOrConnectionString,
        Action<SmsConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSmsServices(configureOptions);
        services.TryAddSingleton<ISmsDeliveryService, AzureSmsDeliveryService>();

        services.TryAddSingleton(sp =>
        {
            var connectionString = ResolveConnectionString(sp, nameOrConnectionString);
            return new SmsClient(connectionString);
        });

        return services;
    }

    /// <summary>
    /// Resolves a connection string from configuration or returns the provided value if it is already a connection string.
    /// </summary>
    /// <param name="services">The <see cref="IServiceProvider"/> used to resolve configuration services.</param>
    /// <param name="nameOrConnectionString">
    /// The name of a connection string or configuration entry, or the connection string itself.
    /// </param>
    /// <returns>The resolved connection string.</returns>
    public static string ResolveConnectionString(this IServiceProvider services, string nameOrConnectionString)
    {
        var isConnectionString = nameOrConnectionString.IndexOfAny([';', '=', ':', '/']) > 0;
        if (isConnectionString)
            return nameOrConnectionString;

        var configuration = services.GetRequiredService<IConfiguration>();
        configuration.Bind(configuration); // Ensure configuration is bound

        // first try connection strings section
        var connectionString = configuration.GetConnectionString(nameOrConnectionString);

        if (!string.IsNullOrEmpty(connectionString))
            return connectionString;

        // next try root collection
        connectionString = configuration[nameOrConnectionString];
        if (!string.IsNullOrEmpty(connectionString))
            return connectionString;

        return nameOrConnectionString;
    }
}
