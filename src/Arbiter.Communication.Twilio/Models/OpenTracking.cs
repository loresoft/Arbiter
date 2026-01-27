using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class OpenTracking
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("substitution_tag")]
    public string SubstitutionTag { get; set; }
}
