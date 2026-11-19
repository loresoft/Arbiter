using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arbiter.CommandQuery.EntityFramework.Health;

/// <summary>
/// An <see cref="IHealthCheck"/> implementation that reports the health of a <see cref="DbContext"/> by
/// verifying connectivity, optionally using a custom query.
/// </summary>
/// <typeparam name="TContext">The type of the <see cref="DbContext"/> being checked.</typeparam>
public sealed class DbContextHealthCheck<TContext> : IHealthCheck
    where TContext : DbContext
{
    private static readonly Func<TContext, CancellationToken, Task<bool>> DefaultQuery = (dbContext, cancellationToken) => dbContext.Database.CanConnectAsync(cancellationToken);

    private readonly ILogger<DbContextHealthCheck<TContext>> _logger;
    private readonly TContext _dataContext;
    private readonly Func<TContext, CancellationToken, Task<bool>> _query;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextHealthCheck{TContext}"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record health check errors.</param>
    /// <param name="dataContext">The <typeparamref name="TContext"/> instance to check.</param>
    /// <param name="query">An optional delegate used to determine whether the <typeparamref name="TContext"/> is healthy.</param>
    public DbContextHealthCheck(
        ILogger<DbContextHealthCheck<TContext>> logger,
        TContext dataContext,
        Func<TContext, CancellationToken, Task<bool>>? query = null)
    {
        _logger = logger;
        _dataContext = dataContext;
        _query = query ?? DefaultQuery;
    }


    /// <summary>
    /// Runs the health check, returning the status of the <typeparamref name="TContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="HealthCheckContext"/> for the current health check execution.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the health check.</param>
    /// <returns>A <see cref="HealthCheckResult"/> describing the health of the <typeparamref name="TContext"/>.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        string? serverName = null;
        string? databaseName = null;

        try
        {
            var database = _dataContext.Database;

            // connection strings are only available for relational providers
            var connectionString = database.IsRelational()
                ? database.GetConnectionString()
                : null;

            (serverName, databaseName) = ParseConnectionDetails(connectionString);

            var result = await _query(_dataContext, cancellationToken).ConfigureAwait(false);

            if (result)
            {
                var message = BuildHealthMessage("Connected to", serverName, databaseName);
                return HealthCheckResult.Healthy(message);
            }

            var failure = BuildHealthMessage("Failed to connect to", serverName, databaseName);
            return new HealthCheckResult(context.Registration.FailureStatus, failure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking SQL health for server '{ServerName}' and database '{DatabaseName}': {Message}",
                serverName, databaseName, ex.Message);

            var description = serverName is null || databaseName is null
                ? "SQL health check failed."
                : BuildHealthMessage("Failed to connect to", serverName, databaseName);

            return new HealthCheckResult(context.Registration.FailureStatus, description, ex);
        }
    }


    /// <summary>
    /// Parses the server name and database name from a connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to parse, or <see langword="null"/> if unavailable (e.g. non-relational providers).</param>
    /// <returns>
    /// A tuple containing the <c>ServerName</c> and <c>DatabaseName</c> extracted from the connection string,
    /// or <c>"unknown"</c> for either value when the connection string is <see langword="null"/>, empty, or malformed.
    /// </returns>
    private static (string ServerName, string DatabaseName) ParseConnectionDetails(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return ("unknown", "unknown");

        var builder = new DbConnectionStringBuilder();

        try
        {
            builder.ConnectionString = connectionString;
        }
        catch (ArgumentException)
        {
            return ("unknown", "unknown");
        }

        return
        (
            ServerName: GetConnectionStringValue(builder, "Server", "Data Source", "Address", "Host", "Network Address"),
            DatabaseName: GetConnectionStringValue(builder, "Database", "Initial Catalog")
        );
    }

    /// <summary>
    /// Retrieves the first non-empty value from a <see cref="DbConnectionStringBuilder"/> matching any of the specified keys.
    /// </summary>
    /// <param name="builder">The connection string builder to query.</param>
    /// <param name="keys">An ordered list of candidate keys to look up.</param>
    /// <returns>The first matching value, or <c>"unknown"</c> if none is found.</returns>
    private static string GetConnectionStringValue(DbConnectionStringBuilder builder, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!builder.TryGetValue(key, out var value) || value is null)
                continue;

            var text = value.ToString();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }

        return "unknown";
    }

    /// <summary>
    /// Builds a human-readable health check message describing the target server and database.
    /// </summary>
    /// <param name="prefix">A verb phrase to begin the message (e.g., <c>"Connected to"</c> or <c>"Failed to connect to"</c>).</param>
    /// <param name="serverName">The name of the server.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <returns>A formatted message string.</returns>
    private static string BuildHealthMessage(string prefix, string? serverName, string? databaseName)
    {
        return $"{prefix} server '{serverName}' and database '{databaseName}'.";
    }
}
