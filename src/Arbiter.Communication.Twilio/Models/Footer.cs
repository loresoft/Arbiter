using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class Footer
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("html")]
    public string Html { get; set; }
}
