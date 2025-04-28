using Arbiter.CommandQuery.Endpoints;
using Arbiter.Mediation.OpenTelemetry;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

using Tracker.Extensions;
using Tracker.Options;
using Tracker.Web.Components;

namespace Tracker;

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
            .AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

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
            .AddFeatureEndpoints();

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

        services.Configure<EnvironmentOptions>(options =>
        {
            options.EnvironmentName = builder.Environment.EnvironmentName;
        });

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

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client.Routes).Assembly);

        app.MapFeatureEndpoints();

    }
}
