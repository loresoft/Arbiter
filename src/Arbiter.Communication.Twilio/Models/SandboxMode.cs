using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class SandboxMode
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }
}
