
using Arbiter.CommandQuery;
using Arbiter.CommandQuery.Endpoints;
using Arbiter.OpenTelemetry.Server;

using AspNetCore.SecurityKey;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

using Scalar.AspNetCore;

using Tracker.Extensions;

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

    private static void ConfigureLogging(IHostApplicationBuilder builder)
    {
        builder.AddOpenTelemetry(options => options.BuildVersion = ThisAssembly.FileVersion);
    }

    private static void ConfigureServices(IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services
            .AddAuthentication()
            .AddSecurityKey();

        services
            .AddAuthorization();

        services
           .AddProblemDetails()
           .AddValidation();

        services
            .AddTrackerShared();

        builder
            .AddTrackerCore()
            .AddTrackerService();

        services
            .AddOpenApi();

        services
            .AddEndpointRoutes()
            .AddDiagnosticRoutes(options =>
            {
                // sample application opts in to the sensitive diagnostics routes
                options.ConfigurationEnabled = true;
                options.CacheClearEnabled = true;
            });

        services
            .ConfigureHttpJsonOptions(options => options.SerializerOptions.AddDomainOptions())
            .AddSingleton(sp => sp.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions);

        services
            .AddResponseCompression(options => options.EnableForHttps = true);

        services.AddEnvironmentOptions();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseResponseCompression();
        }

        // don't require authentication for static files
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRequestLogging(config => config.IncludeRequestBody = true);
                
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseClaimLogging();

        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();

        app.MapEndpointRoutes().RequireAuthorization();

        HealthCheckOptions options = new() { Predicate = _ => false };

        app.MapHealthChecks("/livez", options).AllowAnonymous();
        app.MapHealthChecks("/healthz").AllowAnonymous();
    }
}
