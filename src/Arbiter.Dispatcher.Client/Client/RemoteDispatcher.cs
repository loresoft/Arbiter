using Arbiter.CommandQuery.Definitions;
using Arbiter.Mediation;

using Google.Protobuf;

using Grpc.Core;

using MessagePack;

using Microsoft.Extensions.Caching.Hybrid;

namespace Arbiter.Dispatcher.Client;

public class RemoteDispatcher : IDispatcher
{
    public const string TypeHeader = "x-message-type";

    private readonly DispatcherRpc.DispatcherRpcClient _dispatcherClient = null!;
    private readonly HybridCache? _hybridCache;
    private readonly MessagePackSerializerOptions _options;

    public RemoteDispatcher(
        DispatcherRpc.DispatcherRpcClient dispatcherClient,
        MessagePackSerializerOptions options,
        HybridCache? hybridCache = null)
    {
        _options = options;
        _dispatcherClient = dispatcherClient;
        _hybridCache = hybridCache;
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
        var type = request.GetType();
        var requestBytes = MessagePackSerializer.Serialize(type, request, _options, cancellationToken);

        var requestType = request.GetType();
        var requestName = requestType.AssemblyQualifiedName ?? requestType.FullName!;

        // Add type information to gRPC metadata
        var metadata = new Metadata { { TypeHeader, requestName } };

        // Call the single generic gRPC endpoint
        var callOptions = new CallOptions(headers: metadata, cancellationToken: cancellationToken);

        var dispatcherRequest = new DispatcherRequest() { Payload = ByteString.CopyFrom(requestBytes) };
        var response = await _dispatcherClient.ExecuteAsync(dispatcherRequest, callOptions).ConfigureAwait(false);

        if (response == null || response.Payload == null)
            return default;

        var responseBytes = response.Payload.ToByteArray();
        if (responseBytes == null || responseBytes.Length == 0)
            return default;

        // Single deserialization - directly to response type
        return MessagePackSerializer.Deserialize<TResponse>(responseBytes, _options, cancellationToken);
    }
}

