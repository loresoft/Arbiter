using Arbiter.CommandQuery.EntityFramework.Health;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework;

/// <summary>
/// Extension methods for registering <see cref="DbContextHealthCheck{TContext}"/> instances with an <see cref="IHealthChecksBuilder"/>.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Registers a health check that reports the health of a <typeparamref name="TContext"/> by verifying connectivity.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/> to check.</typeparam>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the health check to.</param>
    /// <param name="name">
    /// The name of the health check. When <see langword="null"/>, defaults to the name of <typeparamref name="TContext"/>.
    /// </param>
    /// <param name="query">
    /// An optional delegate used to determine whether the <typeparamref name="TContext"/> is healthy.
    /// When <see langword="null"/>, defaults to verifying the database connection can be opened.
    /// </param>
    /// <returns>The <see cref="IHealthChecksBuilder"/> so that additional calls can be chained.</returns>
    public static IHealthChecksBuilder AddDataContext<TContext>(
        this IHealthChecksBuilder builder,
        string? name = null,
        Func<TContext, CancellationToken, Task<bool>>? query = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);

        name ??= typeof(TContext).Name;

        var healthCheckRegistration = new HealthCheckRegistration(
            name: "DataContext: " + name,
            factory: sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DbContextHealthCheck<TContext>>>();
                var dataContext = sp.GetRequiredService<TContext>();

                return new DbContextHealthCheck<TContext>(logger, dataContext, query);
            },
            failureStatus: HealthStatus.Unhealthy,
            tags: ["SQL"]
        );

        builder.Add(healthCheckRegistration);

        return builder;
    }
}
