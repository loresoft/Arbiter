using System.Text.Json.Serialization;

namespace Arbiter.Communication.Twilio.Models;

public class Asm
{
    [JsonPropertyName("group_id")]
    public int GroupId { get; set; }

    [JsonPropertyName("groups_to_display")]
    public List<int> GroupsToDisplay { get; set; }
}
