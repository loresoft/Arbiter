using Arbiter.CommandQuery.Handlers;
using Arbiter.CommandQuery.Models;
using Arbiter.Health.Commands;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.Health.Handlers;

/// <summary>
/// Handles <see cref="HealthCheckCommand"/> requests by executing registered health checks and returning a health report.
/// </summary>
public class HealthCheckHandler : RequestHandlerBase<HealthCheckCommand, HealthReportModel>
{
    private readonly HealthCheckService _healthCheckService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthCheckHandler"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory used to create loggers for request processing.</param>
    /// <param name="healthCheckService">The health check service used to execute health checks.</param>
    public HealthCheckHandler(ILoggerFactory loggerFactory, HealthCheckService healthCheckService)
        : base(loggerFactory)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Executes the health check command and returns the aggregated health report.
    /// </summary>
    /// <param name="request">The health check command request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the health check operation.</param>
    /// <returns>
    /// A <see cref="HealthReportModel"/> containing health status details, or an unhealthy fallback report when execution fails.
    /// </returns>
    protected override async ValueTask<HealthReportModel?> Process(HealthCheckCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken).ConfigureAwait(false);

            return new HealthReportModel
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration,
                Entries = [.. healthReport.Entries.Select(e => ConvertEntry(e))],
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Health check failed: {Message}", ex.Message);

            return new HealthReportModel
            {
                Status = nameof(HealthStatus.Unhealthy),
            };
        }
    }

    private static HealthEntryModel ConvertEntry(KeyValuePair<string, HealthReportEntry> e)
    {
        return new HealthEntryModel
        {
            Key = e.Key,
            Description = e.Value.Description,
            Duration = e.Value.Duration,
            Message = e.Value.Exception?.Message,
            Exception = e.Value.Exception?.ToString(),
            Status = e.Value.Status.ToString(),
            Tags = e.Value.Tags?.ToList(),
        };
    }
}
