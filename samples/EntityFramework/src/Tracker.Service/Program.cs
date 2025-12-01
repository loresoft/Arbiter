
using Arbiter.CommandQuery.Endpoints;
using Arbiter.Mediation.OpenTelemetry;

using AspNetCore.SecurityKey;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using Scalar.AspNetCore;

using Tracker.Extensions;
using Tracker.Options;

namespace Tracker.Service;

public static class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        ConfigureLogging(builder);
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigureMiddleware(app);

        app.Run();
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddMediatorInstrumentation()
            )
            .WithTracing(tracing =>
                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddMediatorInstrumentation()
            );

        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
        {
            builder.Services
                .AddOpenTelemetry()
                .UseOtlpExporter();
        }

        builder.Services.AddMediatorDiagnostics();
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
