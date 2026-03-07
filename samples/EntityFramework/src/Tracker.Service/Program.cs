
using Arbiter.CommandQuery.Endpoints;
using Arbiter.Mediation;

using AspNetCore.SecurityKey;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Scalar.AspNetCore;

using Tracker.Extensions;
using Tracker.Options;

namespace Tracker.Service;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);
            ConfigureServices(builder);

            var app = builder.Build();
            ConfigureMiddleware(app);

            app.Run();

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Host terminated unexpectedly: {ex.Message}");
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        // Per-category filters
        builder.Logging
            .AddFilter("Microsoft", LogLevel.Information)
            .AddFilter("Microsoft.AspNetCore", LogLevel.Warning)
            .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otelBuilder = builder.Services.AddOpenTelemetry();

        otelBuilder
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: builder.Environment.ApplicationName,
                    serviceVersion: ThisAssembly.FileVersion
                )
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
                ])
            );

        otelBuilder
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddMeter(MediatorTelemetry.MeterName)
            )
            .WithTracing(tracing =>
                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddSource(MediatorTelemetry.SourceName)
            );

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
            otelBuilder.UseOtlpExporter();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var connectionString = configuration.GetConnectionString("Tracker");

        services
            .AddAuthentication()
            .AddSecurityKey();

        services
            .AddAuthorization();

        services
           .AddProblemDetails();

        services
            .AddTrackerShared()
            .AddTrackerCore()
            .AddTrackerService();

        services
            .AddOpenApi(options => options.AddDocumentTransformer<SecurityKeyDocumentTransformer>());

        services
            .AddEndpointRoutes();

        services
            .AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration.GetConnectionString("RedisConnection"));

        services
            .ConfigureHttpJsonOptions(options => options.SerializerOptions.AddDomainOptions())
            .AddSingleton(sp => sp.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions);

        services
            .AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

        services.Configure<EnvironmentOptions>(options => options.EnvironmentName = builder.Environment.EnvironmentName);
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseResponseCompression();
        }

        app.UseRequestLogging(config => config.IncludeRequestBody = true);

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.MapEndpointRoutes().RequireAuthorization();
    }

}
