using System.Text.Json.Serialization;

namespace Arbiter.Messaging.WebPubSub.Cache;

/// <summary>
/// Provides source-generated JSON metadata for <see cref="CacheExpireMessage"/>.
/// </summary>
/// <remarks>
/// Use this context to serialize and deserialize cache expiration messages without runtime reflection,
/// which keeps the messaging pipeline trimming and native AOT compatible.
/// </remarks>
[JsonSerializable(typeof(CacheExpireMessage))]
public partial class CacheExpireSerializerContext : JsonSerializerContext;
