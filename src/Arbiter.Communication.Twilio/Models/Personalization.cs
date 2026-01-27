using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class Personalization
{
    [JsonPropertyName("to")]
    public List<EmailAddress> To { get; set; } = [];

    [JsonPropertyName("cc")]
    public List<EmailAddress> Cc { get; set; }

    [JsonPropertyName("bcc")]
    public List<EmailAddress> Bcc { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; }

    [JsonPropertyName("substitutions")]
    public Dictionary<string, string> Substitutions { get; set; }

    [JsonPropertyName("dynamic_template_data")]
    public Dictionary<string, object> DynamicTemplateData { get; set; }

    [JsonPropertyName("custom_args")]
    public Dictionary<string, string> CustomArgs { get; set; }

    [JsonPropertyName("send_at")]
    public long? SendAt { get; set; }
}
