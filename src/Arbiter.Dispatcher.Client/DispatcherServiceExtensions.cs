using System.Threading.Channels;

using Arbiter.Dispatcher.Client;
using Arbiter.Dispatcher.State;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Arbiter.Dispatcher;

public static class DispatcherServiceExtensions
{
    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services, string serviceAddress)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(serviceAddress);

        return services.AddRemoteDispatcher(_ =>
        {
            var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
            var channelOptions = new GrpcChannelOptions()
            {
                HttpHandler = httpHandler,
                MaxReceiveMessageSize = 10 * 1024 * 1024, // 10 MB
                MaxSendMessageSize = 10 * 1024 * 1024, // 10 MB
            };

            return GrpcChannel.ForAddress(serviceAddress, channelOptions);
        });
    }

    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services, Func<IServiceProvider, string> addressFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(addressFactory);

        return services.AddRemoteDispatcher(sp =>
        {
            var serviceAddress = addressFactory(sp);

            var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());
            var channelOptions = new GrpcChannelOptions()
            {
                HttpHandler = httpHandler,
                MaxReceiveMessageSize = 10 * 1024 * 1024, // 10 MB
                MaxSendMessageSize = 10 * 1024 * 1024, // 10 MB
            };

            return GrpcChannel.ForAddress(serviceAddress, channelOptions);
        });
    }

    public static IServiceCollection AddRemoteDispatcher(this IServiceCollection services, Func<IServiceProvider, GrpcChannel> channelFactory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(channelFactory);

        // Remote Dispatcher Registration
        services.TryAddTransient(sp =>
        {
            var channel = channelFactory(sp);
            return new RemoteDispatcher(channel);
        });

        // IDispatcher Registration
        services.TryAddTransient<IDispatcher>(sp => sp.GetRequiredService<RemoteDispatcher>());

        // Dispatcher Data Service Registration
        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.TryAdd(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }


    /// <summary>
    /// Adds the server dispatcher to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// The server dispatcher uses the mediator pattern to dispatch commands and queries locally.
    /// </remarks>
    public static IServiceCollection AddServerDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddTransient<IDispatcher, ServerDispatcher>();
        services.TryAddTransient<IDispatcherDataService, DispatcherDataService>();

        // Model State Open Generic Registrations
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateManager<>), typeof(ModelStateManager<>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateLoader<,>), typeof(ModelStateLoader<,>)));
        services.Add(ServiceDescriptor.Scoped(typeof(ModelStateEditor<,,>), typeof(ModelStateEditor<,,>)));

        return services;
    }
}
