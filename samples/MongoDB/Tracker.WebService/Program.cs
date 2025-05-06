
using Arbiter.CommandQuery.Endpoints;

using AspNetCore.SecurityKey;

using Microsoft.AspNetCore.ResponseCompression;

using Scalar.AspNetCore;

using Serilog;
using Serilog.Events;

using Tracker.WebService.Services;

namespace Tracker.WebService;

public static class Program
{
    private const string OutputTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u1}] {Message:lj}{NewLine}{Exception}";

    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: OutputTemplate)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting web host");

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
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        string logDirectory = GetLoggingPath();

        builder.Host
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Debug)
                .Enrich.FromLogContext()
                .Filter.ByExcluding(logEvent => logEvent.Exception is OperationCanceledException)
                .WriteTo.Console(outputTemplate: OutputTemplate)
                .WriteTo.File(
                    path: $"{logDirectory}/log.txt",
                    outputTemplate: OutputTemplate,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 10
                )
            );
        ;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.Services
            .AddAuthentication()
            .AddSecurityKey();

        builder.Services
            .AddAuthorization();

        builder.Services
            .AddSecurityKey();

        services
            .AddProblemDetails()
            .AddHttpContextAccessor();

        services
            .AddTrackerWebService();

        services
            .AddMemoryCache();

        services
            .AddOpenApi(options => options
                .AddDocumentTransformer<SecurityKeyDocumentTransformer>()
            );

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
            });
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseResponseCompression();
        }

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        app.UseHttpsRedirection();

        app.MapOpenApi();
        app.MapScalarApiReference();

        app.MapFeatureEndpoints().RequireAuthorization();
    }

    private static string GetLoggingPath()
    {
        // azure home directory
        var homeDirectory = Environment.GetEnvironmentVariable("HOME") ?? ".";
        var logDirectory = Path.Combine(homeDirectory, "LogFiles");

        return Path.GetFullPath(logDirectory);
    }
}
