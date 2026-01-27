using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

// Mail Send Request https://raw.githubusercontent.com/twilio/sendgrid-oai/refs/heads/main/spec/yaml/tsg_mail_v3.yaml
public class MailSendRequest
{
    [JsonPropertyName("personalizations")]
    public List<Personalization> Personalizations { get; set; } = [];

    [JsonPropertyName("from")]
    public EmailAddress From { get; set; }

    [JsonPropertyName("reply_to")]
    public EmailAddress ReplyTo { get; set; }

    [JsonPropertyName("reply_to_list")]
    public List<EmailAddress> ReplyToList { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("content")]
    public List<Content> Content { get; set; } = [];

    [JsonPropertyName("attachments")]
    public List<Attachment> Attachments { get; set; }

    [JsonPropertyName("template_id")]
    public string TemplateId { get; set; }

    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; }

    [JsonPropertyName("categories")]
    public List<string> Categories { get; set; }

    [JsonPropertyName("custom_args")]
    public Dictionary<string, string> CustomArgs { get; set; }

    [JsonPropertyName("send_at")]
    public long? SendAt { get; set; }

    [JsonPropertyName("batch_id")]
    public string BatchId { get; set; }

    [JsonPropertyName("asm")]
    public Asm Asm { get; set; }

    [JsonPropertyName("ip_pool_name")]
    public string IpPoolName { get; set; }

    [JsonPropertyName("mail_settings")]
    public MailSettings MailSettings { get; set; }

    [JsonPropertyName("tracking_settings")]
    public TrackingSettings TrackingSettings { get; set; }
}
