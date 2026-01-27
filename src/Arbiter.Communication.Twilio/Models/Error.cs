using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class Error
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("help")]
    public string Help { get; set; }
}
