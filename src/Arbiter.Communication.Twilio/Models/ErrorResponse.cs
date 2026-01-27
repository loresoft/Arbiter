using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

// Error Response
public class ErrorResponse
{
    [JsonPropertyName("errors")]
    public List<Error> Errors { get; set; }
}
