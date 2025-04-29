using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Mediation;

/// <summary>
/// Provides extension methods for registering mediation services in the service collection.
/// </summary>
public static class MediatorServiceExtensions
{
    /// <summary>
    /// Adds <see cref="IMediator"/> services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the <see cref="IMediator"/> services to.</param>
    /// <param name="serviceLifetime">The lifetime of the <see cref="IMediator"/> service. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers the <see cref="IMediator"/> interface with the specified lifetime in the service collection.
    /// The default lifetime is <see cref="ServiceLifetime.Singleton"/>.
    /// </remarks>
    /// <example>
    /// The following example demonstrates how to register the mediator services:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.Services.AddMediator(ServiceLifetime.Singleton);
    /// var app = builder.Build();
    /// app.Run();
    /// </code>
    /// </example>
    public static IServiceCollection AddMediator(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    {
        var service = ServiceDescriptor.Describe(typeof(IMediator), typeof(Mediator), serviceLifetime);
        services.TryAdd(service);

        return services;
    }
}
