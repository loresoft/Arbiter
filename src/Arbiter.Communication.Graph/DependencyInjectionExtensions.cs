using Arbiter.Communication.Email;

using Azure.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace Arbiter.Communication.Graph;


/// <summary>
/// Provides extension methods for registering Azure-based email and SMS delivery services with dependency injection.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers Azure Communication Services email delivery and related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGraphEmailDeliver(
        this IServiceCollection services,
        Action<EmailConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEmailServices(configureOptions);
        services.TryAddSingleton<IEmailDeliveryService, GraphEmailDeliverService>();

        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EmailConfiguration>>();
            var config = options.Value;

            var credential = new ClientSecretCredential(
                tenantId: config.TenantId,
                clientId: config.ClientId,
                clientSecret: config.ServiceKey ?? config.Password);

            return new GraphServiceClient(credential);
        });

        return services;
    }
}
