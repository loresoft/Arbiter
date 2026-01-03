using MessagePack;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Dispatcher.Server;

public static class DispatcherServiceExtensions
{
    public static IServiceCollection AddDispatcherService(this IServiceCollection services)
    {
        services.AddGrpc();

        // Register the DispatcherService itself
        services.TryAddSingleton<DispatcherService>();

        // MessagePack Serializer Options Registration
        services.TryAddSingleton(MessagePackSerializerOptions.Standard
            .WithResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance)
            .WithCompression(MessagePackCompression.Lz4BlockArray));

        return services;
    }

    public static IApplicationBuilder UseDispatchService(this IApplicationBuilder app)
    {
        return app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
    }

    public static IEndpointConventionBuilder MapDispatchService(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapGrpcService<DispatcherService>()
            .EnableGrpcWeb();
    }
}
