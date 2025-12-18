using Arbiter.CommandQuery.Definitions;
using Arbiter.Dispatcher.Server;
using Arbiter.Mediation;

using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

using MessagePack;

using Microsoft.Extensions.Caching.Hybrid;

namespace Arbiter.Dispatcher.Client;

public class RemoteDispatcher : IDispatcher, IDisposable
{
    private readonly GrpcChannel _channel;
    private readonly CallInvoker _invoker;
    private readonly HybridCache? _hybridCache;

    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard
        .WithResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance)
        .WithCompression(MessagePackCompression.Lz4BlockArray);

    public RemoteDispatcher(string serverAddress)
    {
        var httpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

        _channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
            MaxReceiveMessageSize = 10 * 1024 * 1024,
            MaxSendMessageSize = 10 * 1024 * 1024,
        });

        _invoker = _channel.CreateCallInvoker();
    }

    public ValueTask<TResponse?> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        return Send(request, cancellationToken);
    }


    public async ValueTask<TResponse?> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // cache only if implements interface
        var cacheRequest = request as ICacheResult;
        if (_hybridCache is null || cacheRequest?.IsCacheable() != true)
            return await SendCore(request, cancellationToken).ConfigureAwait(false);

        var cacheKey = cacheRequest.GetCacheKey();
        var cacheTag = cacheRequest.GetCacheTag();
        var cacheOptions = new HybridCacheEntryOptions
        {
            Expiration = cacheRequest.SlidingExpiration(),
        };

        return await _hybridCache
            .GetOrCreateAsync(
                key: cacheKey,
                factory: async token => await SendCore(request, token).ConfigureAwait(false),
                options: cacheOptions,
                tags: string.IsNullOrEmpty(cacheTag) ? null : [cacheTag],
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<TResponse?> SendCore<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        // Single serialization - directly serialize the request
        var requestBytes = MessagePackSerializer.Serialize(request, Options, cancellationToken);

        var requestType = request.GetType();
        // Add type information to gRPC metadata
        var metadata = new Metadata
        {
            { DispatcherMethod.TypeHeader, requestType.AssemblyQualifiedName ?? requestType.FullName! },
        };

        // Call the single generic gRPC endpoint
        var callOptions = new CallOptions(headers: metadata, cancellationToken: cancellationToken);

        var responseBytes = await _invoker
            .AsyncUnaryCall(
                method: DispatcherMethod.Execute,
                host: null,
                options: callOptions,
                request: requestBytes)
            .ConfigureAwait(false);

        // Single deserialization - directly to response type
        return MessagePackSerializer.Deserialize<TResponse>(responseBytes, Options, cancellationToken);
    }

    /// <summary>
    /// Releases the resources used by the <see cref="RemoteDispatcher"/> instance.
    /// </summary>
    public void Dispose()
    {
        _channel?.Dispose();
        GC.SuppressFinalize(this);
    }
}

