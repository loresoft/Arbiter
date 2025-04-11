using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Converters;

namespace Arbiter.CommandQuery.Dispatcher;

public class DispatchRequest
{
    [JsonConverter(typeof(PolymorphicConverter<IRequest>))]
    public IRequest Request { get; set; } = null!;
}
