
using Arbiter.CommandQuery.Endpoints;
using Arbiter.OpenTelemetry.Server;

using AspNetCore.SecurityKey;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

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

    private static void ConfigureLogging(IHostApplicationBuilder builder)
    {
        builder.AddOpenTelemetry(options => options.BuildVersion = ThisAssembly.FileVersion);
    }

    private static void ConfigureServices(IHostApplicationBuilder builder)
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
            .AddResponseCompression(options => options.EnableForHttps = true);

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
