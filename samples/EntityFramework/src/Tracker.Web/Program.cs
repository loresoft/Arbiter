using Arbiter.CommandQuery.Endpoints;
using Arbiter.Dispatcher;
using Arbiter.Dispatcher.Server;
using Arbiter.Mediation;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Tracker.Extensions;
using Tracker.Options;
using Tracker.Web.Components;

namespace Tracker.Web;

public static class Program
{
    public static int Main(string[] args)
    {

        try
        {
            var builder = WebApplication.CreateBuilder(args);

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
                    .AddSource(DispatcherTelemetry.SourceName)
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
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

        services
            .AddCascadingAuthenticationState();

        // authentication
        services
            .AddMicrosoftIdentityWebAppAuthentication(configuration);

        services
            .AddAuthorization(options => options.FallbackPolicy = options.DefaultPolicy)
            .AddAntiforgery();

        services
            .AddHttpContextAccessor();

        services
            .AddTrackerShared()
            .AddTrackerCore()
            .AddTrackerClient("Server")
            .AddTrackerWeb();

        services
            .AddEndpointRoutes()
            .AddDispatcherService();

        services
            .ConfigureHttpJsonOptions(options => options.SerializerOptions.AddDomainOptions());

        services
            .AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["image/svg+xml"]);
            });

        services
            .AddStackExchangeRedisCache(options => options.Configuration = builder.Configuration.GetConnectionString("RedisConnection"));

        services
            .Configure<EnvironmentOptions>(options => options.EnvironmentName = builder.Environment.EnvironmentName);

        // change authentication cookie name
        services
            .Configure<CookieAuthenticationOptions>(
                name: CookieAuthenticationDefaults.AuthenticationScheme,
                configureOptions: options => options.Cookie.Name = ".Tracker.Authentication"
            );

    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseHsts();
            app.UseExceptionHandler("/Error");
            app.UseResponseCompression();
        }

        app.UseRequestLogging(config =>
        {
            config.IncludeRequestBody = true;
            config.IgnorePath("/_framework/**");
            config.IgnorePath("/_content/**");
        });

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client.Routes).Assembly);

        app.MapEndpointRoutes();

        app.MapDispatcherService().RequireAuthorization();

    }
}
