using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class TrackingSettings
{
    [JsonPropertyName("click_tracking")]
    public ClickTracking ClickTracking { get; set; }

    [JsonPropertyName("open_tracking")]
    public OpenTracking OpenTracking { get; set; }

    [JsonPropertyName("subscription_tracking")]
    public SubscriptionTracking SubscriptionTracking { get; set; }

    [JsonPropertyName("ganalytics")]
    public GoogleAnalytics GoogleAnalytics { get; set; }
}
