using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class MailSettings
{
    [JsonPropertyName("bypass_list_management")]
    public BypassListManagement BypassListManagement { get; set; }

    [JsonPropertyName("footer")]
    public Footer Footer { get; set; }

    [JsonPropertyName("sandbox_mode")]
    public SandboxMode SandboxMode { get; set; }
}
