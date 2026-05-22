using Arbiter.Mediation;
using Arbiter.OpenTelemetry.Models;
using Arbiter.OpenTelemetry.Monitor.Handlers;
using Arbiter.OpenTelemetry.Queries;

using Azure.Identity;
using Azure.Monitor.Query;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.OpenTelemetry.Monitor;

/// <summary>
/// Provides dependency injection extensions for Azure Monitor query services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Azure Monitor logs query client and log record query handler.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLogQuery(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(_ => new LogsQueryClient(new DefaultAzureCredential()));
        services.TryAddSingleton<IRequestHandler<LogRecordQuery, LogRecordResult>, LogRecordQueryHandler>();

        return services;
    }
}
