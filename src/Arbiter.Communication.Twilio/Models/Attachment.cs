using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class Attachment
{
    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; }

    [JsonPropertyName("disposition")]
    public string Disposition { get; set; }

    [JsonPropertyName("content_id")]
    public string ContentId { get; set; }
}
