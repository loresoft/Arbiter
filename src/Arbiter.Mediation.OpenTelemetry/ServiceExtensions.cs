using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Arbiter.Mediation.OpenTelemetry;

/// <summary>
/// Provides extension methods for registering Arbiter.Mediation instrumentation with OpenTelemetry.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Registers the Arbiter.Mediation meter with the <see cref="MeterProviderBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="MeterProviderBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static MeterProviderBuilder AddMediatorInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddMeter(MediatorTelemetry.MeterName);
    }

    /// <summary>
    /// Registers the Arbiter.Mediation activity source with the <see cref="TracerProviderBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="TracerProviderBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static TracerProviderBuilder AddMediatorInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSource(MediatorTelemetry.SourceName);
    }
}
