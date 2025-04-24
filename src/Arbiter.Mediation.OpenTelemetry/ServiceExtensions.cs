using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Arbiter.Mediation.OpenTelemetry;

/// <summary>
/// Provides extension methods for configuring OpenTelemetry diagnostics for the mediator.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Adds the meters for OpenTelemetry to the specified <see cref="MeterProviderBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="MeterProviderBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static MeterProviderBuilder AddMediatorInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddMeter(MediatorDiagnostic.MeterName);

        return builder;
    }

    /// <summary>
    /// Adds the trace source for OpenTelemetry to the specified <see cref="TracerProviderBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="TracerProviderBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TracerProviderBuilder AddMediatorInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSource(MediatorDiagnostic.ActivitySourceName);

        return builder;
    }

    /// <summary>
    /// Adds the diagnostic services to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddMediatorDiagnostics(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IMediatorDiagnostic, MediatorDiagnostic>();

        return services;
    }
}
