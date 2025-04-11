using System.Text.Json.Serialization;

namespace Arbiter.CommandQuery.Queries;

public class EntityPagedResult<TReadModel>
{
    [JsonPropertyName("total")]
    public long Total { get; set; }

    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<TReadModel>? Data { get; set; }
}
