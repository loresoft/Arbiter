using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class GoogleAnalytics
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; }

    [JsonPropertyName("utm_source")]
    public string UtmSource { get; set; }

    [JsonPropertyName("utm_medium")]
    public string UtmMedium { get; set; }

    [JsonPropertyName("utm_term")]
    public string UtmTerm { get; set; }

    [JsonPropertyName("utm_content")]
    public string UtmContent { get; set; }

    [JsonPropertyName("utm_campaign")]
    public string UtmCampaign { get; set; }
}
