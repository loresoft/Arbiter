using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class Content
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}
