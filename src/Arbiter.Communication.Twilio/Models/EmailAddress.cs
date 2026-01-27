using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class EmailAddress
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
