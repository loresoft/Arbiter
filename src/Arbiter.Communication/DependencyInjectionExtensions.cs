using System.Reflection;

using Arbiter.Communication.Email;
using Arbiter.Communication.Extensions;
using Arbiter.Communication.Sms;
using Arbiter.Communication.Template;

using Fluid;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Communication;

/// <summary>
/// Provides extension methods for registering communication, email, and SMS services with the dependency injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers template services, including <see cref="FluidParser"/> and <see cref="ITemplateService"/>, as singletons.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddTemplateServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<FluidParser>();
        services.TryAddSingleton<ITemplateService, TemplateService>();

        return services;
    }

    /// <summary>
    /// Registers a custom template resolver implementation as a singleton.
    /// </summary>
    /// <typeparam name="TService">The type of the template resolver to register. Must implement <see cref="ITemplateResolver"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTemplateResolver<TService>(this IServiceCollection services)
        where TService : class, ITemplateResolver
        => services.AddSingleton<ITemplateResolver, TService>();

    /// <summary>
    /// Registers a template resource resolver for the assembly of the specified type, using the provided resource name format and priority.
    /// </summary>
    /// <typeparam name="T">A type from the assembly containing the embedded resources.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="resourceNameFormat">The format string for resource names, e.g., <c>"Namespace.Templates.{0}.yaml"</c>.</param>
    /// <param name="priority">The resolver priority. Lower values are processed first. Defaults to 9999.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTemplateResourceResolver<T>(this IServiceCollection services, string resourceNameFormat, int priority = 9999)
        => services.AddTemplateResourceResolver(typeof(T).Assembly, resourceNameFormat, priority);

    /// <summary>
    /// Registers a template resource resolver for the specified assembly, using the provided resource name format and priority.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="assembly">The assembly containing the embedded resource templates.</param>
    /// <param name="resourceNameFormat">The format string for resource names, e.g., <c>"Namespace.Templates.{0}.yaml"</c>.</param>
    /// <param name="priority">The resolver priority. Lower values are processed first. Defaults to 9999.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddTemplateResourceResolver(this IServiceCollection services, Assembly assembly, string resourceNameFormat, int priority = 9999)
        => services.AddSingleton<ITemplateResolver>(_ => new TemplateResourceResolver(assembly, resourceNameFormat, priority));

    /// <summary>
    /// Registers core email services, including template and email template services, with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options, such as sender information, SMTP server, and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers <see cref="FluidParser"/>, <see cref="ITemplateService"/>, and <see cref="IEmailTemplateService"/> as singletons.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        Action<EmailConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddOptions<EmailConfiguration>()
            .BindConfiguration(EmailConfiguration.ConfigurationName);

        if (configureOptions != null)
            services.PostConfigure(configureOptions);

        services.AddTemplateServices();
        services.TryAddSingleton<IEmailTemplateService, EmailTemplateService>();

        return services;
    }

    /// <summary>
    /// Registers a custom email delivery service implementation with the dependency injection container.
    /// </summary>
    /// <typeparam name="TService">The type of the email delivery service to register. Must implement <see cref="IEmailDeliveryService"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options, such as sender information, SMTP server, and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers the specified <typeparamref name="TService"/> as a singleton implementation of <see cref="IEmailDeliveryService"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddEmailDelivery<TService>(
        this IServiceCollection services,
        Action<EmailConfiguration>? configureOptions = null)
        where TService : class, IEmailDeliveryService
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEmailServices(configureOptions);
        services.TryAddSingleton<IEmailDeliveryService, TService>();

        return services;
    }

    /// <summary>
    /// Registers a custom email delivery service implementation using a factory with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="factory">A factory function to create the <see cref="IEmailDeliveryService"/> instance.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options, such as sender information, SMTP server, and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="factory"/> is <c>null</c>.</exception>
    public static IServiceCollection AddEmailDelivery(
        this IServiceCollection services,
        Func<IServiceProvider, IEmailDeliveryService> factory,
        Action<EmailConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);

        services.AddEmailServices(configureOptions);
        services.TryAddSingleton(factory);

        return services;
    }

    /// <summary>
    /// Registers the default SMTP email delivery service with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="EmailConfiguration"/> options, such as sender information, SMTP server, and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers <see cref="SmtpEmailDeliveryService"/> as a singleton implementation of <see cref="IEmailDeliveryService"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddSmtpEmailDeliver(
        this IServiceCollection services,
        Action<EmailConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEmailServices(configureOptions);
        services.TryAddSingleton<IEmailDeliveryService, SmtpEmailDeliveryService>();

        return services;
    }

    /// <summary>
    /// Registers core SMS services, including template services, with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="SmsConfiguration"/> options, such as sender information and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddSmsServices(
        this IServiceCollection services,
        Action<SmsConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddOptions<SmsConfiguration>()
            .BindConfiguration(SmsConfiguration.ConfigurationName);

        if (configureOptions != null)
            services.PostConfigure(configureOptions);

        services.AddTemplateServices();
        services.TryAddSingleton<ISmsTemplateService, SmsTemplateService>();

        return services;
    }

    /// <summary>
    /// Registers a custom SMS delivery service implementation with the dependency injection container.
    /// </summary>
    /// <typeparam name="TService">The type of the SMS delivery service to register. Must implement <see cref="ISmsDeliveryService"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="SmsConfiguration"/> options, such as sender information and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers the specified <typeparamref name="TService"/> as a singleton implementation of <see cref="ISmsDeliveryService"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddSmsDelivery<TService>(
        this IServiceCollection services,
        Action<SmsConfiguration>? configureOptions = null)
        where TService : class, ISmsDeliveryService
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSmsServices(configureOptions);
        services.TryAddSingleton<ISmsDeliveryService, TService>();

        return services;
    }

    /// <summary>
    /// Registers a custom SMS delivery service implementation using a factory with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="factory">A factory function to create the <see cref="ISmsDeliveryService"/> instance.</param>
    /// <param name="configureOptions">
    /// An optional delegate to configure <see cref="SmsConfiguration"/> options, such as sender information and template resources.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddSmsDeliver(
        this IServiceCollection services,
        Func<IServiceProvider, ISmsDeliveryService> factory,
        Action<SmsConfiguration>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSmsServices(configureOptions);
        services.TryAddSingleton(factory);

        return services;
    }
}
