using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Converters;

namespace Arbiter.CommandQuery.Dispatcher;

/// <summary>
/// A request to be dispatched.
/// </summary>
public class DispatchRequest
{
    /// <summary>
    /// The request to be dispatched.
    /// </summary>
    [JsonPropertyName("request")]
    [JsonConverter(typeof(PolymorphicConverter<object>))]
    public object Request { get; set; } = null!;
}
