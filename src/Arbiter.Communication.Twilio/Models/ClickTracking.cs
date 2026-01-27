using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class ClickTracking
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("enable_text")]
    public bool EnableText { get; set; }
}
