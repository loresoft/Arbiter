using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Arbiter.OpenTelemetry.Server;

/// <summary>
/// Extension methods for configuring OpenTelemetry in a host application builder.
/// </summary>
public static class HostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds and configures OpenTelemetry services including logging, metrics, and tracing.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configureOptions">Optional action to configure logging, build version, and ASP.NET Core tracing filters.</param>
    /// <param name="configureMetrics">Optional action to customize the metrics provider configuration.</param>
    /// <param name="configureTracing">Optional action to customize the tracing provider configuration.</param>
    /// <param name="configureOpenTelemetry">Optional action to customize the overall OpenTelemetry configuration.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <remarks>
    /// This extension method configures:
    /// <list type="bullet">
    /// <item>Logging with appropriate log levels and activity tracking options</item>
    /// <item>Log enrichment with stack traces and exception messages</item>
    /// <item>OpenTelemetry logging with formatted messages and scopes</item>
    /// <item>Application metadata (name, optional version, environment)</item>
    /// <item>Resource attributes for service identification</item>
    /// <item>Metrics for ASP.NET Core, HTTP, runtime, and SQL Client instrumentation</item>
    /// <item>Tracing for ASP.NET Core, HTTP, and SQL Client instrumentation, with optional request path filtering</item>
    /// <item>OTLP exporter if OTEL_EXPORTER_OTLP_ENDPOINT is configured</item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0051:Method is too long", Justification = "Configuration method is inherently long due to multiple setup steps")]
    public static IHostApplicationBuilder AddOpenTelemetry(
        this IHostApplicationBuilder builder,
        Action<OpenTelemetryServerOptions>? configureOptions = null,
        Action<MeterProviderBuilder>? configureMetrics = null,
        Action<TracerProviderBuilder>? configureTracing = null,
        Action<OpenTelemetryBuilder>? configureOpenTelemetry = null)
    {
        var serverOptions = new OpenTelemetryServerOptions();
        configureOptions?.Invoke(serverOptions);

        ConfigureLogging(builder.Logging, serverOptions);

        // add application metadata to both logs and telemetry, such as application name, build version, and environment name
        builder.Services
            .AddApplicationMetadata(metadataOptions =>
            {
                metadataOptions.ApplicationName = builder.Environment.ApplicationName;
                metadataOptions.BuildVersion = serverOptions.BuildVersion;
                metadataOptions.EnvironmentName = builder.Environment.EnvironmentName;
            })
            .AddApplicationLogEnricher(options =>
            {
                options.ApplicationName = true;
                options.BuildVersion = true;
                options.EnvironmentName = true;
            });

        var openTelemetryBuilder = builder.Services
            .AddOpenTelemetry();

        // configure resource attributes to include service name and version,
        // which will be used for both metrics and traces
        openTelemetryBuilder
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: builder.Environment.ApplicationName,
                    serviceVersion: serverOptions.BuildVersion
                )
                .AddEnvironmentVariableDetector()
            );

        openTelemetryBuilder
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddMeter("Arbiter.Mediation");

                // allow for additional custom configuration of metrics
                configureMetrics?.Invoke(metrics);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options => options.Filter = context => ShouldTraceRequest(context, serverOptions))
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddSource("Arbiter.Mediation", "Arbiter.Dispatcher");

                // allow for additional custom configuration of tracing
                configureTracing?.Invoke(tracing);
            });

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
            openTelemetryBuilder.UseOtlpExporter();

        // allow for additional custom configuration of OpenTelemetry
        configureOpenTelemetry?.Invoke(openTelemetryBuilder);

        return builder;
    }

    private static bool ShouldTraceRequest(HttpContext context, OpenTelemetryServerOptions options)
    {
        if (options.FilteredSegments.Count == 0)
            return true;

        return !options.FilteredSegments.Any(segment =>
            !string.IsNullOrWhiteSpace(segment) &&
            context.Request.Path.StartsWithSegments(segment, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static void ConfigureLogging(ILoggingBuilder logging, OpenTelemetryServerOptions options)
    {
        logging.SetMinimumLevel(options.MinimumLogLevel);

        foreach (var filter in options.LoggingFilters)
            logging.AddFilter(filter.Key, filter.Value);

        // configure activity tracking options to include trace and span identifiers, as well as tags and other relevant information
        logging
            .Configure(options =>
            {
                options.ActivityTrackingOptions =
                    ActivityTrackingOptions.TraceId |
                    ActivityTrackingOptions.SpanId |
                    ActivityTrackingOptions.ParentId |
                    ActivityTrackingOptions.TraceFlags |
                    ActivityTrackingOptions.TraceState |
                    ActivityTrackingOptions.Tags;
            });

        // enrich logs with additional context, such as stack traces and exception messages
        logging
            .EnableEnrichment(options =>
            {
                options.CaptureStackTraces = true;
                options.UseFileInfoForStackTraces = true;
                options.IncludeExceptionMessage = true;
            });

        // configure OpenTelemetry logging to include formatted messages, scopes, and state values
        logging
            .AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
            });
    }
}
