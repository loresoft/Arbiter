using Grpc.Core;

namespace Arbiter.Dispatcher.Server;

public static class DispatcherMethod
{
    public const string ServiceName = "DispatcherService";
    public const string TypeHeader = "x-message-type";

    // Single generic method that handles ALL types
    public static readonly Method<byte[], byte[]> Execute = new(
        type: MethodType.Unary,
        serviceName: ServiceName,
        name: nameof(Execute),
        requestMarshaller: Marshallers.Create(
            serializer: bytes => bytes,
            deserializer: bytes => bytes
        ),
        responseMarshaller: Marshallers.Create(
            serializer: bytes => bytes,
            deserializer: bytes => bytes
        )
    );
}
