using System.Text.Json;
using System.Text.Json.Serialization;

using Arbiter.CommandQuery.Converters;

namespace Tracker.WebService.Services;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddDomainOptions(this JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        options.Converters.Add(new ClaimsPrincipalConverter());
        options.Converters.Add(new PolymorphicConverter<object>());

        return options;
    }
}
