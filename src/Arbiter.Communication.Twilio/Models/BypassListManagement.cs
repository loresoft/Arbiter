using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class BypassListManagement
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }
}
